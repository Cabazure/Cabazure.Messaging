using Cabazure.Messaging.ServiceBus.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Cabazure.Messaging.ServiceBus.Tests.DependencyInjection;

public class ServiceCollectionExtensionsTests
{
    [Theory, AutoNSubstituteData]
    public void Should_Add_CabazureServiceBusOptions_IOptionsMonitor(
        ServiceCollection services,
        [Substitute] Action<ServiceBusBuilder> builder)
    {
        ServiceCollectionExtensions.AddCabazureServiceBus(
            services,
            builder);

        services
            .BuildServiceProvider()
            .GetRequiredService<IOptionsMonitor<CabazureServiceBusOptions>>()
            .Should()
            .NotBeNull();
    }

    [Theory, AutoNSubstituteData]
    public void Should_Call_Builder_With_ServiceBusBuilder(
        ServiceCollection services,
        [Substitute] Action<ServiceBusBuilder> builder)
    {
        ServiceCollectionExtensions.AddCabazureServiceBus(
            services,
            builder);

        builder
            .Received(1)
            .Invoke(Arg.Is<ServiceBusBuilder>(b
                => b.Services == services
                && b.ConnectionName == null));
    }

    [Theory, AutoNSubstituteData]
    public void Should_Call_Builder_With_ServiceBusBuilder_For_Specific_Connection(
        ServiceCollection services,
        string connectionName,
        [Substitute] Action<ServiceBusBuilder> builder)
    {
        ServiceCollectionExtensions.AddCabazureServiceBus(
            services,
            connectionName,
            builder);

        builder
            .Received(1)
            .Invoke(Arg.Is<ServiceBusBuilder>(b
                => b.Services == services
                && b.ConnectionName == connectionName));
    }
}
