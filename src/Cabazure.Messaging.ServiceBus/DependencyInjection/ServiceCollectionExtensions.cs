using Cabazure.Messaging.ServiceBus;
using Cabazure.Messaging.ServiceBus.DependencyInjection;

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace Microsoft.Extensions.DependencyInjection;
#pragma warning restore IDE0130 // Namespace does not match folder structure

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
