using System.Text.Json;
using Azure.Messaging.ServiceBus;

namespace Cabazure.Messaging.ServiceBus.Internal;

public class ServiceBusPublisher<TMessage>(
    JsonSerializerOptions serializerOptions,
    ServiceBusSender sender,
    Action<object, ServiceBusMessage>? eventDataModifier)
    : IServiceBusPublisher<TMessage>
    , IMessagePublisher<TMessage>
{
    public async Task PublishAsync(
        TMessage message,
        ServiceBusPublishingOptions options,
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
            new PublishingOptions(),
            cancellationToken);

    public async Task PublishAsync(
        TMessage message,
        PublishingOptions options,
        CancellationToken cancellationToken)
        => await PerformPublishAsync(
            message,
            options,
            cancellationToken);

    private async Task PerformPublishAsync(
        TMessage message,
        PublishingOptions options,
        CancellationToken cancellationToken)
    {
        var json = JsonSerializer.Serialize(
            message,
            serializerOptions);
        var eventData = new ServiceBusMessage(json);

        eventDataModifier?.Invoke(message!, eventData);
        ConfigureEventData(eventData, options);

        await sender.SendMessageAsync(
            eventData,
            cancellationToken);
    }

    private static void ConfigureEventData(
        ServiceBusMessage eventData,
        PublishingOptions? options)
    {
        if (options == null)
        {
            return;
        }

        foreach (var property in options.Properties)
        {
            eventData.ApplicationProperties[property.Key] = property.Value;
        }

        if (options.MessageId != null)
        {
            eventData.MessageId = options.MessageId;
        }

        if (options.CorrelationId != null)
        {
            eventData.CorrelationId = options.CorrelationId;
        }

        if (options.ContentType != null)
        {
            eventData.ContentType = options.ContentType;
        }

        if (options.PartitionKey != null)
        {
            eventData.PartitionKey = options.PartitionKey;
        }

        if (options is not ServiceBusPublishingOptions sbOptions)
        {
            return;
        }

        if (sbOptions.SessionId != null)
        {
            eventData.SessionId = sbOptions.SessionId;
        }

        if (sbOptions.TimeToLive != null)
        {
            eventData.TimeToLive = sbOptions.TimeToLive.Value;
        }

        if (sbOptions.ScheduledEnqueueTime != null)
        {
            eventData.ScheduledEnqueueTime = sbOptions.ScheduledEnqueueTime.Value;
        }
    }
}
