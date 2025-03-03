using System.Text.Json;
using Azure.Storage.Queues;

namespace Cabazure.Messaging.StorageQueue.Internal;

public class StorageQueuePublisher<TMessage>(
    JsonSerializerOptions serializerOptions,
    QueueClient queue)
    : IStorageQueuePublisher<TMessage>
    , IMessagePublisher<TMessage>
{
    public async Task PublishAsync(
        TMessage message,
        StorageQueuePublishingOptions options,
        CancellationToken cancellationToken)
        => await PerformPublishAsync(
            message,
            options,
            cancellationToken);

    public async Task PublishAsync(
        TMessage message,
        CancellationToken cancellationToken)
        => await PerformPublishAsync(
            message,
            options: null,
            cancellationToken);

    public async Task PublishAsync(
        TMessage message,
        PublishingOptions options,
        CancellationToken cancellationToken)
        => await PerformPublishAsync(
            message,
            options as StorageQueuePublishingOptions,
            cancellationToken);

    private async Task PerformPublishAsync(
        TMessage message,
        StorageQueuePublishingOptions? options,
        CancellationToken cancellationToken)
    {
        var data = BinaryData.FromObjectAsJson(
            message,
            serializerOptions);

        await queue.SendMessageAsync(
            data,
            options?.VisibilityTimeout,
            options?.TimeToLive,
            cancellationToken);
    }
}
