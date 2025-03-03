using System.Text.Json;
using Azure.Storage.Queues;
using Cabazure.Messaging.StorageQueue.Internal;

namespace Cabazure.Messaging.StorageQueue.Tests.Internal;

public class StorageQueuePublisherTests
{
    public record TMessage(string Data);

    [Theory, AutoNSubstituteData]
    public async Task PublishAsync_Calls_SendMessageAsync_On_Client(
        [Frozen, NoAutoProperties] JsonSerializerOptions serializerOptions,
        [Frozen, Substitute] QueueClient client,
        StorageQueuePublisher<TMessage> sut,
        TMessage message,
        CancellationToken cancellationToken)
    {
        await sut.PublishAsync(
            message,
            cancellationToken);

        _ = client
            .Received(1)
            .SendMessageAsync(
                Arg.Any<BinaryData>(),
                visibilityTimeout: null,
                timeToLive: null,
                cancellationToken);
    }

    [Theory, AutoNSubstituteData]
    public async Task PublishAsync_Calls_SendMessageAsync_With_Options(
        [Frozen, NoAutoProperties] JsonSerializerOptions serializerOptions,
        [Frozen, Substitute] QueueClient client,
        StorageQueuePublisher<TMessage> sut,
        TMessage message,
        StorageQueuePublishingOptions options,
        CancellationToken cancellationToken)
    {
        await sut.PublishAsync(
            message,
            options,
            cancellationToken);

        _ = client
            .Received(1)
            .SendMessageAsync(
                Arg.Any<BinaryData>(),
                options.VisibilityTimeout,
                options.TimeToLive,
                cancellationToken);
    }

    [Theory, AutoNSubstituteData]
    public async Task PublishAsync_Sends_Serialized_Message(
        [Frozen, NoAutoProperties] JsonSerializerOptions serializerOptions,
        [Frozen, Substitute] QueueClient client,
        StorageQueuePublisher<TMessage> sut,
        TMessage message,
        CancellationToken cancellationToken)
    {
        await sut.PublishAsync(
            message,
            cancellationToken);

        var data = client
            .ReceivedCallWithArgument<BinaryData>();
        data
            .ToObjectFromJson<TMessage>(serializerOptions)
            .Should()
            .BeEquivalentTo(message);
    }
}
