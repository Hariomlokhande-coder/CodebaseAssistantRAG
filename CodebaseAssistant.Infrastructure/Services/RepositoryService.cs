using CodebaseAssistant.Application.Interfaces;
using CodebaseAssistant.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;
using CodebaseAssistant.Domain.Entities;
using CodebaseAssistant.Domain.Enums;
using CodebaseAssistant.Infrastructure.Configuration;
using Microsoft.Extensions.Options;

namespace CodebaseAssistant.Infrastructure.Services;

public class RepositoryService : IRepositoryService
{
    private readonly CodebaseAssistantDbContext _dbContext;
    private readonly UploadSettings _uploadSettings;

    public RepositoryService(
        CodebaseAssistantDbContext dbContext,
        IOptions<UploadSettings> uploadOptions)
    {
        _dbContext = dbContext;
        _uploadSettings = uploadOptions.Value;
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

        var maxFileSizeMb = _uploadSettings.MaxFileSizeMb > 0 ? _uploadSettings.MaxFileSizeMb : 100;
        var maxFileSize = (long)maxFileSizeMb * 1024 * 1024;

        if (file.Length > maxFileSize)
        {
            throw new ArgumentException($"File size cannot exceed {maxFileSizeMb} MB.");
        }

        var repositoryId = Guid.NewGuid();

        var uploadRoot = !string.IsNullOrWhiteSpace(_uploadSettings.RepositoryPath)
            ? _uploadSettings.RepositoryPath
            : Path.Combine(Directory.GetCurrentDirectory(), "Uploads");

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

            Directory.CreateDirectory(extractPath);

            // Safe extraction to prevent Zip Slip
            using (var archive = new System.IO.Compression.ZipArchive(File.OpenRead(zipPath)))
            {
                foreach (var entry in archive.Entries)
                {
                    if (string.IsNullOrWhiteSpace(entry.FullName))
                        continue;

                    // Normalize entry path
                    var entryPath = Path.GetFullPath(Path.Combine(extractPath, entry.FullName));

                    if (!entryPath.StartsWith(Path.GetFullPath(extractPath), StringComparison.OrdinalIgnoreCase))
                    {
                        throw new InvalidDataException("Zip contains invalid entries.");
                    }

                    if (entry.FullName.EndsWith("/") || entry.FullName.EndsWith("\\"))
                    {
                        Directory.CreateDirectory(entryPath);
                        continue;
                    }

                    Directory.CreateDirectory(Path.GetDirectoryName(entryPath) ?? extractPath);

                    using var entryStream = entry.Open();
                    using var outStream = File.Create(entryPath);
                    await entryStream.CopyToAsync(outStream);
                }
            }

            // Compute hash of the zip file
            string hash;
            using (var sha = SHA256.Create())
            using (var fs = File.OpenRead(zipPath))
            {
                var bytes = sha.ComputeHash(fs);
                hash = BitConverter.ToString(bytes).Replace("-", "").ToLowerInvariant();
            }

            // Detect duplicate repository by hash
            var existing = await _dbContext.Repositories.FirstOrDefaultAsync(r => r.Hash == hash);
            if (existing != null)
            {
                // Cleanup new upload to save space
                try
                {
                    if (Directory.Exists(repositoryFolder))
                        Directory.Delete(repositoryFolder, true);
                }
                catch { }

                // Return the existing repository id instead of creating a duplicate
                return existing.Id;
            }

            var repoName = Path.GetFileNameWithoutExtension(file.FileName);

            var fileInfo = new FileInfo(zipPath);

            // Check for empty extraction
            var filesExtracted = Directory.EnumerateFiles(extractPath, "*.*", SearchOption.AllDirectories).Any();
            if (!filesExtracted)
            {
                throw new ArgumentException("The ZIP archive is empty.");
            }

            var repository = new Repository
            {
                Id = repositoryId,
                Name = repoName ?? repositoryId.ToString(),
                Path = Path.GetFullPath(extractPath),
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
                "Corrupted or invalid ZIP file.");
        }
    }
}
