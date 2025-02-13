using System.Collections.Concurrent;
using Azure.Storage.Blobs;
using Microsoft.Extensions.Options;

namespace Cabazure.Messaging.EventHub.Internal;

public interface IBlobStorageClientProvider
{
    BlobServiceClient GetClient(
        string? connectionName = null);
}

public class BlobStorageClientProvider(
    IOptionsMonitor<CabazureEventHubOptions> monitor)
    : IBlobStorageClientProvider
{
    private sealed record ClientKey(string? Connection);
    private readonly ConcurrentDictionary<ClientKey, BlobServiceClient> clients = new();

    public BlobServiceClient GetClient(
        string? connectionName)
        => clients.GetOrAdd(
            new(connectionName),
            CreateClient);

    private BlobServiceClient CreateClient(ClientKey key)
        => monitor.Get(key.Connection) switch
        {
            { BlobStorage.ConnectionString: { } cs, BlobStorage.BlobClientOptions: { } o } => new BlobServiceClient(cs, o),
            { BlobStorage.ConnectionString: { } cs } => new BlobServiceClient(cs),
            { BlobStorage: { ServiceUri: { } uri, Credential: { } cred }, BlobStorage.BlobClientOptions: { } o } => new BlobServiceClient(uri, cred, o),
            { BlobStorage: { ServiceUri: { } uri, Credential: { } cred } } => new BlobServiceClient(uri, cred),
            { BlobStorage.ServiceUri: { } uri, Credential: { } cred, BlobStorage.BlobClientOptions: { } o } => new BlobServiceClient(uri, cred, o),
            { BlobStorage.ServiceUri: { } uri, Credential: { } cred } => new BlobServiceClient(uri, cred),

            _ => throw new ArgumentException(
                $"Missing blob storage configuration for connection `{key.Connection}`"),
        };
}
