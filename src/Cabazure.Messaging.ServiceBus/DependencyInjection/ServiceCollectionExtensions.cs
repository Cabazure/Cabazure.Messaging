using Cabazure.Messaging.ServiceBus;
using Cabazure.Messaging.ServiceBus.DependencyInjection;

namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCabazureServiceBus(
        this IServiceCollection services,
        Action<ServiceBusBuilder> builder)
        => AddCabazureServiceBus(services, null, builder);

    public static IServiceCollection AddCabazureServiceBus(
        this IServiceCollection services,
        string? connectionName,
        Action<ServiceBusBuilder> builder)
    {
        services.AddOptions<CabazureServiceBusOptions>(connectionName);

        var serviceBusBuilder = new ServiceBusBuilder(services, connectionName);
        builder.Invoke(serviceBusBuilder);

        return services;
    }
}
