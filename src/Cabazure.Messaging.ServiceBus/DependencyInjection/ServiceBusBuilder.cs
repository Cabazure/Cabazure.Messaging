using Azure.Messaging.ServiceBus;
using Cabazure.Messaging.ServiceBus.Internal;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
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
                publisherBuilder.SenderOptions,
                publisherBuilder.GetEventDataModifier()));
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
            (f, o) => f.Create(ConnectionName, topicName, subscriptionName, o),
            builder);

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
