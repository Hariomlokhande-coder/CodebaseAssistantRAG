using System.ComponentModel.DataAnnotations;

namespace CodebaseAssistant.Infrastructure.Configuration;

public sealed class QdrantOptions
{
    [Required]
    [Url]
    public string BaseUrl { get; set; } = string.Empty;

    public string ApiKey { get; set; } = string.Empty;
}