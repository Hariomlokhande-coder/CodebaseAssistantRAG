using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;

namespace CodebaseAssistant.Application.Interfaces;

public interface IRepositoryService
{
    Task<Guid> UploadRepositoryAsync(IFormFile file);
}