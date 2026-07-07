using CodebaseAssistant.Api.Models.Requests;
using CodebaseAssistant.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CodebaseAssistant.Api.Controllers;

[ApiController]
[Route("api/repository")]
public class RepositoryController : ControllerBase
{
    private readonly IRepositoryService _repositoryService;

    public RepositoryController(IRepositoryService repositoryService)
    {
        _repositoryService = repositoryService;
    }


    [HttpPost("upload")]
    public async Task<IActionResult> Upload([FromForm] UploadRepositoryRequest request)
    {
        if (request.File == null || request.File.Length == 0)
            return BadRequest("Please upload a repository zip file.");

        var repositoryId = await _repositoryService.UploadRepositoryAsync(request.File);

        return Ok(new
        {
            RepositoryId = repositoryId,
            Message = "Repository uploaded successfully."
        });
    }
}