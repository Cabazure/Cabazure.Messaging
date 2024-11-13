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
}
