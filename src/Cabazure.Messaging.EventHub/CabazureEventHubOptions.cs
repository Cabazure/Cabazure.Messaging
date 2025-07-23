using System.Text.Json;
using Azure.Core;
using Azure.Storage.Blobs;

namespace Cabazure.Messaging.EventHub;

/// <summary>
/// Represents configuration options for Azure Event Hub connections and settings.
/// </summary>
public class CabazureEventHubOptions
{
    public TokenCredential? Credential { get; set; }

    public string? FullyQualifiedNamespace { get; set; }

    public string? ConnectionString { get; set; }

    public BlobStorageOptions? BlobStorage { get; set; }

    public JsonSerializerOptions SerializerOptions { get; set; } = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DictionaryKeyPolicy = null,
    };

    public CabazureEventHubOptions WithSerializerOptions(JsonSerializerOptions options)
    {
        SerializerOptions = options;
        return this;
    }

    public CabazureEventHubOptions WithConnection(string fullyQualifiedNamespace, TokenCredential credential)
    {
        FullyQualifiedNamespace = fullyQualifiedNamespace;
        Credential = credential;
        return this;
    }

    public CabazureEventHubOptions WithConnection(string connectionString)
    {
        ConnectionString = connectionString;
        return this;
    }

    public CabazureEventHubOptions WithBlobStorage(
        Uri serviceUri,
        TokenCredential credential,
        BlobClientOptions? options = null)
    {
        BlobStorage = new()
        {
            ServiceUri = serviceUri,
            Credential = credential,
            BlobClientOptions = options,
        };
        return this;
    }

    public CabazureEventHubOptions WithBlobStorage(
        string connectionString,
        BlobClientOptions? options = null)
    {
        BlobStorage = new()
        {
            ConnectionString = connectionString,
            BlobClientOptions = options,
        };
        return this;
    }

}

/// <summary>
/// Represents configuration options for Azure Blob Storage used for Event Hub checkpoint storage.
/// </summary>
public class BlobStorageOptions
{
    public Uri? ServiceUri { get; set; }

    public TokenCredential? Credential { get; set; }

    public string? ConnectionString { get; set; }

    public BlobClientOptions? BlobClientOptions { get; set; }
}
