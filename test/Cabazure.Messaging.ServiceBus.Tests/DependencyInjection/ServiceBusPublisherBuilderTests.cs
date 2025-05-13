using Azure.Messaging.ServiceBus;
using Cabazure.Messaging.ServiceBus.DependencyInjection;

namespace Cabazure.Messaging.ServiceBus.Tests.DependencyInjection;

public class ServiceBusPublisherBuilderTests
{
    public record TMessage(string Property1);

    [Theory, AutoNSubstituteData]
    public void WithSenderOptions_Sets_SenderOptions(
        ServiceBusPublisherBuilder<TMessage> sut,
        [NoAutoProperties] ServiceBusSenderOptions options)
    {
        sut.WithSenderOptions(options);

        sut.SenderOptions.Should().Be(options);
    }

    [Theory, AutoNSubstituteData]
    public void WithProperty_Adds_Property_With_Name(
        ServiceBusPublisherBuilder<TMessage> sut,
        string name,
        Func<TMessage, object> valueSelector,
        TMessage message,
        [NoAutoProperties] ServiceBusMessage eventData)
    {
        sut.WithProperty(name, valueSelector);

        sut.GetEventDataModifier().Invoke(message, eventData);
        eventData.ApplicationProperties.Should().Contain(name, valueSelector(message));
    }

    [Theory, AutoNSubstituteData]
    public void WithProperty_Adds_Property_With_Resolved_Name(
        ServiceBusPublisherBuilder<TMessage> sut,
        TMessage message,
        [NoAutoProperties] ServiceBusMessage eventData)
    {
        sut.WithProperty(m => m.Property1);

        sut.GetEventDataModifier().Invoke(message, eventData);
        eventData.ApplicationProperties.Should().Contain(nameof(TMessage.Property1), message.Property1);
    }

    [Theory, AutoNSubstituteData]
    public void WithProperty_Adds_Property_With_Constant(
        ServiceBusPublisherBuilder<TMessage> sut,
        string name,
        object value,
        TMessage message,
        [NoAutoProperties] ServiceBusMessage eventData)
    {
        sut.WithProperty(name, value);

        sut.GetEventDataModifier().Invoke(message, eventData);
        eventData.ApplicationProperties.Should().Contain(name, value);
    }

    [Theory, AutoNSubstituteData]
    public void WithPartitionKey_Sets_PartitionKey_Selector(
        ServiceBusPublisherBuilder<TMessage> sut,
        string partitionKey,
        TMessage message,
        [NoAutoProperties] ServiceBusMessage eventData)
    {
        sut.WithPartitionKey(m => partitionKey);
        
        sut.GetEventDataModifier().Invoke(message, eventData);
        eventData.PartitionKey.Should().Be(partitionKey);
    }

    [Theory, AutoNSubstituteData]
    public void WithPartitionKey_Sets_PartitionKey_Constant(
        ServiceBusPublisherBuilder<TMessage> sut,
        string partitionKey,
        TMessage message,
        [NoAutoProperties] ServiceBusMessage eventData)
    {
        sut.WithPartitionKey(partitionKey);

        sut.GetEventDataModifier().Invoke(message, eventData);
        eventData.PartitionKey.Should().Be(partitionKey);
    }
}
