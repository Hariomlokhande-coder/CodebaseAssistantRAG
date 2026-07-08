using Microsoft.EntityFrameworkCore;
using CodebaseAssistant.Domain.Entities;

namespace CodebaseAssistant.Infrastructure.Persistence
{
    public class CodebaseAssistantDbContext : DbContext
    {
        public CodebaseAssistantDbContext(
            DbContextOptions<CodebaseAssistantDbContext> options)
            : base(options)
        {
        }

        public DbSet<Repository> Repositories => Set<Repository>();
        public DbSet<CodeChunk> CodeChunks => Set<CodeChunk>();
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(
                typeof(CodebaseAssistantDbContext).Assembly);

            base.OnModelCreating(modelBuilder);
        }
    }
}