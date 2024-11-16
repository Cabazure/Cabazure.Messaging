using System.Text.Json;
using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Producer;
using Cabazure.Messaging.EventHub.Internal;

namespace Cabazure.Messaging.EventHub.Tests.Internal;

public class EventHubPublisherTests
{
    public record TMessage(string Data);

    [Theory, AutoNSubstituteData]
    public async Task PublishAsync_Calls_SendAsync_On_Client(
        [Frozen, NoAutoProperties] JsonSerializerOptions serializerOptions,
        [Frozen, Substitute] EventHubProducerClient client,
        [Frozen, Substitute] Func<object, string> partitionKeySelector,
        EventHubPublisher<TMessage> sut,
        TMessage message,
        CancellationToken cancellationToken)
    {
        partitionKeySelector.Invoke(default).ReturnsNullForAnyArgs();
        await sut.PublishAsync(
            message,
            cancellationToken);

        _ = client
            .Received(1)
            .SendAsync(
                Arg.Any<IEnumerable<EventData>>(),
                cancellationToken);
    }

    [Theory, AutoNSubstituteData]
    public async Task PublishAsync_Calls_SendAsync_With_Options(
        [Frozen, NoAutoProperties] JsonSerializerOptions serializerOptions,
        [Frozen, Substitute] EventHubProducerClient client,
        EventHubPublisher<TMessage> sut,
        TMessage message,
        CancellationToken cancellationToken)
    {
        await sut.PublishAsync(
            message,
            cancellationToken);

        _ = client
            .Received(1)
            .SendAsync(
                Arg.Any<IEnumerable<EventData>>(),
                Arg.Any<SendEventOptions>(),
                cancellationToken);
    }

    [Theory, AutoNSubstituteData]
    public async Task PublishAsync_Sends_Serialized_Message(
        [Frozen, NoAutoProperties] JsonSerializerOptions serializerOptions,
        [Frozen, Substitute] EventHubProducerClient client,
        EventHubPublisher<TMessage> sut,
        TMessage message,
        CancellationToken cancellationToken)
    {
        await sut.PublishAsync(
            message,
            cancellationToken);

        var eventData = client
            .ReceivedCallWithArgument<IEnumerable<EventData>>()
            .Single();
        eventData.EventBody
            .ToObjectFromJson<TMessage>(
                serializerOptions)
            .Should()
            .BeEquivalentTo(message);
    }

    [Theory, AutoNSubstituteData]
    public async Task PublishAsync_Sends_Properties_From_Factory(
        [Frozen, NoAutoProperties] JsonSerializerOptions serializerOptions,
        [Frozen, Substitute] EventHubProducerClient client,
        [Frozen, Substitute] Func<object, Dictionary<string, object>> propertiesFactory,
        Dictionary<string, object> properties,
        EventHubPublisher<TMessage> sut,
        TMessage message,
        CancellationToken cancellationToken)
    {
        propertiesFactory.Invoke(message).Returns(properties);
        await sut.PublishAsync(
            message,
            cancellationToken);

        var eventData = client
            .ReceivedCallWithArgument<IEnumerable<EventData>>()
            .Single();
        eventData.Properties
            .Should()
            .BeEquivalentTo(properties);
    }

    [Theory, AutoNSubstituteData]
    public async Task PublishAsync_Sends_Metadate_From_PublishingOptions(
        [Frozen, NoAutoProperties] JsonSerializerOptions serializerOptions,
        [Frozen, Substitute] EventHubProducerClient client,
        EventHubPublisher<TMessage> sut,
        TMessage message,
        PublishingOptions options,
        CancellationToken cancellationToken)
    {
        await sut.PublishAsync(
            message,
            options,
            cancellationToken);

        var eventData = client
            .ReceivedCallWithArgument<IEnumerable<EventData>>()
            .Single();
        eventData.ContentType
            .Should()
            .BeEquivalentTo(options.ContentType);
        eventData.CorrelationId
            .Should()
            .BeEquivalentTo(options.CorrelationId);
        eventData.MessageId
            .Should()
            .BeEquivalentTo(options.MessageId);
        eventData.Properties
            .Should()
            .BeEquivalentTo(options.Properties);
    }

    [Theory, AutoNSubstituteData]
    public async Task PublishAsync_Sends_PartitionKey_From_Factory(
        [Frozen, NoAutoProperties] JsonSerializerOptions serializerOptions,
        [Frozen, Substitute] EventHubProducerClient client,
        [Frozen, Substitute] Func<object, string> partitionKeySelector,
        string partitionKey,
        EventHubPublisher<TMessage> sut,
        TMessage message,
        CancellationToken cancellationToken)
    {
        partitionKeySelector.Invoke(message).Returns(partitionKey);
        await sut.PublishAsync(
            message,
            cancellationToken);

        var sendOptions = client
            .ReceivedCallWithArgument<SendEventOptions>();
        sendOptions.PartitionKey
            .Should()
            .BeEquivalentTo(partitionKey);
    }

    [Theory, AutoNSubstituteData]
    public async Task PublishAsync_Sends_PartitionKey_From_PublishingOptions(
        [Frozen, NoAutoProperties] JsonSerializerOptions serializerOptions,
        [Frozen, Substitute] EventHubProducerClient client,
        EventHubPublisher<TMessage> sut,
        TMessage message,
        EventHubPublishingOptions options,
        CancellationToken cancellationToken)
    {
        await sut.PublishAsync(
            message,
            options,
            cancellationToken);

        var sendOptions = client
            .ReceivedCallWithArgument<SendEventOptions>();
        sendOptions.PartitionKey
            .Should()
            .BeEquivalentTo(options.PartitionKey);
        sendOptions.PartitionId
            .Should()
            .BeEquivalentTo(options.PartitionId);
    }
}
