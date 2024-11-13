namespace Cabazure.Messaging.EventHub;

public interface IEventHubPublisher<in T>
    : IMessagePublisher<T>
{
    Task PublishAsync(
        T message,
        EventHubPublishingOptions options,
        CancellationToken cancellationToken);
}
