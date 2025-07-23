namespace Cabazure.Messaging.ServiceBus;

/// <summary>
/// Defines a contract for publishing messages to Azure Service Bus with Service Bus-specific options.
/// </summary>
/// <typeparam name="TMessage">The type of message to publish.</typeparam>
public interface IServiceBusPublisher<in TMessage>
    : IMessagePublisher<TMessage>
{
    /// <summary>
    /// Publishes a message asynchronously to Azure Service Bus with Service Bus-specific publishing options.
    /// </summary>
    /// <param name="message">The message to publish.</param>
    /// <param name="options">The Service Bus-specific options to use when publishing the message.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous publishing operation.</returns>
    Task PublishAsync(
        TMessage message,
        ServiceBusPublishingOptions options,
        CancellationToken cancellationToken);
}
