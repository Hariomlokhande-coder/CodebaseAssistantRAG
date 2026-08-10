namespace CodebaseAssistant.Application.DTOs;

public sealed class VectorSearchResult
{
    public Guid PointId { get; init; }

    public float Score { get; init; }

    public Guid RepositoryId { get; init; }

    public Guid ChunkId { get; init; }

    public string FilePath { get; init; } = string.Empty;

    public string Namespace { get; init; } = string.Empty;

    public string ClassName { get; init; } = string.Empty;

    public string MethodName { get; init; } = string.Empty;

    public string ChunkType { get; init; } = string.Empty;
}