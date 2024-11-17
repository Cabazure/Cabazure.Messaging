using System.Collections.Concurrent;
using Azure.Storage.Blobs;
using Microsoft.Extensions.Options;

namespace Cabazure.Messaging.EventHub.Internal;

public interface IBlobStorageClientProvider
{
    BlobContainerClient GetClient(
        string? connectionName = null);
}

public class BlobStorageClientProvider(
    IOptionsMonitor<CabazureEventHubOptions> monitor)
    : IBlobStorageClientProvider
{
    private sealed record ClientKey(string? Connection);
    private readonly ConcurrentDictionary<ClientKey, BlobContainerClient> clients = new();

    public BlobContainerClient GetClient(
        string? connectionName)
        => clients.GetOrAdd(
            new(connectionName),
            CreateClient);

    private BlobContainerClient CreateClient(ClientKey key)
    {
        var options = monitor.Get(key.Connection);
        var storageClient = options.BlobStorage switch
        {
            { ConnectionString: { } cs, ContainerName: { } cont } => new BlobContainerClient(cs, cont),
            { ContainerUri: { } uri, Credential: { } cred } => new BlobContainerClient(uri, cred),
            { ContainerUri: { } uri } => new BlobContainerClient(uri, options.Credential),

            _ => throw new ArgumentException(
                $"Missing blob storage configuration for connection `{key.Connection}`"),
        };

        if (options.BlobStorage.CreateIfNotExist)
        {
            storageClient.CreateIfNotExists();
        }

        return storageClient;
    }
}
