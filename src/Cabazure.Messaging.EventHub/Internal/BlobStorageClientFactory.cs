using Azure.Storage.Blobs;
using Microsoft.Extensions.Options;

namespace Cabazure.Messaging.EventHub.Internal;

public interface IBlobStorageClientFactory
{
    BlobContainerClient Create(
        string? connectionName = null);
}

public class BlobStorageClientFactory(
    IOptionsMonitor<CabazureEventHubOptions> monitor)
    : IBlobStorageClientFactory
{
    public BlobContainerClient Create(
        string? connectionName = null)
    {
        var options = monitor.Get(connectionName);
        var storageClient = options.BlobStorage switch
        {
            { ConnectionString: { } cs, ContainerName: { } cont } => new BlobContainerClient(cs, cont),
            { ContainerUri: { } uri, Credential: { } cred } => new BlobContainerClient(uri, cred),
            { ContainerUri: { } uri } => new BlobContainerClient(uri, options.Credential),

            _ => throw new ArgumentException(
                $"Missing blob storage configuration for connection `{connectionName}`"),
        };

        if (options.BlobStorage.CreateIfNotExist)
        {
            storageClient.CreateIfNotExists();
        }

        return storageClient;
    }
}
