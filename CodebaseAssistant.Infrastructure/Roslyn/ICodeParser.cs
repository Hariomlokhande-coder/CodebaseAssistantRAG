namespace CodebaseAssistant.Infrastructure.Roslyn;

public interface ICodeParser
{
    Task<List<ParsedCodeItem>> ParseAsync(string filePath);
}