namespace CodebaseAssistant.Api.Models.Requests;

public sealed class ChatRequest
{
    public Guid RepositoryId { get; set; }
    public string Question { get; set; } = string.Empty;
    public int TopK { get; set; } = 5;
}
