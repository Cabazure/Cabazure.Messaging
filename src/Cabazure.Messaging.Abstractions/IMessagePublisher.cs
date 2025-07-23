namespace Cabazure.Messaging;

/// <summary>
/// Defines a contract for publishing messages of a specific type to messaging services.
/// </summary>
/// <typeparam name="TMessage">The type of message to publish.</typeparam>
public interface IMessagePublisher<in TMessage>
{
    Task PublishAsync(
        TMessage message,
        CancellationToken cancellationToken);

    Task PublishAsync(
        TMessage message,
        PublishingOptions options,
        CancellationToken cancellationToken);
}
