using CodebaseAssistant.Application.Interfaces;
using CodebaseAssistant.Infrastructure.Configuration;
using CodebaseAssistant.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CodebaseAssistant.Infrastructure.DependencyInjection;

public static class DependencyInjection
{
    // Renamed to avoid ambiguous extension method collisions during solution build
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<UploadSettings>(
            configuration.GetSection("UploadSettings"));

        services.AddScoped<IRepositoryService, RepositoryService>();

        services.AddScoped<IIndexingService, IndexingService>();

        return services;
    }
}