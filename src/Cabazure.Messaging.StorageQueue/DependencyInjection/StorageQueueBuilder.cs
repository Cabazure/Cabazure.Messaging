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
    public IServiceCollection Services { get; } = services;

    public string? ConnectionName { get; } = connectionName;

    public StorageQueueBuilder Configure(
        Action<CabazureStorageQueueOptions> configure)
    {
        Services.Configure(ConnectionName, configure);
        return this;
    }

    public StorageQueueBuilder Configure<TConfigureOptions>()
        where TConfigureOptions : class, IConfigureOptions<CabazureStorageQueueOptions>
    {
        Services.ConfigureOptions<TConfigureOptions>();
        return this;
    }

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
