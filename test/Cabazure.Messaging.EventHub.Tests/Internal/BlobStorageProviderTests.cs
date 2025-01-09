using System.Text.Json;
using Azure.Core;
using Azure.Storage.Blobs;
using Cabazure.Messaging.EventHub.Internal;
using Microsoft.Extensions.Options;

namespace Cabazure.Messaging.EventHub.Tests.Internal;

public class BlobStorageProviderTests
{
    private readonly Uri serviceUri;
    private readonly string accountName;
    private readonly string connectionString;
    private readonly TokenCredential credential;
    private readonly CabazureEventHubOptions defaultOptions;

    public BlobStorageProviderTests()
    {
        serviceUri = new Uri("https://localhost");
        accountName = "account1";
        connectionString = $"AccountName={accountName};AccountKey=;";
        credential = Substitute.For<TokenCredential>();

        defaultOptions = new()
        {
            BlobStorage = new()
            {
                ConnectionString = connectionString,
                ServiceUri = serviceUri,
                Credential = credential,
            },
        };
    }

    [Theory, AutoNSubstituteData]
    public void Create_Throws_If_No_Options(
        [Frozen] IOptionsMonitor<CabazureEventHubOptions> monitor,
        BlobStorageClientProvider sut,
        string connectionName)
        => FluentActions
            .Invoking(() =>
                sut.GetClient(connectionName))
            .Should()
            .Throw<ArgumentException>()
            .WithMessage(
                $"Missing blob storage configuration for connection `{connectionName}`");

    [Theory, AutoNSubstituteData]
    public void GetClient_Gets_Options(
        [Frozen] IOptionsMonitor<CabazureEventHubOptions> monitor,
        BlobStorageClientProvider sut,
        string connectionName)
    {
        monitor.Get(default).ReturnsForAnyArgs(defaultOptions);
        sut.GetClient(connectionName);

        monitor
            .Received(1)
            .Get(connectionName);
    }

    [Theory, AutoNSubstituteData]
    public void GetClient_Returns_Client(
        [Frozen] IOptionsMonitor<CabazureEventHubOptions> monitor,
        BlobStorageClientProvider sut,
        string connectionName)
    {
        monitor.Get(default).ReturnsForAnyArgs(defaultOptions);
        var result = sut.GetClient(connectionName);

        result
            .Should()
            .BeOfType<BlobServiceClient>();
    }

    [Theory, AutoNSubstituteData]
    public void GetClient_Uses_Namespace_From_Options(
        [Frozen] IOptionsMonitor<CabazureEventHubOptions> monitor,
        BlobStorageClientProvider sut,
        string connectionName)
    {
        var options = new CabazureEventHubOptions
        {
            BlobStorage = new BlobStorageOptions
            {
                ServiceUri = serviceUri,
                Credential = credential,
            },
        };
        monitor.Get(default).ReturnsForAnyArgs(options);
        var result = sut.GetClient(connectionName);

        result.Uri
            .Should()
            .Be(serviceUri);
    }

    [Theory, AutoNSubstituteData]
    public void GetClient_Uses_Namespace_From_ConnectionString_In_Options(
        [Frozen, NoAutoProperties]
        JsonSerializerOptions serializerOptions,
        [Frozen] IOptionsMonitor<CabazureEventHubOptions> monitor,
        BlobStorageClientProvider sut,
        string connectionName)
    {
        var options = new CabazureEventHubOptions
        {
            BlobStorage = new BlobStorageOptions
            {
                ConnectionString = connectionString,
            }
        };
        monitor.Get(default).ReturnsForAnyArgs(options);
        var result = sut.GetClient(connectionName);

        result.AccountName
            .Should()
            .Be(accountName);
    }

    [Theory, AutoNSubstituteData]
    public void GetClient_Returns_Same_Client_For_Same_Connection(
        [Frozen] IOptionsMonitor<CabazureEventHubOptions> monitor,
        BlobStorageClientProvider sut,
        string connectionName)
    {
        monitor.Get(default).ReturnsForAnyArgs(defaultOptions);
        var result1 = sut.GetClient(connectionName);
        var result2 = sut.GetClient(connectionName);

        result1
            .Should()
            .BeSameAs(result2);
    }

    [Theory, AutoNSubstituteData]
    public void GetClient_Returns_Different_Client_For_Different_Connection(
        [Frozen] IOptionsMonitor<CabazureEventHubOptions> monitor,
        BlobStorageClientProvider sut,
        string connectionName1,
        string connectionName2)
    {
        monitor.Get(default).ReturnsForAnyArgs(defaultOptions);
        var result1 = sut.GetClient(connectionName1);
        var result2 = sut.GetClient(connectionName2);

        result1
            .Should()
            .NotBeSameAs(result2);
    }
}
