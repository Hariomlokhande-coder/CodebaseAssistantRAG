using CodebaseAssistant.Application.Interfaces;
using CodebaseAssistant.Infrastructure.Configuration;
using CodebaseAssistant.Infrastructure.Embedding;
using CodebaseAssistant.Infrastructure.Roslyn;
using CodebaseAssistant.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CodebaseAssistant.Infrastructure.DependencyInjection;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<UploadSettings>(
            configuration.GetSection("UploadSettings"));

        services.Configure<OpenAiOptions>(
            configuration.GetSection("OpenAI"));

        services.Configure<QdrantOptions>(
            configuration.GetSection("Qdrant"));

        services.Configure<ChatOptions>(
            configuration.GetSection("Chat"));

        services.AddHttpClient<IEmbeddingService,
            OpenAiEmbeddingService>();

        // Qdrant vector store
        services.AddScoped<IVectorStore, VectorDatabase.QdrantVectorStore>();

        services.AddScoped<IRepositoryService,
            RepositoryService>();

        services.AddScoped<ICodeParser,
            CSharpCodeParser>();

        services.AddScoped<IIndexingService,
            IndexingService>();

        services.AddHttpClient<IChatService, ChatService>();

        return services;
    }
}
