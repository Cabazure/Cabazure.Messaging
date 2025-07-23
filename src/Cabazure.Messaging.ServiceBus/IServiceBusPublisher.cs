namespace Cabazure.Messaging.ServiceBus;

/// <summary>
/// Defines a contract for publishing messages to Azure Service Bus with Service Bus-specific options.
/// </summary>
/// <typeparam name="TMessage">The type of message to publish.</typeparam>
public interface IServiceBusPublisher<in TMessage>
    : IMessagePublisher<TMessage>
{
    Task PublishAsync(
        TMessage message,
        ServiceBusPublishingOptions options,
        CancellationToken cancellationToken);
}
