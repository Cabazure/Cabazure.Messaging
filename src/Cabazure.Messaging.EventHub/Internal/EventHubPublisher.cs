﻿using System.Text.Json;
using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Producer;

namespace Cabazure.Messaging.EventHub.Internal;

public class EventHubPublisher<TMessage>(
    JsonSerializerOptions serializerOptions,
    EventHubProducerClient producer,
    Action<object, EventData>? eventDataModifier,
    Func<object, string>? partitionKeyFactory)
    : IEventHubPublisher<TMessage>
    , IMessagePublisher<TMessage>
{
    public async Task PublishAsync(
        TMessage message,
        EventHubPublishingOptions options,
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
            null,
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
        PublishingOptions? options,
        CancellationToken cancellationToken)
    {
        var json = JsonSerializer.Serialize(
            message,
            serializerOptions);
        var eventData = new EventData(json);

        eventDataModifier?.Invoke(message!, eventData);
        ConfigureEventData(eventData, options);

        var partitionKey = partitionKeyFactory?.Invoke(message!);
        if (GetSendEventOptions(options, partitionKey) is { } sendOptions)
        {
            await producer.SendAsync(
                [eventData],
                sendOptions,
                cancellationToken);
        }
        else
        {
            await producer.SendAsync(
                [eventData],
                cancellationToken);
        }
    }

    private static void ConfigureEventData(
        EventData eventData,
        PublishingOptions? options)
    {
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
