namespace CodebaseAssistant.Infrastructure.Embedding.Models;

public class OpenAiEmbeddingRequest
{
    public string Model { get; set; } = string.Empty;

    public string Input { get; set; } = string.Empty;
}