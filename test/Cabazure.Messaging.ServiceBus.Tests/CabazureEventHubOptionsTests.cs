using System.Text.Json;
using Azure.Core;

namespace Cabazure.Messaging.ServiceBus.Tests;

public class CabazureServiceBusOptionsTests
{
    [Theory, AutoNSubstituteData]
    public void WithSerializerOptions_Sets_SerializerOptions(
        [NoAutoProperties] CabazureServiceBusOptions sut,
        [NoAutoProperties] JsonSerializerOptions options)
    {
        sut.WithSerializerOptions(options);
        sut.SerializerOptions.Should().Be(options);
    }

    [Theory, AutoNSubstituteData]
    public void WithConnection_Sets_Namespace_And_Credentials(
        [NoAutoProperties] CabazureServiceBusOptions sut,
        string fullyQUalifiedNamespace,
        TokenCredential credential)
    {
        sut.WithConnection(fullyQUalifiedNamespace, credential);
        sut.FullyQualifiedNamespace.Should().Be(fullyQUalifiedNamespace);
        sut.Credential.Should().Be(credential);
    }

    [Theory, AutoNSubstituteData]
    public void WithConnection_Sets_ConnectionString(
        [NoAutoProperties] CabazureServiceBusOptions sut,
        string connectionString)
    {
        sut.WithConnection(connectionString);
        sut.ConnectionString.Should().Be(connectionString);
    }
}
