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

    [Theory]
    [InlineData(0)]
    [InlineData(60)]
    [InlineData(604800)]
    public void WithRetryDelay_Enables_Retries(
        int seconds)
    {
        var sut = new StorageQueueProcessorBuilder();
        var retryDelay = TimeSpan.FromSeconds(seconds);

        sut.WithRetryDelay(retryDelay).Should().BeSameAs(sut);

        sut.Options.RetryDelay.Should().Be(retryDelay);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(604801)]
    public void WithRetryDelay_Rejects_Invalid_Visibility_Timeout(
        int seconds)
    {
        var sut = new StorageQueueProcessorBuilder();

        var act = () => sut.WithRetryDelay(TimeSpan.FromSeconds(seconds));

        act.Should().Throw<ArgumentOutOfRangeException>()
            .WithParameterName("retryDelay");
    }
}
