using System.Text.Json;
using Azure.Core;
using Azure.Messaging.EventHubs.Producer;
using Cabazure.Messaging.EventHub.Internal;
using Microsoft.Extensions.Options;

namespace Cabazure.Messaging.EventHub.Tests.Internal;

public class EventHubProducerClientFactoryTests
{
    private const string EventHubConnectionString = "Endpoint=sb://localhost;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=SAS_KEY_VALUE;UseDevelopmentEmulator=true;";

    [Theory, AutoNSubstituteData]
    public void Create_Throws_If_No_Options(
        [Frozen] IOptionsMonitor<CabazureEventHubOptions> monitor,
        EventHubProducerClientFactory sut,
        string connectionName,
        string topicName)
    {
        FluentActions
            .Invoking(() =>
                sut.Create(connectionName, topicName))
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
        string topicName)
    {
        monitor.Get(default).ReturnsForAnyArgs(options);

        sut.Create(
            connectionName,
            topicName);

        monitor.Received(1).Get(connectionName);
    }

    [Theory, AutoNSubstituteData]
    public void Creates_Client_From_ConnectionString(
       [Frozen, NoAutoProperties]
       JsonSerializerOptions serializerOptions,
       [Frozen] IOptionsMonitor<CabazureEventHubOptions> monitor,
       EventHubProducerClientFactory sut,
       string connectionName,
       string topicName)
    {
        var options = new CabazureEventHubOptions
        {
            ConnectionString = EventHubConnectionString,
        };
        monitor.Get(default).ReturnsForAnyArgs(options);

        var client = sut.Create(
            connectionName,
            topicName);

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
       string fullyQualifiedNamespace,
       TokenCredential credential,
       string connectionName,
       string topicName)
    {
        var options = new CabazureEventHubOptions
        {
            FullyQualifiedNamespace = fullyQualifiedNamespace,
            Credential = credential,
        };
        monitor.Get(default).ReturnsForAnyArgs(options);

        var client = sut.Create(
            connectionName,
            topicName);

        client
            .Should()
            .BeOfType<EventHubProducerClient>();
    }
}
