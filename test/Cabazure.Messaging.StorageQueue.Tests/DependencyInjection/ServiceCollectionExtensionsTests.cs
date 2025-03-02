using Cabazure.Messaging.StorageQueue.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Cabazure.Messaging.StorageQueue.Tests.DependencyInjection;

public class ServiceCollectionExtensionsTests
{
    [Theory, AutoNSubstituteData]
    public void Should_Add_CabazureStorageQueueOptions_IOptionsMonitor(
        ServiceCollection services,
        [Substitute] Action<StorageQueueBuilder> builder)
    {
        ServiceCollectionExtensions.AddCabazureStorageQueue(
            services,
            builder);

        services
            .BuildServiceProvider()
            .GetRequiredService<IOptionsMonitor<CabazureStorageQueueOptions>>()
            .Should()
            .NotBeNull();
    }

    [Theory, AutoNSubstituteData]
    public void Should_Call_Builder_With_StorageQueueBuilder(
        ServiceCollection services,
        [Substitute] Action<StorageQueueBuilder> builder)
    {
        ServiceCollectionExtensions.AddCabazureStorageQueue(
            services,
            builder);

        builder
            .Received(1)
            .Invoke(Arg.Is<StorageQueueBuilder>(b
                => b.Services == services
                && b.ConnectionName == null));
    }

    [Theory, AutoNSubstituteData]
    public void Should_Call_Builder_With_StorageQueueBuilder_For_Specific_Connection(
        ServiceCollection services,
        string connectionName,
        [Substitute] Action<StorageQueueBuilder> builder)
    {
        ServiceCollectionExtensions.AddCabazureStorageQueue(
            services,
            connectionName,
            builder);

        builder
            .Received(1)
            .Invoke(Arg.Is<StorageQueueBuilder>(b
                => b.Services == services
                && b.ConnectionName == connectionName));
    }
}
