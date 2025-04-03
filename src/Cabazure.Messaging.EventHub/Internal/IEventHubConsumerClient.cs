using System.Diagnostics.CodeAnalysis;
using Azure.Messaging.EventHubs.Consumer;

namespace Cabazure.Messaging.EventHub.Internal;

public interface IEventHubConsumerClient : IAsyncDisposable
{
    public string FullyQualifiedNamespace { get; }

    public string EventHubName { get; }

    public string ConsumerGroup { get; }

    IAsyncEnumerable<PartitionEvent> ReadEventsAsync(
        bool startReadingAtEarliestEvent,
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

    public IAsyncEnumerable<PartitionEvent> ReadEventsAsync(
        bool startReadingAtEarliestEvent,
        ReadEventOptions? readOptions = null,
        CancellationToken cancellationToken = default)
            => client.ReadEventsAsync(
                startReadingAtEarliestEvent,
                readOptions,
                cancellationToken);
}
