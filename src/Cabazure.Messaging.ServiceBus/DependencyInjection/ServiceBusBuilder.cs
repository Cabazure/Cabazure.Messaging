using Azure.Messaging.ServiceBus;
using Cabazure.Messaging.ServiceBus.Processing;
using Cabazure.Messaging.ServiceBus.Publishing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace Cabazure.Messaging.ServiceBus.DependencyInjection;

public class ServiceBusBuilder(
    IServiceCollection services,
    string? connectionName)
{
    public ServiceBusBuilder Configure(
        Action<CabazureServiceBusOptions> configure)
    {
        services.Configure(connectionName, configure);
        return this;
    }

    public ServiceBusBuilder Configure<TConfigureOptions>()
        where TConfigureOptions : class, IConfigureOptions<CabazureServiceBusOptions>
    {
        services.ConfigureOptions<TConfigureOptions>();
        return this;
    }

    public ServiceBusBuilder AddPublisher<T>(
        string topicOrQueueName,
        Action<ServiceBusPublisherBuilder<T>>? builder = null)
    {
        Func<object, Dictionary<string, object>>? propertiesFactory = null;
        Func<object, string>? partitionKeyFactory = null;
        if (builder != null)
        {
            var publisherBuilder = new ServiceBusPublisherBuilder<T>();
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

        services.TryAddSingleton<IServiceBusClientProvider, ServiceBusClientProvider>();
        services.TryAddSingleton<IServiceBusSenderProvider, ServiceBusSenderProvider>();
        services.TryAddSingleton<IServiceBusPublisherFactory, ServiceBusPublisherFactory>();

        services.AddSingleton(new ServiceBusPublisherRegistration(
            connectionName,
            typeof(T),
            topicOrQueueName,
            propertiesFactory,
            partitionKeyFactory));
        services.AddSingleton(s => s
            .GetRequiredService<IServiceBusPublisherFactory>()
            .Create<T>());
        services.AddSingleton<IMessagePublisher<T>>(s => s
            .GetRequiredService<IServiceBusPublisher<T>>());

        return this;
    }

    public ServiceBusBuilder AddProcessor<TMessage, TProcessor>(
        string topicName,
        string subscriptionName,
        Action<ServiceBusProcessorBuilder>? builder = null)
        where TProcessor : class, IMessageProcessor<TMessage>
        => AddProcessorInternal<TMessage, TProcessor>(
            topicName,
            subscriptionName,
            builder);

    public ServiceBusBuilder AddProcessor<TMessage, TProcessor>(
        string queueName,
        Action<ServiceBusProcessorBuilder>? builder = null)
        where TProcessor : class, IMessageProcessor<TMessage>
        => AddProcessorInternal<TMessage, TProcessor>(
            queueName,
            subscriptionName: null,
            builder);

    private ServiceBusBuilder AddProcessorInternal<TMessage, TProcessor>(
        string topicOrQueueName,
        string? subscriptionName,
        Action<ServiceBusProcessorBuilder>? builder)
        where TProcessor : class, IMessageProcessor<TMessage>
    {
        var processorBuilder = new ServiceBusProcessorBuilder();
        builder?.Invoke(processorBuilder);

        services.AddSingleton<TProcessor>();
        services.AddSingleton(s =>
        {
            var config = s
                .GetRequiredService<IOptionsMonitor<CabazureServiceBusOptions>>()
                .Get(connectionName);

            var client = config switch
            {
                { FullyQualifiedNamespace: { } ns, Credential: { } cred }
                    => new ServiceBusClient(ns, cred, processorBuilder.ClientOptions),
                { ConnectionString: { } cs }
                    => new ServiceBusClient(cs, processorBuilder.ClientOptions),
                _ => throw new ArgumentException(
                    $"Connection not configured for ServiceBus processor `{typeof(TProcessor).Name}`"),
            };

            var processor = (topicOrQueueName, subscriptionName) switch
            {
                ({ } topic, { } subscription)
                    => client.CreateProcessor(
                        topic,
                        subscription,
                        processorBuilder.ProcessorOptions),
                ({ } queue, _)
                    => client.CreateProcessor(
                        queue,
                        processorBuilder.ProcessorOptions),
            };

            return new ServiceBusProcessorService<TMessage, TProcessor>(
                s.GetRequiredService<TProcessor>(),
                processor,
                config.SerializerOptions,
                processorBuilder.Filters);
        });
        services.AddSingleton<IMessageProcessorService<TProcessor>>(s
            => s.GetRequiredService<ServiceBusProcessorService<TMessage, TProcessor>>());
        services.AddHostedService(s
            => s.GetRequiredService<ServiceBusProcessorService<TMessage, TProcessor>>());

        return this;
    }
}
