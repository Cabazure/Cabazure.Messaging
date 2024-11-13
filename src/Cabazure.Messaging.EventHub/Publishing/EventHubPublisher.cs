using System.Text.Json;
using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Producer;

namespace Cabazure.Messaging.EventHub.Publishing;

public class EventHubPublisher<T>(
    JsonSerializerOptions serializerOptions,
    EventHubProducerClient client,
    Func<object, Dictionary<string, object>>? propertiesFactory,
    Func<object, string>? partitionKeyFactory)
    : IEventHubPublisher<T>
    , IMessagePublisher<T>
{
    public async Task PublishAsync(
        T message,
        EventHubPublishingOptions options,
        CancellationToken cancellationToken)
        => await PerformPublishAsync(
            message,
            options,
            cancellationToken);

    public async Task PublishAsync(
        T message,
        CancellationToken cancellationToken)
        => await PerformPublishAsync(
            message,
            null,
            cancellationToken);

    public async Task PublishAsync(
        T message,
        PublishingOptions options,
        CancellationToken cancellationToken)
        => await PerformPublishAsync(
            message,
            options,
            cancellationToken);

    private async Task PerformPublishAsync(
        T message,
        PublishingOptions? options,
        CancellationToken cancellationToken)
    {
        var json = JsonSerializer.Serialize(
            message,
            serializerOptions);
        var eventData = new EventData(json);

        var properties = propertiesFactory?.Invoke(message!);
        ConfigureEventData(
            eventData,
            options,
            properties);

        var partitionKey = partitionKeyFactory?.Invoke(message!);
        if (GetSendEventOptions(options, partitionKey) is { } sendOptions)
        {
            await client.SendAsync(
                [eventData],
                sendOptions,
                cancellationToken);
        }
        else
        {
            await client.SendAsync(
                [eventData],
                cancellationToken);
        }
    }

    private static void ConfigureEventData(
        EventData eventData,
        PublishingOptions? options,
        Dictionary<string, object>? properties)
    {
        if (properties != null)
        {
            foreach (var property in properties)
            {
                eventData.Properties[property.Key] = property.Value;
            }
        }

        if (options == null)
        {
            return;
        }

        foreach (var property in options.Properties)
        {
            eventData.Properties[property.Key] = property.Value;
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
    }

    private static SendEventOptions? GetSendEventOptions(
        PublishingOptions? options,
        string? partitionKey)
    {
        SendEventOptions? sendOptions = null;
        if (options?.PartitionKey != null)
        {
            sendOptions ??= new();
            sendOptions.PartitionKey = options.PartitionKey;
        }
        else if (partitionKey != null)
        {
            sendOptions ??= new();
            sendOptions.PartitionKey = partitionKey;
        }

        if (options is EventHubPublishingOptions { PartitionId: { } pid })
        {
            sendOptions ??= new();
            sendOptions.PartitionId = pid;
        }

        return sendOptions;
    }
}
