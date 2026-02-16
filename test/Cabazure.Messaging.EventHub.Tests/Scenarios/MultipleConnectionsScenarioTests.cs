using System.Reflection;
using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Producer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Cabazure.Messaging.EventHub.Tests.Scenarios;

public class MultipleConnectionsScenarioTests
{
    private readonly Service1 sut;

    public MultipleConnectionsScenarioTests()
    {
        var services = new ServiceCollection();

        services.AddCabazureEventHub(
            "1",
            b => b
                .Configure<ConfigureEventHubOptions>()
                .AddPublisher<Message1>("eventhub1"));
        services.AddCabazureEventHub(
            "2",
            b => b
                .Configure<ConfigureEventHubOptions>()
                .AddPublisher<Message2>("eventhub2"));
        services.AddCabazureEventHub(
            "3",
            b => b
                .Configure<ConfigureEventHubOptions>()
                .AddPublisher<Message3>("eventhub3"));

        services.AddSingleton<Service1>();

        var provider = services.BuildServiceProvider();

        sut = provider.GetRequiredService<Service1>();
    }

    [Fact]
    public void Have_Unique_ConnectionStrings_Per_Connection()
    {
        sut.Should().NotBeNull();

        AssertPublisherConnectionCustomEndpoint(sut.Publisher1, 1);
        AssertPublisherConnectionCustomEndpoint(sut.Publisher2, 2);
        AssertPublisherConnectionCustomEndpoint(sut.Publisher3, 3);
    }

    private static void AssertPublisherConnectionCustomEndpoint(object messagePublisher, int port)
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

        options.CustomEndpointAddress.Should().Be($"sb://localhost:{port}");
    }

    sealed record Message1();

    sealed record Message2();

    sealed record Message3();

    sealed class Service1(
        IMessagePublisher<Message1> publisher1,
        IMessagePublisher<Message2> publisher2,
        IMessagePublisher<Message3> publisher3)
    {
        public IMessagePublisher<Message3> Publisher3 { get; } = publisher3;

        public IMessagePublisher<Message2> Publisher2 { get; } = publisher2;

        public IMessagePublisher<Message1> Publisher1 { get; } = publisher1;
    }


    sealed class ConfigureEventHubOptions : IConfigureNamedOptions<CabazureEventHubOptions>
    {
        public void Configure(CabazureEventHubOptions options)
            => Configure(string.Empty, options);

        public void Configure(string? name, CabazureEventHubOptions options)
        {
            options.WithConnection(
                $"Endpoint=sb://localhost:{name};"
              + $"SharedAccessKeyName=RootManageSharedAccessKey;"
              + $"SharedAccessKey=SAS_KEY_VALUE;"
              + $"UseDevelopmentEmulator=true");
        }
    }
}
