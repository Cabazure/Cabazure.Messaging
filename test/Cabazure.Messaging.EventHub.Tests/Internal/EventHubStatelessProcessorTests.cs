using System.Text.Json;
using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Consumer;
using Cabazure.Messaging.EventHub.Internal;
using Dasync.Collections;

namespace Cabazure.Messaging.EventHub.Tests.Internal;

public class EventHubStatelessProcessorTests
{
    public record TMessage(string Data);
    public abstract class TProcessor : IMessageProcessor<TMessage>
    {
        public abstract Task ProcessAsync(TMessage message, MessageMetadata metadata, CancellationToken cancellationToken);
    }
    public abstract class TProcessorWithErrorHandling : IMessageProcessor<TMessage>, IProcessErrorHandler
    {
        public abstract Task ProcessAsync(TMessage message, MessageMetadata metadata, CancellationToken cancellationToken);

        public abstract Task ProcessErrorAsync(Exception exception, CancellationToken cancellationToken);
    }

    [Theory, AutoNSubstituteData]
    public void Has_Processor(
        [Frozen, NoAutoProperties] JsonSerializerOptions serializerOptions,
        [Frozen] TProcessor processor,
        EventHubStatelessProcessor<TMessage, TProcessor> sut)
        => sut.Processor.Should().Be(processor);

    [Theory, AutoNSubstituteData]
    public async Task Can_Start_And_Stop(
        [Frozen, NoAutoProperties] JsonSerializerOptions serializerOptions,
        [Frozen] IEventHubConsumerClient client,
        [Frozen] ReadEventOptions readOptions,
        EventHubStatelessProcessor<TMessage, TProcessor> sut,
        CancellationToken cancellationToken)
    {
        sut.IsRunning.Should().BeFalse();

        _ = sut.StartProcessingAsync(cancellationToken);

        sut.IsRunning.Should().BeTrue();

        await sut.StopProcessingAsync(cancellationToken);
        await sut.ExecuteTask;

        sut.IsRunning.Should().BeFalse();
    }

    [Theory, AutoNSubstituteData]
    public async Task Should_Read_Events_From_Client(
        [Frozen, NoAutoProperties] JsonSerializerOptions serializerOptions,
        [Frozen] IEventHubConsumerClient client,
        [Frozen] ReadEventOptions readOptions,
        EventHubStatelessProcessor<TMessage, TProcessor> sut,
        CancellationTokenSource cts)
    {
        client
            .WhenForAnyArgs(c => c.ReadEventsAsync(default, default, default))
            .Do(c => cts.Cancel());

        _ = sut.StartProcessingAsync(cts.Token);

        await client.WaitForCallForAnyArgs(c
            => c.ReadEventsAsync(default, default, default));

        client
            .Received(1)
            .ReadEventsAsync(false, readOptions, Arg.Any<CancellationToken>());
    }

    [Theory, AutoNSubstituteData]
    public async Task Should_Call_Processor_For_Each_Message(
        [Frozen, NoAutoProperties] JsonSerializerOptions serializerOptions,
        [Frozen] IEventHubConsumerClient client,
        [Frozen, Substitute] TProcessor processor,
        [Frozen] List<Func<IDictionary<string, object>, bool>> filters,
        EventHubStatelessProcessor<TMessage, TProcessor> sut,
        TMessage[] messages,
        EventData[] data,
        string fullyQualifiedNamespace,
        string eventHubName,
        string consumerGroup,
        string partitionId,
        CancellationTokenSource cts)
    {
        filters.Clear();
        for (int i = 0; i < data.Length; i++)
        {
            data[i].EventBody = BinaryData.FromObjectAsJson(
                messages[i],
                serializerOptions);
        }
        var partition = EventHubsModelFactory.PartitionContext(
            fullyQualifiedNamespace,
            eventHubName,
            consumerGroup,
            partitionId);
        var eventHubMessages = data
            .Select(m => new PartitionEvent(partition, m));

        client
            .ReadEventsAsync(default, default, default)
            .ReturnsForAnyArgs(
                c =>
                {
                    cts.Cancel();
                    return eventHubMessages.ToAsyncEnumerable();
                });

        await sut.StartProcessingAsync(cts.Token);
        await sut.ExecuteTask;

        _ = processor
            .Received(3)
            .ProcessAsync(
                Arg.Any<TMessage>(),
                Arg.Any<EventHubMetadata>(),
                Arg.Any<CancellationToken>());

        processor
            .ReceivedCallsWithArguments<TMessage>()
            .Should()
            .BeEquivalentTo(messages);

        processor
            .ReceivedCallsWithArguments<EventHubMetadata>()
            .Should()
            .BeEquivalentTo(eventHubMessages
                .Select(e => EventHubMetadata.Create(
                    e.Data,
                    e.Partition.PartitionId)));
    }
}
