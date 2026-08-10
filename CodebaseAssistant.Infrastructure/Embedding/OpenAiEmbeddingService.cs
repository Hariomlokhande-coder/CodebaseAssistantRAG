using CodebaseAssistant.Application.Interfaces;
using CodebaseAssistant.Infrastructure.Configuration;
using CodebaseAssistant.Infrastructure.Embedding.Models;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
namespace CodebaseAssistant.Infrastructure.Embedding;
public class OpenAiEmbeddingService : IEmbeddingService
{
    private readonly HttpClient _httpClient;
    private readonly OpenAiOptions _options;

    public OpenAiEmbeddingService(
        HttpClient httpClient,
        IOptions<OpenAiOptions> options)
    {
        _httpClient = httpClient;
        _options = options.Value;
    }

    public async Task<float[]> GenerateEmbeddingAsync(
        string text,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(text))
            throw new ArgumentException(
                "Text cannot be empty.",
                nameof(text));

        _httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                _options.ApiKey);

        var request = new OpenAiEmbeddingRequest
        {
            Model = _options.Model,
            Input = text
        };

        var json = JsonSerializer.Serialize(request);

        var content = new StringContent(
            json,
            Encoding.UTF8,
            "application/json");
        var response = await _httpClient.PostAsync(
            $"{_options.BaseUrl}/embeddings",
            content,
            cancellationToken);

        response.EnsureSuccessStatusCode();

        var responseJson =
            await response.Content.ReadAsStringAsync(
                cancellationToken);

        var embeddingResponse =
            JsonSerializer.Deserialize<OpenAiEmbeddingResponse>(
                responseJson,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

        if (embeddingResponse == null ||
            embeddingResponse.Data.Count == 0)
        {
            throw new Exception(
                "Failed to generate embedding.");
        }

        return embeddingResponse
            .Data[0]
            .Embedding
            .ToArray();
    }
}