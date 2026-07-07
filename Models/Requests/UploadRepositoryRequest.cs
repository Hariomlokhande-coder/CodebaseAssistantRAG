using Microsoft.AspNetCore.Http;

namespace CodebaseAssistant.Api.Models.Requests;

public class UploadRepositoryRequest
{
    public IFormFile File { get; set; } = default!;
}


