namespace Cabazure.Messaging.EventHub;

public interface IEventHubPublisherFactory
{
    IEventHubPublisher<T> Create<T>(
        string? connectionName = null);
}