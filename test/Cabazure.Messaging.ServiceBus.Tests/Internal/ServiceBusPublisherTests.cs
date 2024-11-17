using System.Text.Json;
using Azure.Messaging.ServiceBus;
using Cabazure.Messaging.ServiceBus.Internal;

namespace Cabazure.Messaging.ServiceBus.Tests.Internal;

public class ServiceBusPublisherTests
{
    public record TMessage(string Data);

    [Theory, AutoNSubstituteData]
    public async Task PublishAsync_Calls_SendAsync_On_Client(
        [Frozen, NoAutoProperties] JsonSerializerOptions serializerOptions,
        [Frozen, Substitute] ServiceBusSender sender,
        [Frozen, Substitute] Func<object, string> partitionKeySelector,
        ServiceBusPublisher<TMessage> sut,
        TMessage message,
        CancellationToken cancellationToken)
    {
        partitionKeySelector.Invoke(default).ReturnsNullForAnyArgs();
        await sut.PublishAsync(
            message,
            cancellationToken);

        _ = sender
            .Received(1)
            .SendMessageAsync(
                Arg.Any<ServiceBusMessage>(),
                cancellationToken);
    }

    [Theory, AutoNSubstituteData]
    public async Task PublishAsync_Calls_SendAsync_With_Options(
        [Frozen, NoAutoProperties] JsonSerializerOptions serializerOptions,
        [Frozen, Substitute] ServiceBusSender sender,
        ServiceBusPublisher<TMessage> sut,
        TMessage message,
        CancellationToken cancellationToken)
    {
        await sut.PublishAsync(
            message,
            cancellationToken);

        _ = sender
            .Received(1)
            .SendMessageAsync(
                Arg.Any<ServiceBusMessage>(),
                cancellationToken);
    }

    [Theory, AutoNSubstituteData]
    public async Task PublishAsync_Sends_Serialized_Message(
        [Frozen, NoAutoProperties] JsonSerializerOptions serializerOptions,
        [Frozen, Substitute] ServiceBusSender sender,
        ServiceBusPublisher<TMessage> sut,
        TMessage message,
        CancellationToken cancellationToken)
    {
        await sut.PublishAsync(
            message,
            cancellationToken);

        var eventData = sender
            .ReceivedCallWithArgument<ServiceBusMessage>();
        eventData.Body
            .ToObjectFromJson<TMessage>(
                serializerOptions)
            .Should()
            .BeEquivalentTo(message);
    }

    [Theory, AutoNSubstituteData]
    public async Task PublishAsync_Sends_Properties_From_Factory(
        [Frozen, NoAutoProperties] JsonSerializerOptions serializerOptions,
        [Frozen, Substitute] ServiceBusSender sender,
        [Frozen, Substitute] Func<object, Dictionary<string, object>> propertiesFactory,
        Dictionary<string, object> properties,
        ServiceBusPublisher<TMessage> sut,
        TMessage message,
        CancellationToken cancellationToken)
    {
        propertiesFactory.Invoke(message).Returns(properties);
        await sut.PublishAsync(
            message,
            cancellationToken);

        var eventData = sender
            .ReceivedCallWithArgument<ServiceBusMessage>();
        eventData.ApplicationProperties
            .Should()
            .BeEquivalentTo(properties);
    }

    [Theory, AutoNSubstituteData]
    public async Task PublishAsync_Sends_PartitionKey_From_Factory(
        [Frozen, NoAutoProperties] JsonSerializerOptions serializerOptions,
        [Frozen, Substitute] ServiceBusSender sender,
        [Frozen, Substitute] Func<object, string> partitionKeySelector,
        string partitionKey,
        ServiceBusPublisher<TMessage> sut,
        TMessage message,
        CancellationToken cancellationToken)
    {
        partitionKeySelector.Invoke(message).Returns(partitionKey);
        await sut.PublishAsync(
            message,
            cancellationToken);

        var eventData = sender
            .ReceivedCallWithArgument<ServiceBusMessage>();
        eventData.PartitionKey
            .Should()
            .BeEquivalentTo(partitionKey);
    }

    [Theory, AutoNSubstituteData]
    public async Task PublishAsync_Sends_Metadate_From_PublishingOptions(
        [Frozen, NoAutoProperties] JsonSerializerOptions serializerOptions,
        [Frozen, Substitute] ServiceBusSender sender,
        ServiceBusPublisher<TMessage> sut,
        TMessage message,
        PublishingOptions options,
        CancellationToken cancellationToken)
    {
        await sut.PublishAsync(
            message,
            options,
            cancellationToken);

        var eventData = sender
            .ReceivedCallWithArgument<ServiceBusMessage>();
        eventData.ContentType
            .Should()
            .BeEquivalentTo(options.ContentType);
        eventData.CorrelationId
            .Should()
            .BeEquivalentTo(options.CorrelationId);
        eventData.MessageId
            .Should()
            .BeEquivalentTo(options.MessageId);
        eventData.ApplicationProperties
            .Should()
            .BeEquivalentTo(options.Properties);
        eventData.PartitionKey
            .Should()
            .BeEquivalentTo(options.PartitionKey);
    }

    [Theory, AutoNSubstituteData]
    public async Task PublishAsync_Sends_Metadate_From_ServicePublishingOptions(
        [Frozen, NoAutoProperties] JsonSerializerOptions serializerOptions,
        [Frozen, Substitute] ServiceBusSender sender,
        ServiceBusPublisher<TMessage> sut,
        TMessage message,
        ServiceBusPublishingOptions options,
        CancellationToken cancellationToken)
    {
        await sut.PublishAsync(
            message,
            options,
            cancellationToken);

        var eventData = sender
            .ReceivedCallWithArgument<ServiceBusMessage>();
        eventData.SessionId
            .Should()
            .Be(options.SessionId);
        eventData.TimeToLive
            .Should()
            .Be(options.TimeToLive!.Value);
        eventData.ScheduledEnqueueTime
            .Should()
            .Be(options.ScheduledEnqueueTime!.Value);
    }
}
