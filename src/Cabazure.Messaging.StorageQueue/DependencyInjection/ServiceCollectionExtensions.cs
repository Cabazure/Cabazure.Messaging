using Cabazure.Messaging.StorageQueue;
using Cabazure.Messaging.StorageQueue.DependencyInjection;

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace Microsoft.Extensions.DependencyInjection;
#pragma warning restore IDE0130 // Namespace does not match folder structure

/// <summary>
/// Provides extension methods for configuring Azure Storage Queue services in the dependency injection container.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds Azure Storage Queue services to the service collection with the default connection.
    /// </summary>
    /// <param name="services">The service collection to add services to.</param>
    /// <param name="builder">A delegate to configure the Storage Queue services.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddCabazureStorageQueue(
        this IServiceCollection services,
        Action<StorageQueueBuilder> builder)
        => AddCabazureStorageQueue(services, null, builder);

    /// <summary>
    /// Adds Azure Storage Queue services to the service collection with a named connection.
    /// </summary>
    /// <param name="services">The service collection to add services to.</param>
    /// <param name="connectionName">The name of the connection configuration to use.</param>
    /// <param name="builder">A delegate to configure the Storage Queue services.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddCabazureStorageQueue(
        this IServiceCollection services,
        string? connectionName,
        Action<StorageQueueBuilder> builder)
    {
        services.AddOptions<CabazureStorageQueueOptions>(connectionName);

        var serviceBusBuilder = new StorageQueueBuilder(services, connectionName);
        builder.Invoke(serviceBusBuilder);

        return services;
    }
}
