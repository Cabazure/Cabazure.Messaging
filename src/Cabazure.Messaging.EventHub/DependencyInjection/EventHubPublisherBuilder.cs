using System.Linq.Expressions;

namespace Cabazure.Messaging.EventHub.DependencyInjection;

public class EventHubPublisherBuilder<T>
{
    public Dictionary<string, Func<T, object>> Properties { get; } = [];
    public Func<T, string>? PartitionKey { get; private set; }

    public EventHubPublisherBuilder<T> AddProperty(
        string name,
        Func<T, object> valueSelector)
    {
        Properties.Add(name, valueSelector);
        return this;
    }

    public EventHubPublisherBuilder<T> AddProperty(
        string name,
        object value)
    {
        Properties.Add(name, _ => value);
        return this;
    }

    public EventHubPublisherBuilder<T> AddProperty(
        Expression<Func<T, object>> valueSelector)
    {
        var name = valueSelector.Body switch
        {
            MemberExpression { Member.Name: { } n } => n,
            UnaryExpression { Operand: MemberExpression { Member.Name: { } n } } => n,

            _ => throw new ArgumentException(
                "Could not extract property name from expression",
                nameof(valueSelector)),
        };

        return AddProperty(name, valueSelector.Compile());
    }

    public EventHubPublisherBuilder<T> AddPartitionKey(
        Func<T, string> valueSelector)
    {
        PartitionKey = valueSelector;
        return this;
    }
}
