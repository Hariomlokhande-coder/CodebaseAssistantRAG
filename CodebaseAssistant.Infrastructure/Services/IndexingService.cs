using CodebaseAssistant.Application.Interfaces;
using CodebaseAssistant.Domain.Entities;
using CodebaseAssistant.Domain.Enums;
using CodebaseAssistant.Infrastructure.Persistence;
using CodebaseAssistant.Infrastructure.Roslyn;
using Microsoft.EntityFrameworkCore;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace CodebaseAssistant.Infrastructure.Services
{
    public class IndexingService : IIndexingService
    {
        private readonly CodebaseAssistantDbContext _dbContext;
        private readonly ICodeParser _codeParser;

        private static readonly string[] SupportedExtensions = new[]
        {
            ".cs", ".csproj", ".js", ".ts", ".py", ".sql", ".md", ".json"
        };

        private static readonly string[] IgnoredFolders = new[]
        {
            "bin", "obj", ".git", "node_modules", ".vs"
        };

        public IndexingService(
            CodebaseAssistantDbContext dbContext,
            ICodeParser codeParser)
        {
            _dbContext = dbContext;
            _codeParser = codeParser;
        }

        public async Task IndexRepositoryAsync(Guid repositoryId)
        {
            Repository? repository =
                await _dbContext.Repositories
                    .FirstOrDefaultAsync(x => x.Id == repositoryId);

            if (repository is null)
                throw new Exception($"Repository not found: {repositoryId}");

            var repositoryPath = repository.Path;

            if (string.IsNullOrWhiteSpace(repositoryPath) || !Directory.Exists(repositoryPath))
                throw new DirectoryNotFoundException($"Repository folder not found: {repositoryPath}");

            // Mark repository as indexing
            repository.Status = RepositoryStatus.Indexing;
            _dbContext.Repositories.Update(repository);
            await _dbContext.SaveChangesAsync();

            try
            {
                // Remove existing chunks
                var existingChunks = _dbContext.CodeChunks.Where(x => x.RepositoryId == repositoryId);
                _dbContext.CodeChunks.RemoveRange(existingChunks);
                await _dbContext.SaveChangesAsync();

                // Enumerate files
                var allFiles = Directory.EnumerateFiles(repositoryPath, "*.*", SearchOption.AllDirectories)
                    .Where(f => SupportedExtensions.Contains(Path.GetExtension(f), StringComparer.OrdinalIgnoreCase))
                    .Where(f => !IsInIgnoredFolder(f, repositoryPath))
                    .ToList();

                foreach (var file in allFiles)
                {
                    var ext = Path.GetExtension(file).ToLowerInvariant();

                    if (ext == ".cs")
                    {
                        var parsed = await _codeParser.ParseAsync(file);

                        foreach (var item in parsed)
                        {
                            var content = item.Code;
                            var (startLine, endLine) = await GetLineRangeAsync(file, content);

                            var chunk = new CodeChunk
                            {
                                RepositoryId = repositoryId,
                                FilePath = Path.GetRelativePath(repositoryPath, item.FilePath),
                                Namespace = item.Namespace,
                                ClassName = item.ClassName,
                                MethodName = item.MethodName,
                                Content = content,
                                StartLine = startLine,
                                EndLine = endLine,
                                ChunkType = "Method",
                                ContentHash = ComputeHash(content)
                            };

                            _dbContext.CodeChunks.Add(chunk);
                        }
                    }
                    else
                    {
                        var content = await File.ReadAllTextAsync(file);
                        var lines = content.Split('\n');

                        var chunk = new CodeChunk
                        {
                            RepositoryId = repositoryId,
                            FilePath = Path.GetRelativePath(repositoryPath, file),
                            Namespace = string.Empty,
                            ClassName = string.Empty,
                            MethodName = string.Empty,
                            Content = content,
                            StartLine = 1,
                            EndLine = lines.Length,
                            ChunkType = "File",
                            ContentHash = ComputeHash(content)
                        };

                        _dbContext.CodeChunks.Add(chunk);
                    }
                }

                await _dbContext.SaveChangesAsync();

                repository.Status = RepositoryStatus.Indexed;
                _dbContext.Repositories.Update(repository);
                await _dbContext.SaveChangesAsync();
            }
            catch (Exception)
            {
                repository.Status = RepositoryStatus.Failed;
                _dbContext.Repositories.Update(repository);
                await _dbContext.SaveChangesAsync();
                throw;
            }
        }

        private static bool IsInIgnoredFolder(string filePath, string repoRoot)
        {
            var relative = Path.GetRelativePath(repoRoot, filePath);
            var parts = relative.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            return parts.Any(p => IgnoredFolders.Contains(p, StringComparer.OrdinalIgnoreCase));
        }

        private static string ComputeHash(string content)
        {
            using var sha = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(content ?? string.Empty);
            var hash = sha.ComputeHash(bytes);
            return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
        }

        private static async Task<(int start, int end)> GetLineRangeAsync(string filePath, string snippet)
        {
            // Best-effort: find snippet in file and compute line numbers
            var all = await File.ReadAllTextAsync(filePath);
            var index = all.IndexOf(snippet, StringComparison.Ordinal);
            if (index < 0)
            {
                var total = all.Split('\n').Length;
                return (1, total);
            }

            var pre = all.Substring(0, index);
            var startLine = pre.Split('\n').Length;
            var snippetLines = snippet.Split('\n').Length;
            var endLine = startLine + snippetLines - 1;
            return (startLine, endLine);
        }
    }
}
