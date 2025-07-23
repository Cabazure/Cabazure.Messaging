using System.Text.Json;
using Azure.Core;

namespace Cabazure.Messaging.ServiceBus;

/// <summary>
/// Represents configuration options for Azure Service Bus connections and settings.
/// </summary>
public class CabazureServiceBusOptions
{
    /// <summary>
    /// Gets or sets the token credential used for authentication with Azure Service Bus.
    /// </summary>
    public TokenCredential? Credential { get; set; }

    /// <summary>
    /// Gets or sets the fully qualified namespace of the Azure Service Bus.
    /// </summary>
    public string? FullyQualifiedNamespace { get; set; }

    /// <summary>
    /// Gets or sets the connection string for Azure Service Bus.
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
    public CabazureServiceBusOptions WithSerializerOptions(JsonSerializerOptions options)
    {
        SerializerOptions = options;
        return this;
    }

    /// <summary>
    /// Configures the Service Bus connection using a fully qualified namespace and token credential.
    /// </summary>
    /// <param name="fullyQualifiedNamespace">The fully qualified namespace of the Azure Service Bus.</param>
    /// <param name="credential">The token credential for authentication.</param>
    /// <returns>The current instance for method chaining.</returns>
    public CabazureServiceBusOptions WithConnection(string fullyQualifiedNamespace, TokenCredential credential)
    {
        FullyQualifiedNamespace = fullyQualifiedNamespace;
        Credential = credential;
        return this;
    }

    /// <summary>
    /// Configures the Service Bus connection using a connection string.
    /// </summary>
    /// <param name="connectionString">The connection string for Azure Service Bus.</param>
    /// <returns>The current instance for method chaining.</returns>
    public CabazureServiceBusOptions WithConnection(string connectionString)
    {
        ConnectionString = connectionString;
        return this;
    }
}
