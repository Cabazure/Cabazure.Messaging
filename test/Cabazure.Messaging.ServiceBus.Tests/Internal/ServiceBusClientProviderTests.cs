using System.Text.Json;
using Azure.Core;
using Azure.Messaging.ServiceBus;
using Cabazure.Messaging.ServiceBus.Internal;
using Microsoft.Extensions.Options;

namespace Cabazure.Messaging.ServiceBus.Tests.Internal;

public class ServiceBusClientProviderTests
{
    [Theory, AutoNSubstituteData]
    public void Create_Throws_If_No_Options(
        [Frozen] IOptionsMonitor<CabazureServiceBusOptions> monitor,
        ServiceBusClientProvider sut,
        string connectionName)
        => FluentActions
            .Invoking(() =>
                sut.GetClient(connectionName))
            .Should()
            .Throw<ArgumentException>()
            .WithMessage(
                $"Missing configuration for Service Bus connection `{connectionName}`");

    [Theory, AutoNSubstituteData]
    public void GetClient_Gets_Options(
        [Frozen, NoAutoProperties]
        JsonSerializerOptions serializerOptions,
        CabazureServiceBusOptions options,
        [Frozen] IOptionsMonitor<CabazureServiceBusOptions> monitor,
        ServiceBusClientProvider sut,
        string connectionName)
    {
        monitor.Get(default).ReturnsForAnyArgs(options);
        sut.GetClient(connectionName);

        monitor
            .Received(1)
            .Get(connectionName);
    }

    [Theory, AutoNSubstituteData]
    public void GetClient_Returns_Client(
        [Frozen, NoAutoProperties]
        JsonSerializerOptions serializerOptions,
        CabazureServiceBusOptions options,
        [Frozen] IOptionsMonitor<CabazureServiceBusOptions> monitor,
        ServiceBusClientProvider sut,
        string connectionName)
    {
        monitor.Get(default).ReturnsForAnyArgs(options);
        var result = sut.GetClient(connectionName);

        result
            .Should()
            .BeOfType<ServiceBusClient>();
    }

    [Theory, AutoNSubstituteData]
    public void GetClient_Uses_Namespace_From_Options(
        [Frozen, NoAutoProperties]
        JsonSerializerOptions serializerOptions,
        [Frozen] IOptionsMonitor<CabazureServiceBusOptions> monitor,
        ServiceBusClientProvider sut,
        string connectionName,
        string fqns,
        TokenCredential credential)
    {
        var options = new CabazureServiceBusOptions
        {
            FullyQualifiedNamespace = fqns,
            Credential = credential,
        };
        monitor.Get(default).ReturnsForAnyArgs(options);
        var result = sut.GetClient(connectionName);

        result.FullyQualifiedNamespace
            .Should()
            .Be(fqns);
    }

    [Theory, AutoNSubstituteData]
    public void GetClient_Uses_Namespace_From_ConnectionString_In_Options(
        [Frozen, NoAutoProperties]
        JsonSerializerOptions serializerOptions,
        [Frozen] IOptionsMonitor<CabazureServiceBusOptions> monitor,
        ServiceBusClientProvider sut,
        string connectionName,
        string fqns,
        TokenCredential credential)
    {
        var options = new CabazureServiceBusOptions
        {
            ConnectionString =
                $"Endpoint=sb://{fqns};" +
                $"SharedAccessKeyName=RootManageSharedAccessKey;" +
                $"SharedAccessKey=SAS_KEY_VALUE;",
        };
        monitor.Get(default).ReturnsForAnyArgs(options);
        var result = sut.GetClient(connectionName);

        result.FullyQualifiedNamespace
            .Should()
            .Be(fqns);
    }

    [Theory, AutoNSubstituteData]
    public void GetClient_Returns_Same_Client_For_Same_Connection(
        [Frozen, NoAutoProperties]
        JsonSerializerOptions serializerOptions,
        CabazureServiceBusOptions options,
        [Frozen] IOptionsMonitor<CabazureServiceBusOptions> monitor,
        ServiceBusClientProvider sut,
        string connectionName)
    {
        monitor.Get(default).ReturnsForAnyArgs(options);
        var result1 = sut.GetClient(connectionName);
        var result2 = sut.GetClient(connectionName);

        result1
            .Should()
            .BeSameAs(result2);
    }

    [Theory, AutoNSubstituteData]
    public void GetClient_Returns_Different_Client_For_Different_Connection(
        [Frozen, NoAutoProperties]
        JsonSerializerOptions serializerOptions,
        CabazureServiceBusOptions options,
        [Frozen] IOptionsMonitor<CabazureServiceBusOptions> monitor,
        ServiceBusClientProvider sut,
        string connectionName1,
        string connectionName2)
    {
        monitor.Get(default).ReturnsForAnyArgs(options);
        var result1 = sut.GetClient(connectionName1);
        var result2 = sut.GetClient(connectionName2);

        result1
            .Should()
            .NotBeSameAs(result2);
    }

    [Theory, AutoNSubstituteData]
    public async Task DisposeAsync_Disposes_Clients(
        [Frozen, NoAutoProperties]
        JsonSerializerOptions serializerOptions,
        CabazureServiceBusOptions options,
        [Frozen] IOptionsMonitor<CabazureServiceBusOptions> monitor,
        ServiceBusClientProvider sut,
        string connectionName)
    {
        monitor.Get(default).ReturnsForAnyArgs(options);
        var client = sut.GetClient(connectionName);

        client.IsClosed.Should().BeFalse();

        await sut.DisposeAsync();

        client.IsClosed.Should().BeTrue();
    }
}
