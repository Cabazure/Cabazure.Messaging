using Azure.Messaging.EventHubs;
using Azure.Storage.Blobs;
using Cabazure.Messaging.EventHub.Processing;
using Cabazure.Messaging.EventHub.Publishing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace Cabazure.Messaging.EventHub.DependencyInjection;

public class EventHubBuilder(
    IServiceCollection services,
    string? connectionName)
{
    public EventHubBuilder Configure(
        Action<CabazureEventHubOptions> configure)
    {
        services.Configure(connectionName, configure);
        return this;
    }

    public EventHubBuilder Configure<TConfigureOptions>()
        where TConfigureOptions : class, IConfigureNamedOptions<CabazureEventHubOptions>
    {
        services.ConfigureOptions<TConfigureOptions>();
        return this;
    }

    public EventHubBuilder AddPublisher<T>(
        string eventHubName,
        Action<EventHubPublisherBuilder<T>>? builder = null)
    {
        Func<object, Dictionary<string, object>>? propertiesFactory = null;
        Func<object, string>? partitionKeyFactory = null;
        if (builder != null)
        {
            var publisherBuilder = new EventHubPublisherBuilder<T>();
            builder.Invoke(publisherBuilder);

            if (publisherBuilder.Properties.Count > 0)
            {
                propertiesFactory = delegate (object obj)
                {
                    var message = (T)obj;
                    return publisherBuilder.Properties
                        .ToDictionary(
                            kvp => kvp.Key,
                            kvp => kvp.Value(message));
                };
            }

            if (publisherBuilder.PartitionKey != null)
            {
                partitionKeyFactory = delegate (object obj)
                {
                    var message = (T)obj;
                    return publisherBuilder.PartitionKey.Invoke(message);
                };
            }
        }

        services.AddSingleton(new EventHubPublisherRegistration(
            connectionName,
            typeof(T),
            eventHubName,
            propertiesFactory,
            partitionKeyFactory));

        services.TryAddSingleton<IEventHubPublisherFactory, EventHubPublisherFactory>();

        services.AddSingleton(s => s
            .GetRequiredService<IEventHubPublisherFactory>()
            .Create<T>());
        services.AddSingleton<IMessagePublisher<T>>(s => s
            .GetRequiredService<IEventHubPublisher<T>>());

        return this;
    }

    public EventHubBuilder AddProcessor<TMessage, TProcessor>(
        string eventHubName,
        string consumerGroup = "$default",
        Action<EventHubProcessorBuilder>? builder = null)
        where TProcessor : class, IMessageProcessor<TMessage>
    {
        var publisherBuilder = new EventHubProcessorBuilder();
        builder?.Invoke(publisherBuilder);

        services.AddSingleton<TProcessor>();
        services.AddSingleton(s =>
        {
            var config = s
                .GetRequiredService<IOptionsMonitor<CabazureEventHubOptions>>()
                .Get(connectionName);

            var storageClient = config.BlobStorage switch
            {
                { ConnectionString: { } cs, ContainerName: { } cont } => new BlobContainerClient(cs, cont),
                { ContainerUri: { } uri, Credential: { } cred } => new BlobContainerClient(uri, cred),
                { ContainerUri: { } uri } => new BlobContainerClient(uri, config.Credential),

                _ => throw new ArgumentException(
                    $"Blob storage not configured for processor `{typeof(TProcessor).Name}`"),
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
                        publisherBuilder.ClientOptions),
                { ConnectionString: { } cs }
                    => new EventProcessorClient(
                        checkpointStore: storageClient,
                        consumerGroup,
                        cs,
                        eventHubName,
                        publisherBuilder.ClientOptions),
                _ => throw new ArgumentException(
                    $"Connection not configured for processor `{typeof(TProcessor).Name}`"),
            };

            return new EventHubProcessorService<TMessage, TProcessor>(
                s.GetRequiredService<TProcessor>(),
                client,
                config.SerializerOptions,
                publisherBuilder.Filters);
        });
        services.AddSingleton<IMessageProcessorService<TProcessor>>(s
            => s.GetRequiredService<EventHubProcessorService<TMessage, TProcessor>>());
        services.AddHostedService(s
            => s.GetRequiredService<EventHubProcessorService<TMessage, TProcessor>>());

        return this;
    }
}
