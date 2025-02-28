using System.Text.Json;
using Azure.Core;
using Azure.Storage.Queues;

namespace Cabazure.Messaging.StorageQueue;

public class CabazureStorageQueueOptions
{
    public TokenCredential? Credential { get; set; }

    public Uri? QueueServiceUri { get; set; }

    public string? ConnectionString { get; set; }

    public JsonSerializerOptions SerializerOptions { get; set; } = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DictionaryKeyPolicy = null,
    };

    public CabazureStorageQueueOptions WithSerializerOptions(JsonSerializerOptions options)
    {
        SerializerOptions = options;
        return this;
    }

    public CabazureStorageQueueOptions WithConnection(Uri queueServiceUri, TokenCredential credential)
    {
        QueueServiceUri = queueServiceUri;
        Credential = credential;
        return this;
    }

    public CabazureStorageQueueOptions WithConnection(string connectionString)
    {
        ConnectionString = connectionString;
        return this;
    }
}
