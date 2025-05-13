using System.Linq.Expressions;
using Azure.Messaging.EventHubs;

namespace Cabazure.Messaging.EventHub.DependencyInjection;

public class EventHubPublisherBuilder<TMessage>
{
    public Func<TMessage, string>? PartitionKeyFactory { get; private set; }

    public List<Action<TMessage, EventData>> EventDataModifiers { get; } = [];

    public EventHubPublisherBuilder<TMessage> WithMessageId(
        Func<TMessage, string> valueSelector)
    {
        EventDataModifiers.Add((m, d) => d.MessageId = valueSelector(m));
        return this;
    }

    public EventHubPublisherBuilder<TMessage> WithCorrelationId(
        Func<TMessage, string> valueSelector)
    {
        EventDataModifiers.Add((m, d) => d.CorrelationId = valueSelector(m));
        return this;
    }

    public EventHubPublisherBuilder<TMessage> WithProperty(
        string name,
        Func<TMessage, object> valueSelector)
    {
        EventDataModifiers.Add((m, d) => d.Properties.Add(name, valueSelector(m)));
        return this;
    }

    public EventHubPublisherBuilder<TMessage> WithProperty(
        string name,
        object value)
    {
        EventDataModifiers.Add((_, d) => d.Properties.Add(name, value));
        return this;
    }

    public EventHubPublisherBuilder<TMessage> WithProperty(
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

    public EventHubPublisherBuilder<TMessage> WithPartitionKey(
        Func<TMessage, string> valueSelector)
    {
        PartitionKeyFactory = valueSelector;
        return this;
    }

    public EventHubPublisherBuilder<TMessage> WithPartitionKey(
        string partitionKey)
    {
        PartitionKeyFactory = _ => partitionKey;
        return this;
    }

    public Func<object, string>? GetPartitionKeyFactory()
        => PartitionKeyFactory == null
         ? null
         : o => PartitionKeyFactory.Invoke((TMessage)o);

    public Action<object, EventData>? GetEventDataModifier()
        => EventDataModifiers.Count == 0
         ? null
         : (m, d) => EventDataModifiers.ForEach(i => i.Invoke((TMessage)m, d));
}
