using Azure.Messaging.ServiceBus;
using Cabazure.Messaging.ServiceBus.Internal;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace Cabazure.Messaging.ServiceBus.DependencyInjection;

public class ServiceBusBuilder(
    IServiceCollection services,
    string? connectionName)
{
    public IServiceCollection Services { get; } = services;

    public string? ConnectionName { get; } = connectionName;

    public ServiceBusBuilder Configure(
        Action<CabazureServiceBusOptions> configure)
    {
        Services.Configure(ConnectionName, configure);
        return this;
    }

    public ServiceBusBuilder Configure<TConfigureOptions>()
        where TConfigureOptions : class, IConfigureOptions<CabazureServiceBusOptions>
    {
        Services.ConfigureOptions<TConfigureOptions>();
        return this;
    }

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
                publisherBuilder.GetPropertyFactory(),
                publisherBuilder.GetPartitionKeyFactory()));
        Services.AddSingleton(s => s
            .GetRequiredService<IServiceBusPublisherFactory>()
            .Create<T>());
        Services.AddSingleton<IMessagePublisher<T>>(s => s
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

        Services.AddSingleton<TProcessor>();
        Services.AddSingleton(s =>
        {
            var config = s
                .GetRequiredService<IOptionsMonitor<CabazureServiceBusOptions>>()
                .Get(ConnectionName);

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
        Services.AddSingleton<IMessageProcessorService<TProcessor>>(s
            => s.GetRequiredService<ServiceBusProcessorService<TMessage, TProcessor>>());
        Services.AddHostedService(s
            => s.GetRequiredService<ServiceBusProcessorService<TMessage, TProcessor>>());

        return this;
    }
}
