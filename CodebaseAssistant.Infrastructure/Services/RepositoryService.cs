using CodebaseAssistant.Application.Interfaces;
using Microsoft.AspNetCore.Http;
using System.IO.Compression;
using CodebaseAssistant.Domain.Entities;
using CodebaseAssistant.Domain.Enums;
using CodebaseAssistant.Infrastructure.Persistence;

namespace CodebaseAssistant.Infrastructure.Services;

public class RepositoryService : IRepositoryService
{
    public async Task<Guid> UploadRepositoryAsync(IFormFile file)
    {
        if (file == null || file.Length == 0)
            throw new ArgumentException("File is required.");

        if (!Path.GetExtension(file.FileName)
                 .Equals(".zip", StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException("Only ZIP files are allowed.");
        }

        const long maxFileSize = 100 * 1024 * 1024; // 100 MB

        if (file.Length > maxFileSize)
        {
            throw new ArgumentException(
                "File size cannot exceed 100 MB.");
        }

        var repositoryId = Guid.NewGuid();

        var uploadRoot = Path.Combine(
            Directory.GetCurrentDirectory(),
            "Uploads");

        Directory.CreateDirectory(uploadRoot);

        var repositoryFolder = Path.Combine(
            uploadRoot,
            repositoryId.ToString());

        Directory.CreateDirectory(repositoryFolder);

        var zipPath = Path.Combine(
            repositoryFolder,
            "repository.zip");

        using (var stream = new FileStream(zipPath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        try
        {
            var extractPath = Path.Combine(
                repositoryFolder,
                "Source");

            ZipFile.ExtractToDirectory(
                zipPath,
                extractPath);
        }
        catch (InvalidDataException)
        {
            throw new ArgumentException(
                "Corrupted ZIP file.");
        }

        return repositoryId;
    }
}