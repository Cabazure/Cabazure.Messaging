namespace Cabazure.Messaging.ServiceBus;

public interface IServiceBusPublisher<in T>
    : IMessagePublisher<T>
{
    Task PublishAsync(
        T message,
        ServiceBusPublishingOptions options,
        CancellationToken cancellationToken);
}
