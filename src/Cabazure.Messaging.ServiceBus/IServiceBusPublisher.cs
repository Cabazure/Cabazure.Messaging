namespace Cabazure.Messaging.ServiceBus;

public interface IServiceBusPublisher<in TMessage>
    : IMessagePublisher<TMessage>
{
    Task PublishAsync(
        TMessage message,
        ServiceBusPublishingOptions options,
        CancellationToken cancellationToken);
}
