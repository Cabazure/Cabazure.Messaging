using System.Text.Json;
using Azure.Core;

namespace Cabazure.Messaging.EventHub.Tests;

public class CabazureEventHubOptionsTests
{
    [Theory, AutoNSubstituteData]
    public void WithSerializerOptions_Sets_SerializerOptions(
        [NoAutoProperties] CabazureEventHubOptions sut,
        [NoAutoProperties] JsonSerializerOptions options)
    {
        sut.WithSerializerOptions(options);
        sut.SerializerOptions.Should().Be(options);
    }

    [Theory, AutoNSubstituteData]
    public void WithConnection_Sets_Namespace_And_Credentials(
        [NoAutoProperties] CabazureEventHubOptions sut,
        string fullyQUalifiedNamespace,
        TokenCredential credential)
    {
        sut.WithConnection(fullyQUalifiedNamespace, credential);
        sut.FullyQualifiedNamespace.Should().Be(fullyQUalifiedNamespace);
        sut.Credential.Should().Be(credential);
    }

    [Theory, AutoNSubstituteData]
    public void WithConnection_Sets_ConnectionString(
        [NoAutoProperties] CabazureEventHubOptions sut,
        string connectionString)
    {
        sut.WithConnection(connectionString);
        sut.ConnectionString.Should().Be(connectionString);
    }

    [Theory, AutoNSubstituteData]
    public void WithBlobStorage_Sets_BlobStorageOptions_With_ContainerUri(
        [NoAutoProperties] CabazureEventHubOptions sut,
        Uri containerUri,
        TokenCredential credential,
        bool createIfNotExist)
    {
        sut.WithBlobStorage(containerUri, credential, createIfNotExist);
        sut.BlobStorage
            .Should()
            .BeEquivalentTo(
                new BlobStorageOptions
                {
                    ContainerUri = containerUri,
                    Credential = credential,
                    CreateIfNotExist = createIfNotExist,
                });
    }

    [Theory, AutoNSubstituteData]
    public void WithBlobStorage_Sets_BlobStorageOptions_With_ConnectionString(
        [NoAutoProperties] CabazureEventHubOptions sut,
        string connectionString,
        string blobContainerName,
        bool createIfNotExist)
    {
        sut.WithBlobStorage(connectionString, blobContainerName, createIfNotExist);
        sut.BlobStorage
            .Should()
            .BeEquivalentTo(
                new BlobStorageOptions
                {
                    ConnectionString = connectionString,
                    ContainerName = blobContainerName,
                    CreateIfNotExist = createIfNotExist,
                });
    }
}
