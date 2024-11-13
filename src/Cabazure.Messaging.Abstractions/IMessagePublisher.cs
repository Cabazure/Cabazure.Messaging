namespace Cabazure.Messaging;

public interface IMessagePublisher<in T>
{
    Task PublishAsync(
        T message,
        CancellationToken cancellationToken);

    Task PublishAsync(
        T message,
        PublishingOptions options,
        CancellationToken cancellationToken);
}
