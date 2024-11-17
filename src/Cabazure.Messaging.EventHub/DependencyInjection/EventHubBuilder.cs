using Azure.Messaging.EventHubs;
using Azure.Storage.Blobs;
using Cabazure.Messaging.EventHub.Internal;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace Cabazure.Messaging.EventHub.DependencyInjection;

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
        Services.TryAddSingleton<IEventHubProducerClientProvider, EventHubProducerClientProvider>();

        Services.AddSingleton(
            new EventHubPublisherRegistration(
                ConnectionName,
                typeof(T),
                eventHubName,
                publisherBuilder.GetPropertyFactory(),
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
        var processorBuilder = new EventHubProcessorBuilder();
        builder?.Invoke(processorBuilder);

        Services.AddSingleton<TProcessor>();
        Services.AddSingleton(s =>
        {
            var config = s
                .GetRequiredService<IOptionsMonitor<CabazureEventHubOptions>>()
                .Get(ConnectionName);

            var storageClient = config.BlobStorage switch
            {
                { ConnectionString: { } cs, ContainerName: { } cont } => new BlobContainerClient(cs, cont),
                { ContainerUri: { } uri, Credential: { } cred } => new BlobContainerClient(uri, cred),
                { ContainerUri: { } uri } => new BlobContainerClient(uri, config.Credential),

                _ => throw new ArgumentException(
                    $"Blob storage not configured for EventHub processor `{typeof(TProcessor).Name}`"),
            };

            if (config.BlobStorage.CreateIfNotExist)
            {
                storageClient.CreateIfNotExists();
            }

            var client = config switch
            {
                { FullyQualifiedNamespace: { } ns, Credential: { } cred }
                    => new EventProcessorClient(
                        checkpointStore: storageClient,
                        consumerGroup,
                        ns,
                        eventHubName,
                        cred,
                        processorBuilder.ClientOptions),
                { ConnectionString: { } cs }
                    => new EventProcessorClient(
                        checkpointStore: storageClient,
                        consumerGroup,
                        cs,
                        eventHubName,
                        processorBuilder.ClientOptions),
                _ => throw new ArgumentException(
                    $"Connection not configured for EventHub processor `{typeof(TProcessor).Name}`"),
            };

            return new EventHubProcessorService<TMessage, TProcessor>(
                s.GetRequiredService<TProcessor>(),
                client,
                config.SerializerOptions,
                processorBuilder.Filters);
        });
        Services.AddSingleton<IMessageProcessorService<TProcessor>>(s
            => s.GetRequiredService<EventHubProcessorService<TMessage, TProcessor>>());
        Services.AddHostedService(s
            => s.GetRequiredService<EventHubProcessorService<TMessage, TProcessor>>());

        return this;
    }
}
