namespace Cabazure.Messaging;

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
