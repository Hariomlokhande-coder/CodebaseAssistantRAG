namespace CodebaseAssistant.Infrastructure.Configuration;

public sealed class UploadSettings
{
    public string? RepositoryPath { get; set; }
    public int MaxFileSizeMb { get; set; } = 100;
}
