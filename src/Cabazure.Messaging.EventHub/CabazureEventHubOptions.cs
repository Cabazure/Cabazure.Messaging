using System.Text.Json;
using Azure.Core;
using Azure.Messaging.EventHubs.Producer;
using Azure.Storage.Blobs;

namespace Cabazure.Messaging.EventHub;

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

    /// <summary>
    /// Options passed to <see cref="EventHubProducerClient"/> when constructing
    /// publishers on this connection, e.g. to configure <see cref="EventHubProducerClientOptions.RetryOptions"/>
    /// so publishing can ride out longer Event Hub throttling windows. When
    /// <see langword="null"/>, the Azure SDK defaults are used.
    /// </summary>
    public EventHubProducerClientOptions? ProducerClientOptions { get; set; }

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

    public CabazureEventHubOptions WithProducerClientOptions(EventHubProducerClientOptions options)
    {
        ProducerClientOptions = options;
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

public class BlobStorageOptions
{
    public Uri? ServiceUri { get; set; }

    public TokenCredential? Credential { get; set; }

    public string? ConnectionString { get; set; }

    public BlobClientOptions? BlobClientOptions { get; set; }
}
