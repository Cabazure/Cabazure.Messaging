using System.Text.Json;
using Azure.Core;

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
        TokenCredential credential)
    {
        BlobStorage = new()
        {
            ServiceUri = serviceUri,
            Credential = credential,
        };
        return this;
    }

    public CabazureEventHubOptions WithBlobStorage(
        string connectionString)
    {
        BlobStorage = new()
        {
            ConnectionString = connectionString,
        };
        return this;
    }

}

public class BlobStorageOptions
{
    public Uri? ServiceUri { get; set; }

    public TokenCredential? Credential { get; set; }

    public string? ConnectionString { get; set; }
}
