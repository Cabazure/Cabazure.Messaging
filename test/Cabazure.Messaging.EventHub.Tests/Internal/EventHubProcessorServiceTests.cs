using Cabazure.Messaging.EventHub.Internal;

namespace Cabazure.Messaging.EventHub.Tests.Internal;

public class EventHubProcessorServiceTests
{
    public record TMessage(string Data);
    public class TProcessor : IMessageProcessor<TMessage>
    {
        public virtual Task ProcessAsync(TMessage message, MessageMetadata metadata, CancellationToken cancellationToken)
            => Task.CompletedTask;
    }

    [Theory, AutoNSubstituteData]
    public async Task StartAsync_Starts_Processing(
        [Frozen] IEventHubBatchProcessor<TProcessor> processor,
        EventHubProcessorService<TMessage, TProcessor> sut,
        CancellationToken cancellationToken)
    {
        await sut.StartAsync(cancellationToken);

        _ = processor
            .Received(1)
            .StartProcessingAsync(cancellationToken);
    }

    [Theory]
    [InlineAutoNSubstituteData(true)]
    [InlineAutoNSubstituteData(false)]
    public void IsRunning_Returns_From_Processor(
        bool isRunning,
        [Frozen] IEventHubBatchProcessor<TProcessor> processor,
        EventHubProcessorService<TMessage, TProcessor> sut,
        CancellationToken cancellationToken)
    {
        processor.IsRunning.Returns(isRunning);

        sut.IsRunning.Should().Be(isRunning);
    }

    [Theory, AutoNSubstituteData]
    public async Task StopAsync_Stops_Processing(
        [Frozen] IEventHubBatchProcessor<TProcessor> processor,
        EventHubProcessorService<TMessage, TProcessor> sut,
        CancellationToken cancellationToken)
    {
        await sut.StartAsync(cancellationToken);
        await sut.StopAsync(cancellationToken);

        _ = processor
            .Received(1)
            .StopProcessingAsync(cancellationToken);
    }
}
