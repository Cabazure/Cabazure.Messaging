namespace Cabazure.Messaging.EventHub;

/// <summary>
/// Defines a factory for creating Event Hub publishers for different message types.
/// </summary>
public interface IEventHubPublisherFactory
{
    IEventHubPublisher<TMessage> Create<TMessage>(
        string? connectionName = null);
}
