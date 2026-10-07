using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Primitives;
using Azure.Messaging.EventHubs.Processor;
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
    public async Task OnProcessingEventBatchAsync_Should_Not_Throw_When_Canceled_During_Processing(
        [Frozen, NoAutoProperties] EventProcessorOptions processorOptions,
        [Frozen] IEventHubBatchHandler<TMessage, TProcessor> batchHandler,
        [Greedy] EventHubProcessor<TMessage, TProcessor> sut,
        IEnumerable<EventData> events,
        EventProcessorPartition partition)
    {
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();
        batchHandler
            .ProcessBatchAsync(events, partition, cts.Token)
            .ThrowsAsync(new OperationCanceledException(cts.Token));

        var act = () => sut.OnProcessingEventBatchAsync(
            events,
            partition,
            cts.Token);

        await act.Should().NotThrowAsync();
    }

    [Theory, AutoNSubstituteData]
    public async Task OnProcessingEventBatchAsync_Should_Not_Throw_When_Canceled_During_Checkpoint(
        [Frozen, NoAutoProperties] EventProcessorOptions processorOptions,
        [Frozen] IEventHubBatchHandler<TMessage, TProcessor> batchHandler,
        [Frozen] CheckpointStore checkpointStore,
        [Greedy] EventHubProcessor<TMessage, TProcessor> sut,
        IEnumerable<EventData> events,
        EventProcessorPartition partition)
    {
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();
        batchHandler
            .ProcessBatchAsync(events, partition, cts.Token)
            .Returns(CreateEvent());
        checkpointStore
            .UpdateCheckpointAsync(default!, default!, default!, default!, default!, default(CheckpointPosition), TestContext.Current.CancellationToken)
            .ThrowsAsyncForAnyArgs(new TaskCanceledException());

        var act = () => sut.OnProcessingEventBatchAsync(
            events,
            partition,
            cts.Token);

        await act.Should().NotThrowAsync();
        _ = checkpointStore
            .ReceivedWithAnyArgs(1)
            .UpdateCheckpointAsync(default!, default!, default!, default!, default!, default(CheckpointPosition), TestContext.Current.CancellationToken);
    }

    [Theory, AutoNSubstituteData]
    public async Task OnProcessingEventBatchAsync_Should_Throw_When_Checkpoint_Is_Canceled_Without_Cancellation(
        [Frozen, NoAutoProperties] EventProcessorOptions processorOptions,
        [Frozen] IEventHubBatchHandler<TMessage, TProcessor> batchHandler,
        [Frozen] CheckpointStore checkpointStore,
        [Greedy] EventHubProcessor<TMessage, TProcessor> sut,
        IEnumerable<EventData> events,
        EventProcessorPartition partition)
    {
        batchHandler
            .ProcessBatchAsync(events, partition, TestContext.Current.CancellationToken)
            .Returns(CreateEvent());
        checkpointStore
            .UpdateCheckpointAsync(default!, default!, default!, default!, default!, default(CheckpointPosition), TestContext.Current.CancellationToken)
            .ThrowsAsyncForAnyArgs(new TaskCanceledException());

        var act = () => sut.OnProcessingEventBatchAsync(
            events,
            partition,
            TestContext.Current.CancellationToken);

        await act.Should().ThrowAsync<TaskCanceledException>();
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

    private static EventData CreateEvent()
        => EventHubsModelFactory.EventData(
            new BinaryData("{}"),
            sequenceNumber: 42,
            offsetString: "42");
}

public static class EventHubBatchProcessorExtensions
{
    public static Task OnProcessingEventBatchAsync<TMessage, TProcessor>(
        this EventHubProcessor<TMessage, TProcessor> processor,
        IEnumerable<EventData> events,
        EventProcessorPartition partition,
        CancellationToken cancellationToken)
        where TProcessor : IMessageProcessor<TMessage>
        => processor.InvokeProtected<Task>(
            "OnProcessingEventBatchAsync",
            events,
            partition,
            cancellationToken);

    public static Task OnProcessingErrorAsync<TMessage, TProcessor>(
        this EventHubProcessor<TMessage, TProcessor> processor,
        Exception exception,
        EventProcessorPartition partition,
        string operationDescription,
        CancellationToken cancellationToken)
        where TProcessor : IMessageProcessor<TMessage>
        => processor.InvokeProtected<Task>(
            "OnProcessingErrorAsync",
            exception,
            partition,
            operationDescription,
            cancellationToken);
}
