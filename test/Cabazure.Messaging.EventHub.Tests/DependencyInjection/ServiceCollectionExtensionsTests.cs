using Cabazure.Messaging.EventHub.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Cabazure.Messaging.EventHub.Tests.DependencyInjection;

public class ServiceCollectionExtensionsTests
{
    [Theory, AutoNSubstituteData]
    public void Should_Add_CabazureEventHubOptions_IOptionsMonitor(
        ServiceCollection services,
        [Substitute] Action<EventHubBuilder> builder)
    {
        ServiceCollectionExtensions.AddCabazureEventHub(
            services,
            builder);

        services
            .BuildServiceProvider()
            .GetRequiredService<IOptionsMonitor<CabazureEventHubOptions>>()
            .Should()
            .NotBeNull();
    }

    [Theory, AutoNSubstituteData]
    public void Should_Call_Builder_With_EventHubBuilder(
        ServiceCollection services,
        [Substitute] Action<EventHubBuilder> builder)
    {
        ServiceCollectionExtensions.AddCabazureEventHub(
            services,
            builder);

        builder
            .Received(1)
            .Invoke(Arg.Is<EventHubBuilder>(b
                => b.Services == services
                && b.ConnectionName == null));
    }

    [Theory, AutoNSubstituteData]
    public void Should_Call_Builder_With_EventHubBuilder_For_Specific_Connection(
        ServiceCollection services,
        string connectionName,
        [Substitute] Action<EventHubBuilder> builder)
    {
        ServiceCollectionExtensions.AddCabazureEventHub(
            services,
            connectionName,
            builder);

        builder
            .Received(1)
            .Invoke(Arg.Is<EventHubBuilder>(b
                => b.Services == services
                && b.ConnectionName == connectionName));
    }
}
