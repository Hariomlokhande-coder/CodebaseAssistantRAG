using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

using CodebaseAssistant.Domain.Entities;
//using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Security.Cryptography.X509Certificates;

namespace CodebaseAssistant.Infrastructure.Persistence.Configurations;

public class RepositoryConfiguration : IEntityTypeConfiguration<Repository>
{
    public void Configure(EntityTypeBuilder<Repository> builder)
    {
        builder.ToTable("Repositories");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
               .IsRequired()
               .HasMaxLength(200);

        builder.Property(x => x.Path)
               .IsRequired()
               .HasMaxLength(1000);

        builder.Property(x => x.Hash)
               .IsRequired()
               .HasMaxLength(200);

        builder.Property(x => x.Status)
               .IsRequired();

        builder.Property(x => x.CreatedAt)
               .IsRequired();

        builder.Property(x => x.SizeInBytes)
               .IsRequired();   

        builder.Property(x => x.UploadedAt)
               .IsRequired();
        builder.Property( x => x.Description)
               .HasMaxLength(1000);
    }
}