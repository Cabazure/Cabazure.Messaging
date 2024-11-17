using System.Text.Json;
using Azure.Core;
using Azure.Messaging.EventHubs.Producer;
using Cabazure.Messaging.EventHub.Internal;
using Microsoft.Extensions.Options;

namespace Cabazure.Messaging.EventHub.Tests.Internal;

public class EventHubProducerClientFactoryTests
{
    [Theory, AutoNSubstituteData]
    public void Create_Throws_If_No_Options(
        [Frozen] IOptionsMonitor<CabazureEventHubOptions> monitor,
        EventHubProducerClientFactory sut,
        string connectionName,
        string eventHubName)
    {
        FluentActions
            .Invoking(() =>
                sut.Create(connectionName, eventHubName))
            .Should()
            .Throw<ArgumentException>()
            .WithMessage(
                $"Missing configuration for Event Hub connection `{connectionName}`");
    }

    [Theory, AutoNSubstituteData]
    public void Create_Gets_Options(
        [Frozen, NoAutoProperties]
        JsonSerializerOptions serializerOptions,
        CabazureEventHubOptions options,
        [Frozen] IOptionsMonitor<CabazureEventHubOptions> monitor,
        EventHubProducerClientFactory sut,
        string connectionName,
        string eventHubName)
    {
        monitor.Get(default).ReturnsForAnyArgs(options);

        sut.Create(
            connectionName,
            eventHubName);

        monitor.Received(1).Get(connectionName);
    }

    [Theory, AutoNSubstituteData]
    public void Creates_Client_Returns_Client(
        [Frozen, NoAutoProperties]
        JsonSerializerOptions serializerOptions,
        CabazureEventHubOptions options,
        [Frozen] IOptionsMonitor<CabazureEventHubOptions> monitor,
        EventHubProducerClientFactory sut,
        string fullyQualifiedNamespace,
        TokenCredential credential,
        string connectionName,
        string eventHubName)
    {
        monitor.Get(default).ReturnsForAnyArgs(options);

        var client = sut.Create(
            connectionName,
            eventHubName);

        client
            .Should()
            .BeOfType<EventHubProducerClient>();
    }

    [Theory, AutoNSubstituteData]
    public void Creates_Client_From_Namespace_And_Credential(
       [Frozen, NoAutoProperties]
       JsonSerializerOptions serializerOptions,
       [Frozen] IOptionsMonitor<CabazureEventHubOptions> monitor,
       EventHubProducerClientFactory sut,
       string fqns,
       TokenCredential credential,
       string connectionName,
       string eventHubName)
    {
        var options = new CabazureEventHubOptions
        {
            FullyQualifiedNamespace = fqns,
            Credential = credential,
        };
        monitor.Get(default).ReturnsForAnyArgs(options);

        var client = sut.Create(
            connectionName,
            eventHubName);

        client.FullyQualifiedNamespace
            .Should()
            .Be(fqns);
    }

    [Theory, AutoNSubstituteData]
    public void Creates_Uses_Namespace_From_ConnectionString_In_Options(
       [Frozen, NoAutoProperties]
       JsonSerializerOptions serializerOptions,
       [Frozen] IOptionsMonitor<CabazureEventHubOptions> monitor,
       EventHubProducerClientFactory sut,
       string connectionName,
       string fqns,
       string eventHubName)
    {
        var options = new CabazureEventHubOptions
        {
            ConnectionString =
                $"Endpoint=sb://{fqns};" +
                $"SharedAccessKeyName=RootManageSharedAccessKey;" +
                $"SharedAccessKey=SAS_KEY_VALUE;",
        };
        monitor.Get(default).ReturnsForAnyArgs(options);

        var client = sut.Create(
            connectionName,
            eventHubName);

        client.FullyQualifiedNamespace
            .Should()
            .Be(fqns);
    }
}
