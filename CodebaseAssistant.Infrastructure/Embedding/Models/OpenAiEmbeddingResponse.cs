namespace CodebaseAssistant.Infrastructure.Embedding.Models;

public class OpenAiEmbeddingResponse
{
    public List<EmbeddingData> Data { get; set; } = new();
}

public class EmbeddingData
{
    public List<float> Embedding { get; set; } = new();
}


