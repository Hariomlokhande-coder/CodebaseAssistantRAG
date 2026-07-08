using CodebaseAssistant.Api.Models.Requests;
using CodebaseAssistant.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
//using Microsoft.EntityFrameworkCore.Query.Internal;
using CodebaseAssistant.Api.Models.Requests;
//using CodebaseAssistant.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CodebaseAssistant.Api.Controllers;

[ApiController]
[Route("api/repository")]
public class RepositoryController : ControllerBase
{
    private readonly IRepositoryService _repositoryService;
    private readonly IIndexingService _indexingService;

    public RepositoryController(
        IRepositoryService repositoryService,
        IIndexingService indexingService)
    {
        _repositoryService = repositoryService;
        _indexingService = indexingService;
    }

    [HttpPost("upload")]
    public async Task<IActionResult> Upload(
        [FromForm] UploadRepositoryRequest request)
    {
        if (request.File == null || request.File.Length == 0)
        {
            return BadRequest(
                "Please upload a repository zip file.");
        }

        var repositoryId =
            await _repositoryService
                .UploadRepositoryAsync(request.File);

        return Ok(new
        {
            RepositoryId = repositoryId,
            Message = "Repository uploaded successfully."
        });
    }

    [HttpPost("index/{repositoryId:guid}")]
    public async Task<IActionResult> IndexRepository(
        Guid repositoryId)
    {
        await _indexingService
            .IndexRepositoryAsync(repositoryId);

        return Ok(new
        {
            Message = "Repository indexing started.",
            RepositoryId = repositoryId
        });
    }
}