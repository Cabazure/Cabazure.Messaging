using Azure.Messaging.ServiceBus;
using Cabazure.Messaging.ServiceBus.Internal;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Cabazure.Messaging.ServiceBus.DependencyInjection;

/// <summary>
/// Provides a fluent API for configuring Azure Service Bus publishers and processors in the dependency injection container.
/// </summary>
public class ServiceBusBuilder(
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
    /// Configures the Service Bus options using the provided configuration action.
    /// </summary>
    /// <param name="configure">The action to configure the Service Bus options.</param>
    /// <returns>The current builder instance for method chaining.</returns>
    public ServiceBusBuilder Configure(
        Action<CabazureServiceBusOptions> configure)
    {
        Services.Configure(ConnectionName, configure);
        return this;
    }

    /// <summary>
    /// Configures the Service Bus options using the specified configure options class.
    /// </summary>
    /// <typeparam name="TConfigureOptions">The type that implements IConfigureOptions for CabazureServiceBusOptions.</typeparam>
    /// <returns>The current builder instance for method chaining.</returns>
    public ServiceBusBuilder Configure<TConfigureOptions>()
        where TConfigureOptions : class, IConfigureOptions<CabazureServiceBusOptions>
    {
        Services.ConfigureOptions<TConfigureOptions>();
        return this;
    }

    /// <summary>
    /// Adds a Service Bus publisher for the specified message type.
    /// </summary>
    /// <typeparam name="T">The type of message that the publisher will handle.</typeparam>
    /// <param name="topicOrQueueName">The name of the Service Bus topic or queue to publish messages to.</param>
    /// <param name="builder">Optional action to configure the publisher-specific settings.</param>
    /// <returns>The current builder instance for method chaining.</returns>
    public ServiceBusBuilder AddPublisher<T>(
        string topicOrQueueName,
        Action<ServiceBusPublisherBuilder<T>>? builder = null)
    {
        var publisherBuilder = new ServiceBusPublisherBuilder<T>();
        builder?.Invoke(publisherBuilder);

        Services.TryAddSingleton<IServiceBusClientProvider, ServiceBusClientProvider>();
        Services.TryAddSingleton<IServiceBusSenderProvider, ServiceBusSenderProvider>();
        Services.TryAddSingleton<IServiceBusPublisherFactory, ServiceBusPublisherFactory>();

        Services.AddSingleton(
            new ServiceBusPublisherRegistration(
                ConnectionName,
                typeof(T),
                topicOrQueueName,
                publisherBuilder.SenderOptions,
                publisherBuilder.GetEventDataModifier()));
        Services.AddSingleton(s => s
            .GetRequiredService<IServiceBusPublisherFactory>()
            .Create<T>());
        Services.AddSingleton<IMessagePublisher<T>>(s => s
            .GetRequiredService<IServiceBusPublisher<T>>());

        return this;
    }

    /// <summary>
    /// Adds a Service Bus processor for the specified message and processor types using a topic and subscription.
    /// </summary>
    /// <typeparam name="TMessage">The type of message that the processor will handle.</typeparam>
    /// <typeparam name="TProcessor">The type of processor that will process the messages.</typeparam>
    /// <param name="topicName">The name of the Service Bus topic to consume messages from.</param>
    /// <param name="subscriptionName">The name of the topic subscription to consume messages from.</param>
    /// <param name="builder">Optional action to configure the processor-specific settings.</param>
    /// <returns>The current builder instance for method chaining.</returns>
    public ServiceBusBuilder AddProcessor<TMessage, TProcessor>(
        string topicName,
        string subscriptionName,
        Action<ServiceBusProcessorBuilder>? builder = null)
        where TProcessor : class, IMessageProcessor<TMessage>
        => AddProcessorInternal<TMessage, TProcessor>(
            (f, o) => f.Create(ConnectionName, topicName, subscriptionName, o),
            builder);

    /// <summary>
    /// Adds a Service Bus processor for the specified message and processor types using a queue.
    /// </summary>
    /// <typeparam name="TMessage">The type of message that the processor will handle.</typeparam>
    /// <typeparam name="TProcessor">The type of processor that will process the messages.</typeparam>
    /// <param name="queueName">The name of the Service Bus queue to consume messages from.</param>
    /// <param name="builder">Optional action to configure the processor-specific settings.</param>
    /// <returns>The current builder instance for method chaining.</returns>
    public ServiceBusBuilder AddProcessor<TMessage, TProcessor>(
        string queueName,
        Action<ServiceBusProcessorBuilder>? builder = null)
        where TProcessor : class, IMessageProcessor<TMessage>
        => AddProcessorInternal<TMessage, TProcessor>(
            (f, o) => f.Create(ConnectionName, queueName, o),
            builder);

    private ServiceBusBuilder AddProcessorInternal<TMessage, TProcessor>(
        Func<IServiceBusProcessorFactory, ServiceBusProcessorOptions?, IServiceBusProcessor> processorFactory,
        Action<ServiceBusProcessorBuilder>? builder = null)
        where TProcessor : class, IMessageProcessor<TMessage>
    {
        var processorBuilder = new ServiceBusProcessorBuilder();
        builder?.Invoke(processorBuilder);

        Services.AddLogging();
        Services.TryAddSingleton<IServiceBusClientProvider, ServiceBusClientProvider>();
        Services.TryAddSingleton<IServiceBusProcessorFactory, ServiceBusProcessorFactory>();
        Services.TryAddSingleton<TProcessor>();

        Services.AddSingleton(s =>
        {
            var config = s
                .GetRequiredService<IOptionsMonitor<CabazureServiceBusOptions>>()
                .Get(ConnectionName);

            var factory = s
                .GetRequiredService<IServiceBusProcessorFactory>();

            var processor = processorFactory.Invoke(
                factory,
                processorBuilder.ProcessorOptions);

            return new ServiceBusProcessorService<TMessage, TProcessor>(
                s.GetRequiredService<ILogger<TProcessor>>(),
                s.GetRequiredService<TProcessor>(),
                processor,
                config.SerializerOptions,
                processorBuilder.Filters);
        });
        Services.AddSingleton<IMessageProcessorService<TProcessor>>(s
            => s.GetRequiredService<ServiceBusProcessorService<TMessage, TProcessor>>());
        Services.AddHostedService(s
            => s.GetRequiredService<ServiceBusProcessorService<TMessage, TProcessor>>());

        return this;
    }
}
