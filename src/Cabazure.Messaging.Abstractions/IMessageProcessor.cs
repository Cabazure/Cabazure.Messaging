namespace Cabazure.Messaging;

public interface IMessageProcessor<in T>
{
    Task ProcessAsync(
        T message,
        MessageMetadata metadata,
        CancellationToken cancellationToken);
}
