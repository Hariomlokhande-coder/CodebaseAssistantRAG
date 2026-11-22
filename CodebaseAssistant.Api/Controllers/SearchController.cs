using CodebaseAssistant.Api.Models.Requests;
using CodebaseAssistant.Application.DTOs;
using CodebaseAssistant.Application.Interfaces;
using CodebaseAssistant.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CodebaseAssistant.Api.Controllers;

[ApiController]
[Route("api/search")]
public class SearchController : ControllerBase
{
    private readonly CodebaseAssistantDbContext _dbContext;
    private readonly IEmbeddingService _embeddingService;
    private readonly IVectorStore _vectorStore;

    public SearchController(
        CodebaseAssistantDbContext dbContext,
        IEmbeddingService embeddingService,
        IVectorStore vectorStore)
    {
        _dbContext = dbContext;
        _embeddingService = embeddingService;
        _vectorStore = vectorStore;
    }

    [HttpPost]
    public async Task<IActionResult> Query([FromBody] QueryRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Query))
            return BadRequest("Query cannot be empty.");

        if (request.RepositoryId == Guid.Empty)
            return BadRequest("RepositoryId is required.");

        var topK = request.TopK <= 0 ? 5 : request.TopK;
        if (topK > 20)
            return BadRequest("TopK cannot exceed 20.");

        var embedding = await _embeddingService.GenerateEmbeddingAsync(request.Query, cancellationToken);

        var results = await _vectorStore.SearchAsync(
            collectionName: "code_chunks",
            vector: embedding,
            topK: topK,
            repositoryId: request.RepositoryId,
            cancellationToken: cancellationToken);

        // Fetch code chunk contents from SQL for richer response
        var chunkIds = results.Select(r => r.ChunkId).Where(id => id != Guid.Empty).ToList();

        var chunks = await _dbContext.CodeChunks
            .Where(c => chunkIds.Contains(c.Id))
            .ToDictionaryAsync(c => c.Id, c => c, cancellationToken);

        var response = results.Select(r => new
        {
            r.PointId,
            r.Score,
            r.RepositoryId,
            r.ChunkId,
            r.FilePath,
            r.Namespace,
            r.ClassName,
            r.MethodName,
            r.ChunkType,
            Content = chunks.TryGetValue(r.ChunkId, out var c) ? c.Content : string.Empty,
            StartLine = chunks.TryGetValue(r.ChunkId, out var c2) ? c2.StartLine : 0,
            EndLine = chunks.TryGetValue(r.ChunkId, out var c3) ? c3.EndLine : 0
        }).ToList();

        return Ok(new { Results = response });
    }
}
