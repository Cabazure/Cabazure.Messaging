using Azure.Storage.Queues;
using Microsoft.Extensions.Options;

namespace Cabazure.Messaging.StorageQueue.Internal;

public interface IStorageQueueClientProvider
{
    QueueServiceClient GetClient(
        string? connectionName);
}

public class StorageQueueClientProvider(
    IOptionsMonitor<CabazureStorageQueueOptions> monitor)
    : IStorageQueueClientProvider
{
    public QueueServiceClient GetClient(
        string? connectionName)
        => monitor.Get(connectionName) switch
        {
            { ConnectionString: { } cs } => new(cs),
            { QueueServiceUri: { } uri, Credential: { } cred } => new(uri, cred),
            _ => throw new InvalidOperationException(
                $"Missing configuration for Storage Queue connection `{connectionName}`"),
        };
}
