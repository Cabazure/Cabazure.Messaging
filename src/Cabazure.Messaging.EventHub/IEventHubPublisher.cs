namespace Cabazure.Messaging.EventHub;

/// <summary>
/// Defines a contract for publishing messages to Azure Event Hubs with EventHub-specific options.
/// </summary>
/// <typeparam name="TMessage">The type of message to publish.</typeparam>
public interface IEventHubPublisher<in TMessage>
    : IMessagePublisher<TMessage>
{
    Task PublishAsync(
        TMessage message,
        EventHubPublishingOptions options,
        CancellationToken cancellationToken);
}
