using System.Text.Json;
using Azure.Core;
using Cabazure.Messaging.StorageQueue.Internal;
using Microsoft.Extensions.Options;

namespace Cabazure.Messaging.StorageQueue.Tests.Internal;

public class StorageQueueClientProviderTests
{
    [Theory, AutoNSubstituteData]
    public void Create_Throws_If_No_Options(
        [Frozen] IOptionsMonitor<CabazureStorageQueueOptions> monitor,
        StorageQueueClientProvider sut,
        string connectionName,
        string eventHubName)
    {
        FluentActions
            .Invoking(() =>
                sut.GetClient(connectionName))
            .Should()
            .Throw<ArgumentException>()
            .WithMessage(
                $"Missing configuration for Event Hub connection `{connectionName}`");
    }

    [Theory, AutoNSubstituteData]
    public void Create_Gets_Options(
        [Frozen, NoAutoProperties]
        JsonSerializerOptions serializerOptions,
        CabazureStorageQueueOptions options,
        [Frozen] IOptionsMonitor<CabazureStorageQueueOptions> monitor,
        StorageQueueClientProvider sut,
        string connectionName,
        string eventHubName)
    {
        monitor.Get(default).ReturnsForAnyArgs(options);

        sut.GetClient(
            connectionName,
            eventHubName);

        monitor.Received(1).Get(connectionName);
    }

    [Theory, AutoNSubstituteData]
    public void Creates_Client_Returns_Client(
        [Frozen, NoAutoProperties]
        JsonSerializerOptions serializerOptions,
        CabazureStorageQueueOptions options,
        [Frozen] IOptionsMonitor<CabazureStorageQueueOptions> monitor,
        StorageQueueClientProvider sut,
        string fullyQualifiedNamespace,
        TokenCredential credential,
        string connectionName,
        string eventHubName)
    {
        monitor.Get(default).ReturnsForAnyArgs(options);

        var client = sut.GetClient(
            connectionName,
            eventHubName);

        client
            .Should()
            .BeOfType<StorageQueueClientClient>();
    }

    [Theory, AutoNSubstituteData]
    public void Creates_Client_From_Namespace_And_Credential(
       [Frozen, NoAutoProperties]
       JsonSerializerOptions serializerOptions,
       [Frozen] IOptionsMonitor<CabazureStorageQueueOptions> monitor,
       StorageQueueClientProvider sut,
       string fqns,
       TokenCredential credential,
       string connectionName,
       string eventHubName)
    {
        var options = new CabazureStorageQueueOptions
        {
            FullyQualifiedNamespace = fqns,
            Credential = credential,
        };
        monitor.Get(default).ReturnsForAnyArgs(options);

        var client = sut.GetClient(
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
       [Frozen] IOptionsMonitor<CabazureStorageQueueOptions> monitor,
       StorageQueueClientProvider sut,
       string connectionName,
       string eventHubName,
       string fqns)
    {
        var options = new CabazureStorageQueueOptions
        {
            ConnectionString =
                $"Endpoint=sb://{fqns};" +
                $"SharedAccessKeyName=RootManageSharedAccessKey;" +
                $"SharedAccessKey=SAS_KEY_VALUE;",
        };
        monitor.Get(default).ReturnsForAnyArgs(options);

        var client = sut.GetClient(
            connectionName,
            eventHubName);

        client.FullyQualifiedNamespace
            .Should()
            .Be(fqns);
    }

    [Theory, AutoNSubstituteData]
    public void GetClient_Returns_Same_Client_For_Same_Connection(
        [Frozen, NoAutoProperties]
        JsonSerializerOptions serializerOptions,
        CabazureStorageQueueOptions options,
        [Frozen] IOptionsMonitor<CabazureStorageQueueOptions> monitor,
        StorageQueueClientProvider sut,
        string connectionName,
        string eventHubName)
    {
        monitor.Get(default).ReturnsForAnyArgs(options);
        var result1 = sut.GetClient(connectionName, eventHubName);
        var result2 = sut.GetClient(connectionName, eventHubName);

        result1
            .Should()
            .BeSameAs(result2);
    }

    [Theory, AutoNSubstituteData]
    public void GetClient_Returns_Different_Client_For_Different_Connection(
        [Frozen, NoAutoProperties]
        JsonSerializerOptions serializerOptions,
        CabazureStorageQueueOptions options,
        [Frozen] IOptionsMonitor<CabazureStorageQueueOptions> monitor,
        StorageQueueClientProvider sut,
        string connectionName1,
        string connectionName2,
        string eventHubName)
    {
        monitor.Get(default).ReturnsForAnyArgs(options);
        var result1 = sut.GetClient(connectionName1, eventHubName);
        var result2 = sut.GetClient(connectionName2, eventHubName);

        result1
            .Should()
            .NotBeSameAs(result2);
    }

    [Theory, AutoNSubstituteData]
    public void GetClient_Returns_Different_Client_For_Different_StorageQueue(
        [Frozen, NoAutoProperties]
        JsonSerializerOptions serializerOptions,
        CabazureStorageQueueOptions options,
        [Frozen] IOptionsMonitor<CabazureStorageQueueOptions> monitor,
        StorageQueueClientProvider sut,
        string connectionName,
        string eventHubName1,
        string eventHubName2)
    {
        monitor.Get(default).ReturnsForAnyArgs(options);
        var result1 = sut.GetClient(connectionName, eventHubName1);
        var result2 = sut.GetClient(connectionName, eventHubName2);

        result1
            .Should()
            .NotBeSameAs(result2);
    }

    [Theory, AutoNSubstituteData]
    public async Task DisposeAsync_Disposes_Clients(
        [Frozen, NoAutoProperties]
        JsonSerializerOptions serializerOptions,
        CabazureStorageQueueOptions options,
        [Frozen] IOptionsMonitor<CabazureStorageQueueOptions> monitor,
        StorageQueueClientProvider sut,
        string connectionName,
        string eventHubName)
    {
        monitor.Get(default).ReturnsForAnyArgs(options);
        var client = sut.GetClient(connectionName, eventHubName);

        client.IsClosed.Should().BeFalse();

        await sut.DisposeAsync();

        client.IsClosed.Should().BeTrue();
    }
}
