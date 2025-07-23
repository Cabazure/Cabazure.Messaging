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
    /// <summary>
    /// Gets the service collection being configured.
    /// </summary>
    public IServiceCollection Services { get; } = services;

    /// <summary>
    /// Gets the name of the connection configuration, or null for the default connection.
    /// </summary>
    public string? ConnectionName { get; } = connectionName;

    /// <summary>
    /// Configures the Event Hub options using the provided configuration action.
    /// </summary>
    /// <param name="configure">The action to configure the Event Hub options.</param>
    /// <returns>The current builder instance for method chaining.</returns>
    public EventHubBuilder Configure(
        Action<CabazureEventHubOptions> configure)
    {
        Services.Configure(ConnectionName, configure);
        return this;
    }

    /// <summary>
    /// Configures the Event Hub options using the specified configure options class.
    /// </summary>
    /// <typeparam name="TConfigureOptions">The type that implements IConfigureOptions for CabazureEventHubOptions.</typeparam>
    /// <returns>The current builder instance for method chaining.</returns>
    public EventHubBuilder Configure<TConfigureOptions>()
        where TConfigureOptions : class, IConfigureOptions<CabazureEventHubOptions>
    {
        Services.ConfigureOptions<TConfigureOptions>();
        return this;
    }

    /// <summary>
    /// Adds an Event Hub publisher for the specified message type.
    /// </summary>
    /// <typeparam name="T">The type of message that the publisher will handle.</typeparam>
    /// <param name="eventHubName">The name of the Event Hub to publish messages to.</param>
    /// <param name="builder">Optional action to configure the publisher-specific settings.</param>
    /// <returns>The current builder instance for method chaining.</returns>
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

    /// <summary>
    /// Adds an Event Hub processor for the specified message and processor types.
    /// </summary>
    /// <typeparam name="TMessage">The type of message that the processor will handle.</typeparam>
    /// <typeparam name="TProcessor">The type of processor that will process the messages.</typeparam>
    /// <param name="eventHubName">The name of the Event Hub to consume messages from.</param>
    /// <param name="consumerGroup">The consumer group name. Defaults to "$default".</param>
    /// <param name="builder">Optional action to configure the processor-specific settings.</param>
    /// <returns>The current builder instance for method chaining.</returns>
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

    /// <summary>
    /// Adds a stateless Event Hub processor for the specified message and processor types.
    /// Stateless processors do not use checkpointing and read from the latest available messages.
    /// </summary>
    /// <typeparam name="TMessage">The type of message that the processor will handle.</typeparam>
    /// <typeparam name="TProcessor">The type of processor that will process the messages.</typeparam>
    /// <param name="eventHubName">The name of the Event Hub to consume messages from.</param>
    /// <param name="consumerGroup">The consumer group name. Defaults to "$default".</param>
    /// <param name="builder">Optional action to configure the stateless processor-specific settings.</param>
    /// <returns>The current builder instance for method chaining.</returns>
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
