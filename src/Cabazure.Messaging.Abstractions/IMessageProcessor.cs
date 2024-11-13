namespace Cabazure.Messaging;

public interface IMessageProcessor<in TMessage>
{
    Task ProcessAsync(
        TMessage message,
        MessageMetadata metadata,
        CancellationToken cancellationToken);
}
