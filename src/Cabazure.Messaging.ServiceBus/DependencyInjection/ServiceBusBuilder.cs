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
        where TConfigureOptions : class, IConfigureNamedOptions<CabazureServiceBusOptions>
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

        services.AddSingleton(new ServiceBusPublisherRegistration(
            connectionName,
            typeof(T),
            topicOrQueueName,
            propertiesFactory,
            partitionKeyFactory));

        services.TryAddSingleton<IServiceBusPublisherFactory, ServiceBusPublisherFactory>();

        services.AddSingleton(s => s
            .GetRequiredService<IServiceBusPublisherFactory>()
            .Create<T>());
        services.AddSingleton<IMessagePublisher<T>>(s => s
            .GetRequiredService<IServiceBusPublisher<T>>());

        return this;
    }
}