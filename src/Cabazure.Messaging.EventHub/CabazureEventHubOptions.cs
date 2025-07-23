using System.Text.Json;
using Azure.Core;
using Azure.Storage.Blobs;

namespace Cabazure.Messaging.EventHub;

/// <summary>
/// Represents configuration options for Azure Event Hub connections and settings.
/// </summary>
public class CabazureEventHubOptions
{
    /// <summary>
    /// Gets or sets the token credential used for authentication with Azure Event Hub.
    /// </summary>
    public TokenCredential? Credential { get; set; }

    /// <summary>
    /// Gets or sets the fully qualified namespace of the Azure Event Hub.
    /// </summary>
    public string? FullyQualifiedNamespace { get; set; }

    /// <summary>
    /// Gets or sets the connection string for Azure Event Hub.
    /// </summary>
    public string? ConnectionString { get; set; }

    /// <summary>
    /// Gets or sets the blob storage options for Event Hub checkpoint storage.
    /// </summary>
    public BlobStorageOptions? BlobStorage { get; set; }

    /// <summary>
    /// Gets or sets the JSON serializer options used for message serialization.
    /// </summary>
    public JsonSerializerOptions SerializerOptions { get; set; } = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DictionaryKeyPolicy = null,
    };

    /// <summary>
    /// Configures the JSON serializer options and returns the current instance for method chaining.
    /// </summary>
    /// <param name="options">The JSON serializer options to use.</param>
    /// <returns>The current instance for method chaining.</returns>
    public CabazureEventHubOptions WithSerializerOptions(JsonSerializerOptions options)
    {
        SerializerOptions = options;
        return this;
    }

    /// <summary>
    /// Configures the Event Hub connection using a fully qualified namespace and token credential.
    /// </summary>
    /// <param name="fullyQualifiedNamespace">The fully qualified namespace of the Azure Event Hub.</param>
    /// <param name="credential">The token credential for authentication.</param>
    /// <returns>The current instance for method chaining.</returns>
    public CabazureEventHubOptions WithConnection(string fullyQualifiedNamespace, TokenCredential credential)
    {
        FullyQualifiedNamespace = fullyQualifiedNamespace;
        Credential = credential;
        return this;
    }

    /// <summary>
    /// Configures the Event Hub connection using a connection string.
    /// </summary>
    /// <param name="connectionString">The connection string for Azure Event Hub.</param>
    /// <returns>The current instance for method chaining.</returns>
    public CabazureEventHubOptions WithConnection(string connectionString)
    {
        ConnectionString = connectionString;
        return this;
    }

    /// <summary>
    /// Configures blob storage for Event Hub checkpoint storage using a service URI and token credential.
    /// </summary>
    /// <param name="serviceUri">The URI of the Azure Blob Storage service.</param>
    /// <param name="credential">The token credential for authentication.</param>
    /// <param name="options">Optional blob client options.</param>
    /// <returns>The current instance for method chaining.</returns>
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

    /// <summary>
    /// Configures blob storage for Event Hub checkpoint storage using a connection string.
    /// </summary>
    /// <param name="connectionString">The connection string for Azure Blob Storage.</param>
    /// <param name="options">Optional blob client options.</param>
    /// <returns>The current instance for method chaining.</returns>
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
    /// <summary>
    /// Gets or sets the URI of the Azure Blob Storage service.
    /// </summary>
    public Uri? ServiceUri { get; set; }

    /// <summary>
    /// Gets or sets the token credential for Azure Blob Storage authentication.
    /// </summary>
    public TokenCredential? Credential { get; set; }

    /// <summary>
    /// Gets or sets the connection string for Azure Blob Storage.
    /// </summary>
    public string? ConnectionString { get; set; }

    /// <summary>
    /// Gets or sets the blob client options for configuring the blob client behavior.
    /// </summary>
    public BlobClientOptions? BlobClientOptions { get; set; }
}
