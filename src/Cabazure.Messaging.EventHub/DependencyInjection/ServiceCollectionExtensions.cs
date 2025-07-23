using Cabazure.Messaging.EventHub;
using Cabazure.Messaging.EventHub.DependencyInjection;

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace Microsoft.Extensions.DependencyInjection;
#pragma warning restore IDE0130 // Namespace does not match folder structure

/// <summary>
/// Provides extension methods for configuring Azure Event Hub services in the dependency injection container.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds Azure Event Hub services to the service collection with the default connection.
    /// </summary>
    /// <param name="services">The service collection to add services to.</param>
    /// <param name="builder">A delegate to configure the Event Hub services.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddCabazureEventHub(
        this IServiceCollection services,
        Action<EventHubBuilder> builder)
        => AddCabazureEventHub(services, null, builder);

    /// <summary>
    /// Adds Azure Event Hub services to the service collection with a named connection.
    /// </summary>
    /// <param name="services">The service collection to add services to.</param>
    /// <param name="connectionName">The name of the connection configuration to use.</param>
    /// <param name="builder">A delegate to configure the Event Hub services.</param>
    /// <returns>The service collection for chaining.</returns>
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
