namespace CodebaseAssistant.Application.DTOs;

public sealed record ChatSource(
    string FilePath,
    string? ClassName,
    string? MethodName,
    int StartLine,
    int EndLine,
    float Score);

public sealed record ChatResponse(
    string Answer,
    IReadOnlyList<ChatSource> Sources);
