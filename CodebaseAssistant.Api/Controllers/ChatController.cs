using CodebaseAssistant.Api.Models.Requests;
using CodebaseAssistant.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CodebaseAssistant.Api.Controllers;

[ApiController]
[Route("api/chat")]
public sealed class ChatController : ControllerBase
{
    [HttpPost("query")]
    public async Task<IActionResult> Query(
        [FromBody] ChatRequest request,
        [FromServices] IChatService chatService,
        CancellationToken cancellationToken)
    {
        if (request.RepositoryId == Guid.Empty)
            return BadRequest("RepositoryId is required.");
        if (string.IsNullOrWhiteSpace(request.Question))
            return BadRequest("Question is required.");

        var response = await chatService.AskAsync(
            request.RepositoryId, request.Question, request.TopK, cancellationToken);
        return Ok(response);
    }
}
