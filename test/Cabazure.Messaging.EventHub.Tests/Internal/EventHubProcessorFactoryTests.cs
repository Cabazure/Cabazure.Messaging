using System.Text.Json;
using Azure.Core;
using Azure.Messaging.EventHubs;
using Azure.Storage.Blobs;
using Cabazure.Messaging.EventHub.DependencyInjection;
using Cabazure.Messaging.EventHub.Internal;
using Microsoft.Extensions.Options;

namespace Cabazure.Messaging.EventHub.Tests.Internal;

public class EventHubProcessorFactoryTests
{
    [Theory, AutoNSubstituteData]
    public void Create_Throws_If_No_Options(
        [Frozen] IOptionsMonitor<CabazureEventHubOptions> monitor,
        EventHubProcessorFactory sut,
        string connectionName,
        string eventHubName,
        string consumerGroup,
        BlobContainerOptions containerOptions,
        [NoAutoProperties] EventProcessorClientOptions clientOptions)
        => FluentActions
            .Invoking(() =>
                sut.Create(
                    connectionName,
                    eventHubName,
                    consumerGroup,
                    containerOptions,
                    clientOptions))
            .Should()
            .Throw<ArgumentException>()
            .WithMessage(
                $"Missing configuration for Event Hub connection `{connectionName}`");

    [Theory, AutoNSubstituteData]
    public void Create_Gets_Options(
        [Frozen, NoAutoProperties]
        JsonSerializerOptions serializerOptions,
        CabazureEventHubOptions options,
        [Frozen] IOptionsMonitor<CabazureEventHubOptions> monitor,
        [Frozen] IBlobStorageClientProvider storageProvider,
        [Substitute] BlobServiceClient blobClient,
        EventHubProcessorFactory sut,
        string connectionName,
        string eventHubName,
        string consumerGroup,
        BlobContainerOptions containerOptions,
        [NoAutoProperties] EventProcessorClientOptions clientOptions)
    {
        storageProvider.GetClient(default).ReturnsForAnyArgs(blobClient);
        monitor.Get(default).ReturnsForAnyArgs(options);
        sut.Create(
            connectionName,
            eventHubName,
            consumerGroup,
            containerOptions,
            clientOptions);

        monitor
            .Received(1)
            .Get(connectionName);
    }

    [Theory, AutoNSubstituteData]
    public void Create_Gets_StorageClient(
        [Frozen, NoAutoProperties]
        JsonSerializerOptions serializerOptions,
        CabazureEventHubOptions options,
        [Frozen] IOptionsMonitor<CabazureEventHubOptions> monitor,
        [Frozen] IBlobStorageClientProvider storageProvider,
        [Substitute] BlobServiceClient blobClient,
        EventHubProcessorFactory sut,
        string connectionName,
        string eventHubName,
        string consumerGroup,
        BlobContainerOptions containerOptions,
        [NoAutoProperties] EventProcessorClientOptions clientOptions)
    {
        storageProvider.GetClient(default).ReturnsForAnyArgs(blobClient);
        monitor.Get(default).ReturnsForAnyArgs(options);
        sut.Create(
            connectionName,
            eventHubName,
            consumerGroup,
            containerOptions,
            clientOptions);

        storageProvider
            .Received(1)
            .GetClient(connectionName);
    }

    [Theory, AutoNSubstituteData]
    public void Create_Returns_Client(
        [Frozen, NoAutoProperties]
        JsonSerializerOptions serializerOptions,
        CabazureEventHubOptions options,
        [Frozen] IOptionsMonitor<CabazureEventHubOptions> monitor,
        [Frozen] IBlobStorageClientProvider storageProvider,
        [Substitute] BlobServiceClient blobClient,
        EventHubProcessorFactory sut,
        string connectionName,
        string eventHubName,
        string consumerGroup,
        BlobContainerOptions containerOptions,
        [NoAutoProperties] EventProcessorClientOptions clientOptions)
    {
        storageProvider.GetClient(default).ReturnsForAnyArgs(blobClient);
        monitor.Get(default).ReturnsForAnyArgs(options);
        var result = sut.Create(
            connectionName,
            eventHubName,
            consumerGroup,
            containerOptions,
            clientOptions);

        result
            .Should()
            .BeOfType<EventHubProcessorWrapper>();
    }

    [Theory, AutoNSubstituteData]
    public void Create_Uses_Namespace_From_Options(
        [Frozen, NoAutoProperties]
        JsonSerializerOptions serializerOptions,
        [Frozen] IOptionsMonitor<CabazureEventHubOptions> monitor,
        [Frozen] IBlobStorageClientProvider storageProvider,
        [Substitute] BlobServiceClient blobClient,
        EventHubProcessorFactory sut,
        string connectionName,
        string eventHubName,
        string consumerGroup,
        BlobContainerOptions containerOptions,
        [NoAutoProperties] EventProcessorClientOptions clientOptions,
        string fqns,
        TokenCredential credential)
    {
        var options = new CabazureEventHubOptions
        {
            FullyQualifiedNamespace = fqns,
            Credential = credential,
        };
        storageProvider.GetClient(default).ReturnsForAnyArgs(blobClient);
        monitor.Get(default).ReturnsForAnyArgs(options);
        var result = sut.Create(
            connectionName,
            eventHubName,
            consumerGroup,
            containerOptions,
            clientOptions);

        result.FullyQualifiedNamespace
            .Should()
            .Be(fqns);
        result.EventHubName
            .Should()
            .Be(eventHubName);
        result.ConsumerGroup
            .Should()
            .Be(consumerGroup);
    }

    [Theory, AutoNSubstituteData]
    public void Create_Uses_Namespace_From_ConnectionString_In_Options(
        [Frozen, NoAutoProperties]
        JsonSerializerOptions serializerOptions,
        [Frozen] IOptionsMonitor<CabazureEventHubOptions> monitor,
        [Frozen] IBlobStorageClientProvider storageProvider,
        [Substitute] BlobServiceClient blobClient,
        EventHubProcessorFactory sut,
        string connectionName,
        string eventHubName,
        string consumerGroup,
        BlobContainerOptions containerOptions,
        [NoAutoProperties] EventProcessorClientOptions clientOptions,
        string fqns,
        TokenCredential credential)
    {
        var options = new CabazureEventHubOptions
        {
            ConnectionString =
                $"Endpoint=sb://{fqns};" +
                $"SharedAccessKeyName=RootManageSharedAccessKey;" +
                $"SharedAccessKey=SAS_KEY_VALUE;",
        };
        storageProvider.GetClient(default).ReturnsForAnyArgs(blobClient);
        monitor.Get(default).ReturnsForAnyArgs(options);
        var result = sut.Create(
            connectionName,
            eventHubName,
            consumerGroup,
            containerOptions,
            clientOptions);

        result.FullyQualifiedNamespace
            .Should()
            .Be(fqns);
        result.EventHubName
            .Should()
            .Be(eventHubName);
        result.ConsumerGroup
            .Should()
            .Be(consumerGroup);
    }
}
