using Cabazure.Messaging.StorageQueue.DependencyInjection;

namespace Cabazure.Messaging.StorageQueue.Tests.DependencyInjection;

public class StorageQueueProcessorBuilderTests
{
    [Theory, AutoNSubstituteData]
    public void WithPollingInterval_Adds_PollingInterval(
        StorageQueueProcessorBuilder sut,
        TimeSpan pollingInterval)
    {
        sut.WithPollingInterval(pollingInterval);

        sut.Options.PollingInterval.Should().Be(pollingInterval);
    }

    [Theory]
    [InlineAutoNSubstituteData(true)]
    [InlineAutoNSubstituteData(false)]
    public void WithProcessorOptions_Sets_ProcessorOptions(
        bool createIfNotExists,
        StorageQueueProcessorBuilder sut)
    {
        sut.WithInitialization(createIfNotExists);

        sut.Options.CreateIfNotExists.Should().Be(createIfNotExists);
    }
}
