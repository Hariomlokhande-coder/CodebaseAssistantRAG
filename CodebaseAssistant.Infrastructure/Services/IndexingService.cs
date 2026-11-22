using System.Security.Cryptography;
using System.Text;
using CodebaseAssistant.Application.Interfaces;
using CodebaseAssistant.Domain.Entities;
using CodebaseAssistant.Domain.Enums;
using CodebaseAssistant.Infrastructure.Persistence;
using CodebaseAssistant.Infrastructure.Roslyn;
using Microsoft.EntityFrameworkCore;

namespace CodebaseAssistant.Infrastructure.Services;

public sealed class IndexingService : IIndexingService
{
    private const string CollectionName = "code_chunks";
    private static readonly string[] Extensions = [".cs", ".csproj", ".js", ".ts", ".py", ".sql", ".md", ".json"];
    private static readonly string[] IgnoredFolders = ["bin", "obj", ".git", "node_modules", ".vs"];

    private readonly CodebaseAssistantDbContext _dbContext;
    private readonly ICodeParser _codeParser;
    private readonly IEmbeddingService _embeddingService;
    private readonly IVectorStore _vectorStore;

    public IndexingService(
        CodebaseAssistantDbContext dbContext,
        ICodeParser codeParser,
        IEmbeddingService embeddingService,
        IVectorStore vectorStore)
    {
        _dbContext = dbContext;
        _codeParser = codeParser;
        _embeddingService = embeddingService;
        _vectorStore = vectorStore;
    }

    public async Task IndexRepositoryAsync(Guid repositoryId)
    {
        var repository = await _dbContext.Repositories.FindAsync(repositoryId)
            ?? throw new KeyNotFoundException($"Repository not found: {repositoryId}");

        if (!Directory.Exists(repository.Path))
            throw new DirectoryNotFoundException(repository.Path);

        repository.Status = RepositoryStatus.Indexing;
        await _dbContext.SaveChangesAsync();

        try
        {
            var oldChunks = await _dbContext.CodeChunks
                .Where(x => x.RepositoryId == repositoryId)
                .ToListAsync();
            _dbContext.CodeChunks.RemoveRange(oldChunks);

            foreach (var file in GetFiles(repository.Path))
                _dbContext.CodeChunks.AddRange(await CreateChunks(repositoryId, repository.Path, file));

            await _dbContext.SaveChangesAsync();

            var chunks = await _dbContext.CodeChunks
                .Where(x => x.RepositoryId == repositoryId)
                .ToListAsync();

            if (chunks.Count > 0)
            {
                var firstVector = await _embeddingService.GenerateEmbeddingAsync(chunks[0].Content);
                await _vectorStore.CreateCollectionAsync(CollectionName, firstVector.Length);

                foreach (var chunk in chunks)
                {
                    var vector = await _embeddingService.GenerateEmbeddingAsync(chunk.Content);
                    await _vectorStore.UpsertAsync(CollectionName, chunk.Id, vector, Metadata(chunk));
                }
            }

            repository.Status = RepositoryStatus.Indexed;
            await _dbContext.SaveChangesAsync();
        }
        catch
        {
            repository.Status = RepositoryStatus.Failed;
            await _dbContext.SaveChangesAsync();
            throw;
        }
    }

    private static IEnumerable<string> GetFiles(string root) =>
        Directory.EnumerateFiles(root, "*.*", SearchOption.AllDirectories)
            .Where(file => Extensions.Contains(Path.GetExtension(file), StringComparer.OrdinalIgnoreCase))
            .Where(file => !Path.GetRelativePath(root, file)
                .Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
                .Any(part => IgnoredFolders.Contains(part, StringComparer.OrdinalIgnoreCase)));

    private async Task<List<CodeChunk>> CreateChunks(Guid repositoryId, string root, string file)
    {
        if (Path.GetExtension(file).Equals(".cs", StringComparison.OrdinalIgnoreCase))
        {
            return (await _codeParser.ParseAsync(file)).Select(item => new CodeChunk
            {
                RepositoryId = repositoryId,
                FilePath = Path.GetRelativePath(root, file),
                Namespace = item.Namespace,
                ClassName = item.ClassName,
                MethodName = item.MethodName,
                Content = item.Code,
                StartLine = item.StartLine,
                EndLine = item.EndLine,
                ChunkType = item.MemberType,
                ContentHash = Hash(item.Code)
            }).ToList();
        }

        var content = await File.ReadAllTextAsync(file);
        return
        [
            new CodeChunk
            {
                RepositoryId = repositoryId,
                FilePath = Path.GetRelativePath(root, file),
                Content = content,
                StartLine = 1,
                EndLine = content.Count(c => c == '\n') + 1,
                ChunkType = "File",
                ContentHash = Hash(content)
            }
        ];
    }

    private static Dictionary<string, object> Metadata(CodeChunk chunk) => new()
    {
        ["RepositoryId"] = chunk.RepositoryId,
        ["ChunkId"] = chunk.Id,
        ["FilePath"] = chunk.FilePath,
        ["Namespace"] = chunk.Namespace,
        ["ClassName"] = chunk.ClassName,
        ["MethodName"] = chunk.MethodName,
        ["ChunkType"] = chunk.ChunkType
    };

    private static string Hash(string content) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(content))).ToLowerInvariant();
}
