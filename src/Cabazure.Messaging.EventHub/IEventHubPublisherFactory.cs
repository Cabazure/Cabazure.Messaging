namespace Cabazure.Messaging.EventHub;

public interface IEventHubPublisherFactory
{
    IEventHubPublisher<TMessage> Create<TMessage>(
        string? connectionName = null);
}
