namespace CodebaseAssistant.Api.Models.Requests;

public class QueryRequest
{
    public string Query { get; set; } = string.Empty;

    public Guid RepositoryId { get; set; }

    public int TopK { get; set; } = 5;
}
