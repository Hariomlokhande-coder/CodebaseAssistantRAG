using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using CodebaseAssistant.Application.DTOs;
using CodebaseAssistant.Application.Interfaces;
using CodebaseAssistant.Infrastructure.Configuration;
using CodebaseAssistant.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace CodebaseAssistant.Infrastructure.Services;

public sealed class ChatService : IChatService
{
    private const string CollectionName = "code_chunks";
    private readonly HttpClient _httpClient;
    private readonly CodebaseAssistantDbContext _dbContext;
    private readonly IEmbeddingService _embeddingService;
    private readonly IVectorStore _vectorStore;
    private readonly OpenAiOptions _openAi;
    private readonly ChatOptions _chat;

    public ChatService(
        HttpClient httpClient,
        CodebaseAssistantDbContext dbContext,
        IEmbeddingService embeddingService,
        IVectorStore vectorStore,
        IOptions<OpenAiOptions> openAi,
        IOptions<ChatOptions> chat)
    {
        _httpClient = httpClient;
        _dbContext = dbContext;
        _embeddingService = embeddingService;
        _vectorStore = vectorStore;
        _openAi = openAi.Value;
        _chat = chat.Value;
    }

    public async Task<ChatResponse> AskAsync(
        Guid repositoryId,
        string question,
        int topK = 5,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(question);
        if (topK is < 1 or > 20)
            throw new ArgumentOutOfRangeException(nameof(topK), "TopK must be between 1 and 20.");
        if (string.IsNullOrWhiteSpace(_openAi.ApiKey))
            throw new InvalidOperationException("OpenAI:ApiKey is not configured.");

        var repositoryExists = await _dbContext.Repositories
            .AnyAsync(x => x.Id == repositoryId, cancellationToken);
        if (!repositoryExists)
            throw new KeyNotFoundException($"Repository not found: {repositoryId}");

        var queryVector = await _embeddingService.GenerateEmbeddingAsync(question, cancellationToken);
        var matches = await _vectorStore.SearchAsync(
            CollectionName, queryVector, topK, repositoryId, cancellationToken);

        var chunkIds = matches.Select(x => x.ChunkId).ToArray();
        var chunks = await _dbContext.CodeChunks
            .Where(x => chunkIds.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, cancellationToken);

        var context = new StringBuilder();
        var sources = new List<ChatSource>();
        foreach (var match in matches)
        {
            if (!chunks.TryGetValue(match.ChunkId, out var chunk))
                continue;

            var section = $"\n--- {chunk.FilePath}:{chunk.StartLine}-{chunk.EndLine} ---\n{chunk.Content}\n";
            if (context.Length + section.Length > _chat.MaxContextCharacters)
                break;

            context.Append(section);
            sources.Add(new ChatSource(
                chunk.FilePath, chunk.ClassName, chunk.MethodName,
                chunk.StartLine, chunk.EndLine, match.Score));
        }

        if (sources.Count == 0)
            return new ChatResponse("I could not find relevant indexed code for this question.", sources);

        _httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", _openAi.ApiKey);

        var payload = new
        {
            model = _chat.Model,
            messages = new object[]
            {
                new { role = "system", content = "Answer only from the supplied code context. Be concise, explain uncertainty, and cite files as [path:line-line]." },
                new { role = "user", content = $"Question: {question}\n\nCode context:{context}" }
            }
        };

        using var response = await _httpClient.PostAsJsonAsync(
            $"{_openAi.BaseUrl.TrimEnd('/')}/chat/completions", payload, cancellationToken);
        var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
        if (!response.IsSuccessStatusCode)
            throw new HttpRequestException($"Chat provider returned {(int)response.StatusCode}: {responseBody}");

        using var json = JsonDocument.Parse(responseBody);
        var answer = json.RootElement.GetProperty("choices")[0]
            .GetProperty("message").GetProperty("content").GetString();

        return new ChatResponse(answer ?? "The model returned an empty answer.", sources);
    }
}
