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
        Func<TMessage, object> valueSelector)
    {
        sut.WithProperty(name, valueSelector);

        sut.Properties.Should().Contain(name, valueSelector);
    }

    [Theory, AutoNSubstituteData]
    public void WithProperty_Adds_Property_With_Resolved_Name(
        ServiceBusPublisherBuilder<TMessage> sut,
        TMessage message)
    {
        sut.WithProperty(m => m.Property1);

        sut.Properties.Should().ContainKey(nameof(TMessage.Property1));
        sut.Properties[nameof(TMessage.Property1)]
            .Invoke(message)
            .Should()
            .Be(message.Property1);
    }

    [Theory, AutoNSubstituteData]
    public void WithProperty_Adds_Property_With_Constant(
        ServiceBusPublisherBuilder<TMessage> sut,
        string name,
        object value,
        TMessage message)
    {
        sut.WithProperty(name, value);

        sut.Properties.Should().ContainKey(name);
        sut.Properties[name]
            .Invoke(message)
            .Should()
            .Be(value);
    }

    [Theory, AutoNSubstituteData]
    public void WithPartitionKey_Sets_PartitionKey_Selector(
        ServiceBusPublisherBuilder<TMessage> sut,
        Func<TMessage, string> partitionKeySelector)
    {
        sut.WithPartitionKey(partitionKeySelector);

        sut.PartitionKey
            .Should()
            .Be(partitionKeySelector);
    }

    [Theory, AutoNSubstituteData]
    public void WithPartitionKey_Sets_PartitionKey_Constant(
        ServiceBusPublisherBuilder<TMessage> sut,
        string partitionKey,
        TMessage message)
    {
        sut.WithPartitionKey(partitionKey);

        sut.PartitionKey
            .Invoke(message)
            .Should()
            .Be(partitionKey);
    }

    [Theory, AutoNSubstituteData]
    public void GetPropertyFactory_Returns_Function_For_Selecting_Properties(
        ServiceBusPublisherBuilder<TMessage> sut,
        Dictionary<string, object> properties,
        TMessage message)
    {
        foreach (var property in properties)
        {
            sut.Properties.Add(property.Key, _ => property.Value);
        }

        sut.GetPropertyFactory()
            .Invoke(message)
            .Should()
            .BeEquivalentTo(properties);
    }

    [Theory, AutoNSubstituteData]
    public void GetPartitionKeyFactory_Returns_Function_For_Selecting_PartitionKey(
        ServiceBusPublisherBuilder<TMessage> sut,
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
