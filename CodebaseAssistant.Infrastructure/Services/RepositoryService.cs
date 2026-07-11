using CodebaseAssistant.Application.Interfaces;
using CodebaseAssistant.Infrastructure.Persistence;
using CodebaseAssistant.Infrastructure.Roslyn;
using Microsoft.AspNetCore.Http;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;
using CodebaseAssistant.Domain.Entities;
using CodebaseAssistant.Domain.Enums;

namespace CodebaseAssistant.Infrastructure.Services;

public class RepositoryService : IRepositoryService
{
    private readonly CodebaseAssistantDbContext _dbContext;
    private readonly ICodeParser _codeParser;

    public RepositoryService(
        CodebaseAssistantDbContext dbContext,
        ICodeParser codeParser)
    {
        _dbContext = dbContext;
        _codeParser = codeParser;
    }

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

            // Compute hash of the zip file
            string hash;
            using (var sha = SHA256.Create())
            using (var fs = File.OpenRead(zipPath))
            {
                var bytes = sha.ComputeHash(fs);
                hash = BitConverter.ToString(bytes).Replace("-", "").ToLowerInvariant();
            }

            var repoName = Path.GetFileNameWithoutExtension(file.FileName);

            var fileInfo = new FileInfo(zipPath);

            var repository = new Repository
            {
                Id = repositoryId,
                Name = repoName ?? repositoryId.ToString(),
                Path = Path.GetFullPath(Path.Combine(repositoryFolder, "Source")),
                Hash = hash,
                CreatedAt = DateTime.UtcNow,
                UploadedAt = DateTime.UtcNow,
                Status = RepositoryStatus.Uploaded,
                SizeInBytes = fileInfo.Length,
                Description = null
            };

            _dbContext.Repositories.Add(repository);
            await _dbContext.SaveChangesAsync();

            return repositoryId;
        }
        catch (InvalidDataException)
        {
            throw new ArgumentException(
                "Corrupted ZIP file.");
        }
    }
}