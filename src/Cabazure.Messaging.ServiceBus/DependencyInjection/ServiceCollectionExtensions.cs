using Cabazure.Messaging.ServiceBus;
using Cabazure.Messaging.ServiceBus.DependencyInjection;
using Cabazure.Messaging.ServiceBus.Publishing;

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

        services.AddSingleton<IServiceBusSenderProvider, ServiceBusSenderProvider>();
        services.AddSingleton<IServiceBusClientProvider, ServiceBusClientProvider>();

        return services;
    }
}
