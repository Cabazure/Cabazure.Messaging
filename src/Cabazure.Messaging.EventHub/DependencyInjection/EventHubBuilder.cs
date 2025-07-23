using Cabazure.Messaging.EventHub.Internal;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Cabazure.Messaging.EventHub.DependencyInjection;

/// <summary>
/// Provides a fluent API for configuring Azure Event Hub publishers and processors in the dependency injection container.
/// </summary>
public class EventHubBuilder(
    IServiceCollection services,
    string? connectionName)
{
    public IServiceCollection Services { get; } = services;

    public string? ConnectionName { get; } = connectionName;

    public EventHubBuilder Configure(
        Action<CabazureEventHubOptions> configure)
    {
        Services.Configure(ConnectionName, configure);
        return this;
    }

    public EventHubBuilder Configure<TConfigureOptions>()
        where TConfigureOptions : class, IConfigureOptions<CabazureEventHubOptions>
    {
        Services.ConfigureOptions<TConfigureOptions>();
        return this;
    }

    public EventHubBuilder AddPublisher<T>(
        string eventHubName,
        Action<EventHubPublisherBuilder<T>>? builder = null)
    {
        var publisherBuilder = new EventHubPublisherBuilder<T>();
        builder?.Invoke(publisherBuilder);

        Services.TryAddSingleton<IEventHubPublisherFactory, EventHubPublisherFactory>();
        Services.TryAddSingleton<IEventHubProducerProvider, EventHubProducerProvider>();

        Services.AddSingleton(
            new EventHubPublisherRegistration(
                ConnectionName,
                typeof(T),
                eventHubName,
                publisherBuilder.GetEventDataModifier(),
                publisherBuilder.GetPartitionKeyFactory()));
        Services.AddSingleton(s => s
            .GetRequiredService<IEventHubPublisherFactory>()
            .Create<T>());
        Services.AddSingleton<IMessagePublisher<T>>(s => s
            .GetRequiredService<IEventHubPublisher<T>>());

        return this;
    }

    public EventHubBuilder AddProcessor<TMessage, TProcessor>(
        string eventHubName,
        string consumerGroup = "$default",
        Action<EventHubProcessorBuilder>? builder = null)
        where TProcessor : class, IMessageProcessor<TMessage>
    {
        var processorBuilder = new EventHubProcessorBuilder(eventHubName);
        builder?.Invoke(processorBuilder);

        Services.AddLogging();
        Services.TryAddSingleton<IBlobStorageClientProvider, BlobStorageClientProvider>();
        Services.TryAddSingleton<IEventHubProcessorFactory, EventHubProcessorFactory>();
        Services.TryAddSingleton<TProcessor>();

        Services.AddSingleton(s =>
        {
            var config = s
                .GetRequiredService<IOptionsMonitor<CabazureEventHubOptions>>()
                .Get(ConnectionName);

            var batchHandler = new EventHubBatchHandler<TMessage, TProcessor>(
                s.GetRequiredService<ILogger<TProcessor>>(),
                s.GetRequiredService<TProcessor>(),
                config.SerializerOptions,
                processorBuilder.Filters);

            var batchProcessor = s
                .GetRequiredService<IEventHubProcessorFactory>()
                .Create(
                    batchHandler,
                    ConnectionName,
                    eventHubName,
                    consumerGroup,
                    processorBuilder.BlobContainer,
                    processorBuilder.ProcessorOptions);

            return new EventHubProcessorService<TMessage, TProcessor>(batchProcessor);
        });
        Services.AddSingleton<IMessageProcessorService<TProcessor>>(s
            => s.GetRequiredService<EventHubProcessorService<TMessage, TProcessor>>());
        Services.AddHostedService(s
            => s.GetRequiredService<EventHubProcessorService<TMessage, TProcessor>>());

        return this;
    }

    public EventHubBuilder AddStatelessProcessor<TMessage, TProcessor>(
        string eventHubName,
        string consumerGroup = "$default",
        Action<EventHubStatelessProcessorBuilder>? builder = null)
        where TProcessor : class, IMessageProcessor<TMessage>
    {
        var processorBuilder = new EventHubStatelessProcessorBuilder();
        builder?.Invoke(processorBuilder);

        Services.AddLogging();
        Services.TryAddSingleton<IEventHubConsumerClientFactory, EventHubConsumerClientFactory>();
        Services.TryAddSingleton<TProcessor>();

        Services.AddSingleton(s =>
        {
            var config = s
                .GetRequiredService<IOptionsMonitor<CabazureEventHubOptions>>()
                .Get(ConnectionName);

            var client = s
                .GetRequiredService<IEventHubConsumerClientFactory>()
                .Create(ConnectionName, eventHubName, consumerGroup);

            var processor = new EventHubStatelessProcessor<TMessage, TProcessor>(
                client,
                s.GetRequiredService<ILogger<TProcessor>>(),
                s.GetRequiredService<TProcessor>(),
                config.SerializerOptions,
                processorBuilder.ReadOptions,
                processorBuilder.Filters);

            return new EventHubProcessorService<TMessage, TProcessor>(processor);
        });
        Services.AddSingleton<IMessageProcessorService<TProcessor>>(s
            => s.GetRequiredService<EventHubProcessorService<TMessage, TProcessor>>());
        Services.AddHostedService(s
            => s.GetRequiredService<EventHubProcessorService<TMessage, TProcessor>>());

        return this;
    }
}
