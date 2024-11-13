using Cabazure.Messaging.EventHub;
using Cabazure.Messaging.EventHub.DependencyInjection;

namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCabazureEventHub(
        this IServiceCollection services,
        Action<EventHubBuilder> builder)
        => AddCabazureEventHub(services, null, builder);

    public static IServiceCollection AddCabazureEventHub(
        this IServiceCollection services,
        string? connectionName,
        Action<EventHubBuilder> builder)
    {
        services.AddOptions<CabazureEventHubOptions>(connectionName);

        var eventHubBuilder = new EventHubBuilder(services, connectionName);
        builder.Invoke(eventHubBuilder);

        return services;
    }
}
