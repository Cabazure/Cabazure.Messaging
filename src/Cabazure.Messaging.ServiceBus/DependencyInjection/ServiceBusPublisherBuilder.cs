using System.Linq.Expressions;
using Azure.Messaging.ServiceBus;

namespace Cabazure.Messaging.ServiceBus.DependencyInjection;

public class ServiceBusPublisherBuilder<TMessage>
{
    public Dictionary<string, Func<TMessage, object>> Properties { get; } = [];
    public Func<TMessage, string>? PartitionKey { get; private set; }
    public ServiceBusSenderOptions? SenderOptions { get; private set; }

    public ServiceBusPublisherBuilder<TMessage> WithProperty(
        string name,
        Func<TMessage, object> valueSelector)
    {
        Properties.Add(name, valueSelector);
        return this;
    }

    public ServiceBusPublisherBuilder<TMessage> WithProperty(
        string name,
        object value)
    {
        Properties.Add(name, _ => value);
        return this;
    }

    public ServiceBusPublisherBuilder<TMessage> WithProperty(
        Expression<Func<TMessage, object>> valueSelector)
    {
        var name = valueSelector.Body switch
        {
            MemberExpression { Member.Name: { } n } => n,
            UnaryExpression { Operand: MemberExpression { Member.Name: { } n } } => n,

            _ => throw new ArgumentException(
                "Could not extract property name from expression",
                nameof(valueSelector)),
        };

        return WithProperty(name, valueSelector.Compile());
    }

    public ServiceBusPublisherBuilder<TMessage> WithPartitionKey(
        Func<TMessage, string> valueSelector)
    {
        PartitionKey = valueSelector;
        return this;
    }

    public ServiceBusPublisherBuilder<TMessage> WithPartitionKey(
        string partitionKey)
    {
        PartitionKey = _ => partitionKey;
        return this;
    }

    public ServiceBusPublisherBuilder<TMessage> WithSenderOptions(
        ServiceBusSenderOptions senderOptions)
    {
        SenderOptions = senderOptions;
        return this;
    }

    public Func<object, Dictionary<string, object>>? GetPropertyFactory()
        => Properties.Count == 0
         ? null
         : o => Properties.ToDictionary(
            kvp => kvp.Key,
            kvp => kvp.Value((TMessage)o));

    public Func<object, string>? GetPartitionKeyFactory()
        => PartitionKey == null
         ? null
         : o => PartitionKey.Invoke((TMessage)o);
}
