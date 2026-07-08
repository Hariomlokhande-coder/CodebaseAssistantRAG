using CodebaseAssistant.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CodebaseAssistant.Infrastructure.Persistence.Configurations
{
    public class CodeChunkConfiguration : IEntityTypeConfiguration<CodeChunk>
    {
        public void Configure(EntityTypeBuilder<CodeChunk> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.FilePath)
                .IsRequired()
                .HasMaxLength(500);

            builder.Property(x => x.Namespace)
                .HasMaxLength(200);

            builder.Property(x => x.ClassName)
                .HasMaxLength(200);

            builder.Property(x => x.MethodName)
                .HasMaxLength(200);

            builder.Property(x => x.Content)
                .IsRequired();

            builder.Property(x => x.ContentHash)
                .HasMaxLength(100);

            builder.HasOne(x => x.Repository)
                .WithMany()
                .HasForeignKey(x => x.RepositoryId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}