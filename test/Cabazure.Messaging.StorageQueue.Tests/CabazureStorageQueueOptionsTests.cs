using System.Text.Json;
using Azure.Core;

namespace Cabazure.Messaging.StorageQueue.Tests;

public class CabazureStorageQueueOptionsTests
{
    [Theory, AutoNSubstituteData]
    public void WithSerializerOptions_Sets_SerializerOptions(
        [NoAutoProperties] CabazureStorageQueueOptions sut,
        [NoAutoProperties] JsonSerializerOptions options)
    {
        sut.WithSerializerOptions(options);
        sut.SerializerOptions.Should().Be(options);
    }

    [Theory, AutoNSubstituteData]
    public void WithConnection_Sets_Namespace_And_Credentials(
        [NoAutoProperties] CabazureStorageQueueOptions sut,
        Uri queueServiceUri,
        TokenCredential credential)
    {
        sut.WithConnection(queueServiceUri, credential);
        sut.QueueServiceUri.Should().Be(queueServiceUri);
        sut.Credential.Should().Be(credential);
    }

    [Theory, AutoNSubstituteData]
    public void WithConnection_Sets_ConnectionString(
        [NoAutoProperties] CabazureStorageQueueOptions sut,
        string connectionString)
    {
        sut.WithConnection(connectionString);
        sut.ConnectionString.Should().Be(connectionString);
    }
}
