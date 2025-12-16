using System.Diagnostics.CodeAnalysis;
using Azure.Messaging.EventHubs.Consumer;

namespace Cabazure.Messaging.EventHub.Internal;

public interface IEventHubConsumerClient : IAsyncDisposable
{
    public string FullyQualifiedNamespace { get; }

    public string EventHubName { get; }

    public string ConsumerGroup { get; }

    Task<string[]> GetPartitionIdsAsync(
        CancellationToken cancellationToken = default);

    IAsyncEnumerable<PartitionEvent> ReadEventsFromPartitionAsync(
        string partitionId,
        EventPosition startingPosition,
        ReadEventOptions? readOptions = null,
        CancellationToken cancellationToken = default);
}

[ExcludeFromCodeCoverage]
public sealed class EventHubConsumerClientWrapper(
    EventHubConsumerClient client)
    : IEventHubConsumerClient
{
    public string FullyQualifiedNamespace => client.FullyQualifiedNamespace;

    public string EventHubName => client.EventHubName;

    public string ConsumerGroup => client.ConsumerGroup;

    public ValueTask DisposeAsync() => client.DisposeAsync();

    public IAsyncEnumerable<PartitionEvent> ReadEventsFromPartitionAsync(
        string partitionId,
        EventPosition startingPosition,
        ReadEventOptions? readOptions = null,
        CancellationToken cancellationToken = default)
            => client.ReadEventsFromPartitionAsync(
                partitionId,
                startingPosition,
                readOptions,
                cancellationToken);

    public async Task<string[]> GetPartitionIdsAsync(CancellationToken cancellationToken = default)
    {
        var properties = await client.GetEventHubPropertiesAsync(cancellationToken);
        return properties.PartitionIds;
    }
}
