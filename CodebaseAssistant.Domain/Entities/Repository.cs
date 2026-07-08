using CodebaseAssistant.Domain.Enums;

namespace CodebaseAssistant.Domain.Entities
{
    public class Repository
    {
        public Guid Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Path { get; set; } = string.Empty;

        public string Hash { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }

        public RepositoryStatus Status { get; set; }

        public long SizeInBytes { get; set; }

        public DateTime UploadedAt { get; set; }

        public string? Description { get; set; }
    }
}