using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Primitives;
using Cabazure.Messaging.EventHub.Internal;

namespace Cabazure.Messaging.EventHub.Tests.Internal;

public class EventHubProcessorTests
{
    public record TMessage(string Data);
    public class TProcessor : IMessageProcessor<TMessage>
    {
        public virtual Task ProcessAsync(TMessage message, MessageMetadata metadata, CancellationToken cancellationToken)
            => Task.CompletedTask;
    }

    [Theory, AutoNSubstituteData]
    public async Task OnProcessingEventBatchAsync_Should_Call_BatchHandler(
        [Frozen, NoAutoProperties] EventProcessorOptions processorOptions,
        [Frozen] IEventHubBatchHandler<TMessage, TProcessor> batchHandler,
        [Greedy] EventHubProcessor<TMessage, TProcessor> sut,
        IEnumerable<EventData> events,
        EventProcessorPartition partition,
        CancellationToken cancellationToken)
    {
        await sut.OnProcessingEventBatchAsync(
            events,
            partition,
            cancellationToken);

        _ = batchHandler
            .Received(1)
            .ProcessBatchAsync(
                events,
                partition,
                cancellationToken);
    }

    [Theory, AutoNSubstituteData]
    public async Task OnProcessingErrorAsync_Should_Call_BatchHandler(
        [Frozen, NoAutoProperties] EventProcessorOptions processorOptions,
        [Frozen] IEventHubBatchHandler<TMessage, TProcessor> batchHandler,
        [Greedy] EventHubProcessor<TMessage, TProcessor> sut,
        Exception exception,
        EventProcessorPartition partition,
        string operationDescription,
        CancellationToken cancellationToken)
    {
        await sut.OnProcessingErrorAsync(
            exception,
            partition,
            operationDescription,
            cancellationToken);

        _ = batchHandler
            .Received(1)
            .ProcessErrorAsync(
                exception,
                cancellationToken);
    }
}

public static class EventHubBatchProcessorExtensions
{
    public static Task OnProcessingEventBatchAsync<TMessage, TProcessor>(
        this EventHubProcessor<TMessage, TProcessor> processor,
        IEnumerable<EventData> events,
        EventProcessorPartition partition,
        CancellationToken cancellationToken)
        where TProcessor : IMessageProcessor<TMessage>
        => processor.InvokeProtectedMethod<Task>(
            "OnProcessingEventBatchAsync",
            events,
            partition,
            cancellationToken)!;

    public static Task OnProcessingErrorAsync<TMessage, TProcessor>(
        this EventHubProcessor<TMessage, TProcessor> processor,
        Exception exception,
        EventProcessorPartition partition,
        string operationDescription,
        CancellationToken cancellationToken)
        where TProcessor : IMessageProcessor<TMessage>
        => processor.InvokeProtectedMethod<Task>(
            "OnProcessingErrorAsync",
            exception,
            partition,
            operationDescription,
            cancellationToken)!;
}
