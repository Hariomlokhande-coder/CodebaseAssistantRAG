using CodebaseAssistant.Application.DTOs;

namespace CodebaseAssistant.Application.Interfaces;

public interface IChatService
{
    Task<ChatResponse> AskAsync(
        Guid repositoryId,
        string question,
        int topK = 5,
        CancellationToken cancellationToken = default);
}
