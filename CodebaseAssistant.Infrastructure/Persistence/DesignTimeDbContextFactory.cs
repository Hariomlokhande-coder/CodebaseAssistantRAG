using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace CodebaseAssistant.Infrastructure.Persistence;

/// <summary>
/// Design-time factory to ensure EF tools can create a single, unambiguous DbContext instance.
/// This prevents the EF tooling from scanning assemblies and encountering duplicate context types.
/// </summary>
public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<CodebaseAssistantDbContext>
{
    public CodebaseAssistantDbContext CreateDbContext(string[] args)
    {
        var builder = new DbContextOptionsBuilder<CodebaseAssistantDbContext>();

        // Try to locate configuration (appsettings.json) starting from current directory.
        var basePath = Directory.GetCurrentDirectory();

        var config = new ConfigurationBuilder()
            .SetBasePath(basePath)
            .AddJsonFile("appsettings.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var connectionString = config.GetConnectionString("DefaultConnection")
            ?? "Server=(localdb)\\MSSQLLocalDB;Database=CodebaseAssistantRAGDb;Trusted_Connection=True;";

        builder.UseSqlServer(connectionString);

        return new CodebaseAssistantDbContext(builder.Options);
    }
}
