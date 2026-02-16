using System.Reflection;
using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Producer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Cabazure.Messaging.EventHub.Tests.Scenarios;

public class SingleConnectionsScenarioTests
{
    private readonly Service1 sut;

    public SingleConnectionsScenarioTests()
    {
        var services = new ServiceCollection();

        services.AddCabazureEventHub(
            b => b
                .Configure<ConfigureEventHubOptions>()
                .AddPublisher<Message1>("eventhub1"));

        services.AddSingleton<Service1>();

        var provider = services.BuildServiceProvider();

        sut = provider.GetRequiredService<Service1>();
    }

    [Fact]
    public void Have_Unique_ConnectionStrings_Per_Connection()
    {
        sut.Should().NotBeNull();

        AssertPublisherConnectionCustomEndpoint(sut.Publisher1);
    }

    private static void AssertPublisherConnectionCustomEndpoint(object messagePublisher)
    {
        var producer = messagePublisher
            .GetType()
            .GetField(
                "<producer>P",
                BindingFlags.NonPublic | BindingFlags.Instance)
            ?.GetValue(messagePublisher) as EventHubProducerClient;

        var connection = producer?
            .GetType()
            .GetProperty(
                "Connection",
                BindingFlags.NonPublic | BindingFlags.Instance)
            ?.GetValue(producer) as EventHubConnection;

        var options = connection?
            .GetType()
            .GetProperty(
                "Options",
                BindingFlags.NonPublic | BindingFlags.Instance)
            ?.GetValue(connection) as EventHubConnectionOptions;

        options.CustomEndpointAddress.Should().Be($"sb://localhost");
    }

    sealed record Message1();

    sealed class Service1(IMessagePublisher<Message1> publisher1)
    {
        public IMessagePublisher<Message1> Publisher1 { get; } = publisher1;
    }

    sealed class ConfigureEventHubOptions : IConfigureNamedOptions<CabazureEventHubOptions>
    {
        public void Configure(CabazureEventHubOptions options)
            => Configure(string.Empty, options);

        public void Configure(string? name, CabazureEventHubOptions options)
        {
            name.Should().BeNullOrEmpty();
            options.WithConnection(
                $"Endpoint=sb://localhost;"
              + $"SharedAccessKeyName=RootManageSharedAccessKey;"
              + $"SharedAccessKey=SAS_KEY_VALUE;"
              + $"UseDevelopmentEmulator=true");
        }
    }
}
