namespace CodebaseAssistant.Infrastructure.Configuration;

public class OpenAiOptions
{
    public string ApiKey { get; set; } = string.Empty;

    public string BaseUrl { get; set; }
        = "https://api.openai.com/v1";

    public string Model { get; set; }
        = "text-embedding-3-small";

    public int TimeoutSeconds { get; set; } = 30;
}