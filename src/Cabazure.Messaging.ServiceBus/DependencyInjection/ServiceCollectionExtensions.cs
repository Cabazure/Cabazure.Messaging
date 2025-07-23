using Cabazure.Messaging.ServiceBus;
using Cabazure.Messaging.ServiceBus.DependencyInjection;

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace Microsoft.Extensions.DependencyInjection;
#pragma warning restore IDE0130 // Namespace does not match folder structure

/// <summary>
/// Provides extension methods for configuring Azure Service Bus services in the dependency injection container.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds Azure Service Bus services to the service collection with the default connection.
    /// </summary>
    /// <param name="services">The service collection to add services to.</param>
    /// <param name="builder">A delegate to configure the Service Bus services.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddCabazureServiceBus(
        this IServiceCollection services,
        Action<ServiceBusBuilder> builder)
        => AddCabazureServiceBus(services, null, builder);

    /// <summary>
    /// Adds Azure Service Bus services to the service collection with a named connection.
    /// </summary>
    /// <param name="services">The service collection to add services to.</param>
    /// <param name="connectionName">The name of the connection configuration to use.</param>
    /// <param name="builder">A delegate to configure the Service Bus services.</param>
    /// <returns>The service collection for chaining.</returns>
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
