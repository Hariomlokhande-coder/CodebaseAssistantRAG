using CodebaseAssistant.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
namespace CodebaseAssistant.Api.Controllers;

[ApiController]
[Route("api/embedding")]
public class EmbeddingController : ControllerBase
{
    private readonly IEmbeddingService _embeddingService;

    public EmbeddingController(
        IEmbeddingService embeddingService)
    {
        _embeddingService = embeddingService;
    }

    [HttpGet("test")]
    public async Task<IActionResult> Test()
    {
        var embedding =
            await _embeddingService
                .GenerateEmbeddingAsync(
                    "Hello World");

        return Ok(new
        {
            VectorLength = embedding.Length
        });
    }
}