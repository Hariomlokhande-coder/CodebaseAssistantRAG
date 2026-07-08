using CodebaseAssistant.Application.Interfaces;
using CodebaseAssistant.Domain.Entities;
using CodebaseAssistant.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System.IO;

namespace CodebaseAssistant.Infrastructure.Services
{
    public class IndexingService : IIndexingService
    {
        private readonly CodebaseAssistantDbContext _dbContext;

        public IndexingService(
            CodebaseAssistantDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task IndexRepositoryAsync(
            Guid repositoryId)
        {
            Repository? repository =
                await _dbContext.Repositories
                    .FirstOrDefaultAsync(x =>
                        x.Id == repositoryId);

            if (repository is null)
            {
                throw new Exception(
                    $"Repository not found: {repositoryId}");
            }

            string repositoryPath =
                repository.Path;

            if (string.IsNullOrWhiteSpace(repositoryPath))
            {
                throw new Exception(
                    $"Repository path is empty for {repositoryId}");
            }

            if (!Directory.Exists(repositoryPath))
            {
                throw new DirectoryNotFoundException(
                    $"Repository folder not found: {repositoryPath}");
            }

            string[] codeFiles =
                Directory.GetFiles(
                    repositoryPath,
                    "*.cs",
                    SearchOption.AllDirectories);

            if (codeFiles.Length == 0)
            {
                throw new Exception(
                    $"No C# files found in {repositoryPath}");
            }

            var existingChunks =
    _dbContext.CodeChunks
        .Where(x => x.RepositoryId == repositoryId);

            _dbContext.CodeChunks.RemoveRange(
                existingChunks);

            await _dbContext.SaveChangesAsync();

            foreach (string filePath in codeFiles)
            {
                string content =
                    await File.ReadAllTextAsync(filePath);

                CodeChunk chunk = new()
                {
                    RepositoryId = repositoryId,
                    FilePath = filePath,
                    Content = content,
                    StartLine = 1,
                    EndLine = content.Split('\n').Length,
                    Namespace = string.Empty,
                    ClassName = string.Empty,
                    MethodName = string.Empty,
                    ChunkType = "File",
                    ContentHash = string.Empty
                };

                _dbContext.CodeChunks.Add(chunk);
            }

            await _dbContext.SaveChangesAsync();
        }
    }
}