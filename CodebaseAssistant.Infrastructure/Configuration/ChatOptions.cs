namespace CodebaseAssistant.Infrastructure.Configuration;

public sealed class ChatOptions
{
    public string Model { get; set; } = "gpt-4.1-mini";
    public int MaxContextCharacters { get; set; } = 24000;
}
