using Cabazure.Messaging.StorageQueue;
using Cabazure.Messaging.StorageQueue.DependencyInjection;

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace Microsoft.Extensions.DependencyInjection;
#pragma warning restore IDE0130 // Namespace does not match folder structure

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCabazureStorageQueue(
        this IServiceCollection services,
        Action<StorageQueueBuilder> builder)
        => AddCabazureStorageQueue(services, null, builder);

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
