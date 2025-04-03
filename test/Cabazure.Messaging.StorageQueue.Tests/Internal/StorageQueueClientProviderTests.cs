using System.Text.Json;
using Azure.Core;
using Azure.Storage.Queues;
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
                $"Missing configuration for Storage Queue connection `{connectionName}`");
    }

    [Theory, AutoNSubstituteData]
    public void Create_Gets_Options(
        [Frozen, NoAutoProperties]
        JsonSerializerOptions serializerOptions,
        CabazureStorageQueueOptions options,
        [Frozen] IOptionsMonitor<CabazureStorageQueueOptions> monitor,
        StorageQueueClientProvider sut,
        string connectionName)
    {
        options.QueueServiceUri = new UriBuilder(options.QueueServiceUri.Host) { Scheme = Uri.UriSchemeHttps, }.Uri;
        monitor.Get(default).ReturnsForAnyArgs(options);

        sut.GetClient(connectionName);

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
        string connectionName)
    {
        options.QueueServiceUri = new UriBuilder(options.QueueServiceUri.Host) { Scheme = Uri.UriSchemeHttps, }.Uri;
        monitor.Get(default).ReturnsForAnyArgs(options);

        var client = sut.GetClient(
            connectionName);

        client
            .Should()
            .BeOfType<QueueServiceClient>();
    }

    [Theory, AutoNSubstituteData]
    public void Creates_Client_From_Namespace_And_Credential(
       [Frozen, NoAutoProperties]
       JsonSerializerOptions serializerOptions,
       [Frozen] IOptionsMonitor<CabazureStorageQueueOptions> monitor,
       StorageQueueClientProvider sut,
       Uri queueServiceUri,
       TokenCredential credential,
       string connectionName)
    {
        queueServiceUri = new UriBuilder(queueServiceUri.Host) 
            { Scheme = Uri.UriSchemeHttps, }.Uri;
        var options = new CabazureStorageQueueOptions
        {
            QueueServiceUri = queueServiceUri,
            Credential = credential,
        };
        monitor.Get(default).ReturnsForAnyArgs(options);

        var client = sut.GetClient(connectionName);

        client.Uri
            .Should()
            .Be(queueServiceUri);
    }

    [Theory, AutoNSubstituteData]
    public void Creates_Uses_Namespace_From_ConnectionString_In_Options(
       [Frozen, NoAutoProperties]
       JsonSerializerOptions serializerOptions,
       [Frozen] IOptionsMonitor<CabazureStorageQueueOptions> monitor,
       StorageQueueClientProvider sut,
       string connectionName,
       string accountName)
    {
        var options = new CabazureStorageQueueOptions
        {
            ConnectionString =
                $"DefaultEndpointsProtocol=https;" +
                $"AccountName={accountName};" +
                $"AccountKey=Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==;" +
                $"EndpointSuffix=core.windows.net",
        };
        monitor.Get(default).ReturnsForAnyArgs(options);

        var client = sut.GetClient(
            connectionName);

        client.AccountName
            .Should()
            .Be(accountName);
    }
}
