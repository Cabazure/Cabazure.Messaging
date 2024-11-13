using System.Text.Json;
using Azure.Core;

namespace Cabazure.Messaging.ServiceBus;

public class CabazureServiceBusOptions
{
    public TokenCredential? Credential { get; set; }

    public string? FullyQualifiedNamespace { get; set; }

    public string? ConnectionString { get; set; }

    public JsonSerializerOptions SerializerOptions { get; set; } = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DictionaryKeyPolicy = null,
    };

    public CabazureServiceBusOptions WithSerializerOptions(JsonSerializerOptions options)
    {
        SerializerOptions = options;
        return this;
    }

    public CabazureServiceBusOptions WithConnection(string fullyQualifiedNamespace, TokenCredential credential)
    {
        FullyQualifiedNamespace = fullyQualifiedNamespace;
        Credential = credential;
        return this;
    }

    public CabazureServiceBusOptions WithConnection(string connectionString)
    {
        ConnectionString = connectionString;
        return this;
    }
}
