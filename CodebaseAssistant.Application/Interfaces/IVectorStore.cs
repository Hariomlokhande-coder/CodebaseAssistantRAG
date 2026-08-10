using CodebaseAssistant.Application.DTOs;

namespace CodebaseAssistant.Application.Interfaces;

/// <summary>
/// Defines operations for managing vector data in a vector database.
/// Used to create collections, store/update embeddings,
/// search vectors, and delete vectors.
/// </summary>
public interface IVectorStore
{
    Task CreateCollectionAsync(
        string collectionName,
        int vectorSize,
        CancellationToken cancellationToken = default);

    Task UpsertAsync(
        string collectionName,
        Guid pointId,
        float[] vector,
        Dictionary<string, object> metadata,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<VectorSearchResult>> SearchAsync(
        string collectionName,
        float[] vector,
        int topK,
        Guid repositoryId,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(
        string collectionName,
        Guid pointId,
        CancellationToken cancellationToken = default);
}