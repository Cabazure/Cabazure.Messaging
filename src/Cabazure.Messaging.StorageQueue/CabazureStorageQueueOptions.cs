using System.Text.Json;
using Azure.Core;
using Azure.Storage.Queues;

namespace Cabazure.Messaging.StorageQueue;

/// <summary>
/// Represents configuration options for Azure Storage Queue connections and settings.
/// </summary>
public class CabazureStorageQueueOptions
{
    /// <summary>
    /// Gets or sets the token credential used for authentication with Azure Storage Queue.
    /// </summary>
    public TokenCredential? Credential { get; set; }

    /// <summary>
    /// Gets or sets the URI of the Azure Storage Queue service.
    /// </summary>
    public Uri? QueueServiceUri { get; set; }

    /// <summary>
    /// Gets or sets the connection string for Azure Storage Queue.
    /// </summary>
    public string? ConnectionString { get; set; }

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
    public CabazureStorageQueueOptions WithSerializerOptions(JsonSerializerOptions options)
    {
        SerializerOptions = options;
        return this;
    }

    /// <summary>
    /// Configures the Storage Queue connection using a queue service URI and token credential.
    /// </summary>
    /// <param name="queueServiceUri">The URI of the Azure Storage Queue service.</param>
    /// <param name="credential">The token credential for authentication.</param>
    /// <returns>The current instance for method chaining.</returns>
    public CabazureStorageQueueOptions WithConnection(Uri queueServiceUri, TokenCredential credential)
    {
        QueueServiceUri = queueServiceUri;
        Credential = credential;
        return this;
    }

    /// <summary>
    /// Configures the Storage Queue connection using a connection string.
    /// </summary>
    /// <param name="connectionString">The connection string for Azure Storage Queue.</param>
    /// <returns>The current instance for method chaining.</returns>
    public CabazureStorageQueueOptions WithConnection(string connectionString)
    {
        ConnectionString = connectionString;
        return this;
    }
}
