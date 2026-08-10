using CodebaseAssistant.Application.DTOs;
using CodebaseAssistant.Application.Interfaces;
using CodebaseAssistant.Infrastructure.Configuration;
using Microsoft.Extensions.Options;
using Qdrant.Client;
using Qdrant.Client.Grpc;

namespace CodebaseAssistant.Infrastructure.VectorDatabase;

public sealed class QdrantVectorStore : IVectorStore
{
    private readonly QdrantClient _client;

    public QdrantVectorStore(
        IOptions<QdrantOptions> options)
    {
        ArgumentNullException.ThrowIfNull(options);

        var qdrantOptions = options.Value;

        if (string.IsNullOrWhiteSpace(qdrantOptions.BaseUrl))
        {
            throw new InvalidOperationException(
                "Qdrant BaseUrl is not configured.");
        }

        var uri = new Uri(qdrantOptions.BaseUrl);

        _client = string.IsNullOrWhiteSpace(qdrantOptions.ApiKey)
            ? new QdrantClient(uri.Host, uri.Port)
            : new QdrantClient(
                uri.Host,
                uri.Port,
                qdrantOptions.ApiKey);
    }

    public async Task CreateCollectionAsync(
        string collectionName,
        int vectorSize,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(collectionName);

        if (vectorSize <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(vectorSize),
                "Vector size must be greater than zero.");
        }

        var exists = await _client.CollectionExistsAsync(
            collectionName,
            cancellationToken);

        if (exists)
        {
            return;
        }

        await _client.CreateCollectionAsync(
            collectionName,
            new VectorParams
            {
                Size = (ulong)vectorSize,
                Distance = Distance.Cosine
            },
            cancellationToken: cancellationToken);
    }

    public async Task UpsertAsync(
        string collectionName,
        Guid pointId,
        float[] vector,
        Dictionary<string, object> metadata,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(collectionName);

        if (vector is null || vector.Length == 0)
        {
            throw new ArgumentException(
                "Vector cannot be empty.",
                nameof(vector));
        }

        ArgumentNullException.ThrowIfNull(metadata);

        var payload = new Dictionary<string, Value>();

        foreach (var item in metadata)
        {
            payload[item.Key] = ConvertToQdrantValue(item.Value);
        }

        var point = new PointStruct
        {
            Id = new PointId
            {
                Uuid = pointId.ToString()
            },
            Vectors = new Vectors
            {
                Vector = new Vector
                {
                    Data = { vector }
                }
            }
        };

        foreach (var item in payload)
        {
            point.Payload[item.Key] = item.Value;
        }

        await _client.UpsertAsync(
            collectionName,
            new[] { point },
            cancellationToken: cancellationToken);
    }

    public async Task<IReadOnlyList<VectorSearchResult>> SearchAsync(
        string collectionName,
        float[] vector,
        int topK,
        Guid repositoryId,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(collectionName);

        if (vector is null || vector.Length == 0)
        {
            throw new ArgumentException(
                "Vector cannot be empty.",
                nameof(vector));
        }

        if (topK <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(topK),
                "Top-K must be greater than zero.");
        }

        var filter = new Filter
        {
            Must =
            {
                new Condition
                {
                    Field = new FieldCondition
                    {
                        Key = "RepositoryId",
                        Match = new Match
                        {
                            Keyword = repositoryId.ToString()
                        }
                    }
                }
            }
        };

        var results = await _client.SearchAsync(
            collectionName,
            vector,
            filter: filter,
            limit: (ulong)topK,
            cancellationToken: cancellationToken);

        return results
            .Select(result => new VectorSearchResult
            {
                PointId = GetPointId(result.Id),

                Score = result.Score,

                RepositoryId = GetGuidPayload(
                    result.Payload,
                    "RepositoryId"),

                ChunkId = GetGuidPayload(
                    result.Payload,
                    "ChunkId"),

                FilePath = GetStringPayload(
                    result.Payload,
                    "FilePath"),

                Namespace = GetStringPayload(
                    result.Payload,
                    "Namespace"),

                ClassName = GetStringPayload(
                    result.Payload,
                    "ClassName"),

                MethodName = GetStringPayload(
                    result.Payload,
                    "MethodName"),

                ChunkType = GetStringPayload(
                    result.Payload,
                    "ChunkType")
            })
            .ToList();
    }

    public async Task DeleteAsync(
        string collectionName,
        Guid pointId,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(collectionName);

        await _client.DeleteAsync(
            collectionName,
            new PointId
            {
                Uuid = pointId.ToString()
            },
            cancellationToken: cancellationToken);
    }

    private static Value ConvertToQdrantValue(object value)
    {
        return value switch
        {
            string stringValue => new Value
            {
                StringValue = stringValue
            },

            Guid guidValue => new Value
            {
                StringValue = guidValue.ToString()
            },

            int intValue => new Value
            {
                IntegerValue = intValue
            },

            long longValue => new Value
            {
                IntegerValue = longValue
            },

            bool boolValue => new Value
            {
                BoolValue = boolValue
            },

            double doubleValue => new Value
            {
                DoubleValue = doubleValue
            },

            float floatValue => new Value
            {
                DoubleValue = floatValue
            },

            _ => new Value
            {
                StringValue = value.ToString() ?? string.Empty
            }
        };
    }

    private static Guid GetGuidPayload(
        IDictionary<string, Value> payload,
        string key)
    {
        var value = GetStringPayload(payload, key);

        return Guid.TryParse(value, out var result)
            ? result
            : Guid.Empty;
    }

    private static string GetStringPayload(
        IDictionary<string, Value> payload,
        string key)
    {
        return payload.TryGetValue(key, out var value)
            ? value.StringValue
            : string.Empty;
    }

    private static Guid GetPointId(PointId pointId)
    {
        return Guid.TryParse(
            pointId.Uuid,
            out var result)
            ? result
            : Guid.Empty;
    }
}