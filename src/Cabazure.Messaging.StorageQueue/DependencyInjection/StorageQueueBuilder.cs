using Cabazure.Messaging.StorageQueue.Internal;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Cabazure.Messaging.StorageQueue.DependencyInjection;

/// <summary>
/// Provides a fluent API for configuring Azure Storage Queue publishers and processors in the dependency injection container.
/// </summary>
public class StorageQueueBuilder(
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
    /// Configures the Storage Queue options using the provided configuration action.
    /// </summary>
    /// <param name="configure">The action to configure the Storage Queue options.</param>
    /// <returns>The current builder instance for method chaining.</returns>
    public StorageQueueBuilder Configure(
        Action<CabazureStorageQueueOptions> configure)
    {
        Services.Configure(ConnectionName, configure);
        return this;
    }

    /// <summary>
    /// Configures the Storage Queue options using the specified configure options class.
    /// </summary>
    /// <typeparam name="TConfigureOptions">The type that implements IConfigureOptions for CabazureStorageQueueOptions.</typeparam>
    /// <returns>The current builder instance for method chaining.</returns>
    public StorageQueueBuilder Configure<TConfigureOptions>()
        where TConfigureOptions : class, IConfigureOptions<CabazureStorageQueueOptions>
    {
        Services.ConfigureOptions<TConfigureOptions>();
        return this;
    }

    /// <summary>
    /// Adds a Storage Queue publisher for the specified message type.
    /// </summary>
    /// <typeparam name="T">The type of message that the publisher will handle.</typeparam>
    /// <param name="queueName">The name of the Storage Queue to publish messages to.</param>
    /// <returns>The current builder instance for method chaining.</returns>
    public StorageQueueBuilder AddPublisher<T>(
        string queueName)
    {
        Services.TryAddSingleton<IStorageQueueClientProvider, StorageQueueClientProvider>();
        Services.TryAddSingleton<IStorageQueuePublisherFactory, StorageQueuePublisherFactory>();

        Services.AddSingleton(
            new StorageQueuePublisherRegistration(
                ConnectionName,
                typeof(T),
                queueName));
        Services.AddSingleton(s => s
            .GetRequiredService<IStorageQueuePublisherFactory>()
            .Create<T>());
        Services.AddSingleton<IMessagePublisher<T>>(s => s
            .GetRequiredService<IStorageQueuePublisher<T>>());

        return this;
    }

    /// <summary>
    /// Adds a Storage Queue processor for the specified message and processor types.
    /// </summary>
    /// <typeparam name="TMessage">The type of message that the processor will handle.</typeparam>
    /// <typeparam name="TProcessor">The type of processor that will process the messages.</typeparam>
    /// <param name="queueName">The name of the Storage Queue to consume messages from.</param>
    /// <param name="builder">Optional action to configure the processor-specific settings.</param>
    /// <returns>The current builder instance for method chaining.</returns>
    public StorageQueueBuilder AddProcessor<TMessage, TProcessor>(
        string queueName,
        Action<StorageQueueProcessorBuilder>? builder = null)
        where TProcessor : class, IMessageProcessor<TMessage>
    {
        var processorBuilder = new StorageQueueProcessorBuilder();
        builder?.Invoke(processorBuilder);

        Services.AddLogging();
        Services.TryAddSingleton<IStorageQueueClientProvider, StorageQueueClientProvider>();
        Services.TryAddSingleton<TProcessor>();

        Services.AddSingleton(s =>
        {
            var config = s
                .GetRequiredService<IOptionsMonitor<CabazureStorageQueueOptions>>()
                .Get(ConnectionName);

            var client = s
                .GetRequiredService<IStorageQueueClientProvider>()
                .GetClient(ConnectionName)
                .GetQueueClient(queueName);

            return new StorageQueueProcessorService<TMessage, TProcessor>(
                TimeProvider.System,
                s.GetRequiredService<ILogger<TProcessor>>(),
                s.GetRequiredService<TProcessor>(),
                processorBuilder.Options,
                config.SerializerOptions,
                client);
        });
        Services.AddSingleton<IMessageProcessorService<TProcessor>>(s
            => s.GetRequiredService<StorageQueueProcessorService<TMessage, TProcessor>>());
        Services.AddHostedService(s
            => s.GetRequiredService<StorageQueueProcessorService<TMessage, TProcessor>>());

        return this;
    }
}
