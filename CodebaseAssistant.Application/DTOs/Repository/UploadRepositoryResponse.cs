using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodebaseAssistant.Application.DTOs.Repository;

public class UploadRepositoryResponse
{
    public Guid RepositoryId { get; set; }

    public string RepositoryName { get; set; } = string.Empty;

    public string RepositoryPath { get; set; } = string.Empty;

    public string Message { get; set; } = string.Empty;
}