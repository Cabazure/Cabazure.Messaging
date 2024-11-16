namespace Cabazure.Messaging.EventHub;

public interface IEventHubPublisher<in TMessage>
    : IMessagePublisher<TMessage>
{
    Task PublishAsync(
        TMessage message,
        EventHubPublishingOptions options,
        CancellationToken cancellationToken);
}
