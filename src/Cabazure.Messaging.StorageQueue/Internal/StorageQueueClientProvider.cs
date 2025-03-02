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
            { QueueServiceUri: { } uri, Credential: { } cred } => new(uri, cred),
            { ConnectionString: { } cs } => new(cs),
            _ => throw new ArgumentException(
                $"Missing configuration for Storage Queue connection `{connectionName}`"),
        };
}
