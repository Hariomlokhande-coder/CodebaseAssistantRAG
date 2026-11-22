namespace CodebaseAssistant.Infrastructure.Roslyn;

public sealed class ParsedCodeItem
{
    public string FilePath { get; init; } = string.Empty;
    public string Namespace { get; init; } = string.Empty;
    public string ClassName { get; init; } = string.Empty;
    public string MethodName { get; init; } = string.Empty;
    public string Code { get; init; } = string.Empty;
    public string MemberType { get; init; } = string.Empty;
    public int StartLine { get; init; }
    public int EndLine { get; init; }
}
