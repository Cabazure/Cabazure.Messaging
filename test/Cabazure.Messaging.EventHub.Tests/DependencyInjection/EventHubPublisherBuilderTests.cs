using Azure.Messaging.EventHubs;
using Cabazure.Messaging.EventHub.DependencyInjection;

namespace Cabazure.Messaging.EventHub.Tests.DependencyInjection;

public class EventHubPublisherBuilderTests
{
    public record TMessage(string Property1);

    [Theory, AutoNSubstituteData]
    public void WithProperty_Adds_Property_With_Name(
        EventHubPublisherBuilder<TMessage> sut,
        string name,
        string value,
        TMessage message,
        EventData eventData)
    {
        sut.WithProperty(name, m => value);

        sut.GetEventDataModifier().Invoke(message, eventData);
        eventData.Properties.Should().Contain(name, value);
    }

    [Theory, AutoNSubstituteData]
    public void WithProperty_Adds_Property_With_Resolved_Name(
        EventHubPublisherBuilder<TMessage> sut,
        TMessage message,
        EventData eventData)
    {
        sut.WithProperty(m => m.Property1);

        sut.GetEventDataModifier().Invoke(message, eventData);
        eventData.Properties.Should().Contain(nameof(TMessage.Property1), message.Property1);
    }

    [Theory, AutoNSubstituteData]
    public void WithProperty_Adds_Property_With_Constant(
        EventHubPublisherBuilder<TMessage> sut,
        string name,
        object value,
        TMessage message,
        EventData eventData)
    {
        sut.WithProperty(name, value);


        sut.GetEventDataModifier().Invoke(message, eventData);
        eventData.Properties.Should().Contain(name, value);
    }

    [Theory, AutoNSubstituteData]
    public void WithPartitionKey_Sets_PartitionKey_Selector(
        EventHubPublisherBuilder<TMessage> sut,
        Func<TMessage, string> partitionKeySelector)
    {
        sut.WithPartitionKey(partitionKeySelector);

        sut.PartitionKeyFactory
            .Should()
            .Be(partitionKeySelector);
    }

    [Theory, AutoNSubstituteData]
    public void WithPartitionKey_Sets_PartitionKey_Constant(
        EventHubPublisherBuilder<TMessage> sut,
        string partitionKey,
        TMessage message)
    {
        sut.WithPartitionKey(partitionKey);

        sut.PartitionKeyFactory
            .Invoke(message)
            .Should()
            .Be(partitionKey);
    }

    [Theory, AutoNSubstituteData]
    public void GetPartitionKeyFactory_Returns_Function_For_Selecting_PartitionKey(
        EventHubPublisherBuilder<TMessage> sut,
        string partitionKey,
        TMessage message)
    {
        sut.WithPartitionKey(_ = partitionKey);

        sut.GetPartitionKeyFactory()
            .Invoke(message)
            .Should()
            .BeEquivalentTo(partitionKey);
    }
}
