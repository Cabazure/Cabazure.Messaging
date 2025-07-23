namespace Cabazure.Messaging;

/// <summary>
/// Defines a contract for processing messages of a specific type.
/// </summary>
/// <typeparam name="TMessage">The type of message to process.</typeparam>
public interface IMessageProcessor<in TMessage>
{
    Task ProcessAsync(
        TMessage message,
        MessageMetadata metadata,
        CancellationToken cancellationToken);
}
