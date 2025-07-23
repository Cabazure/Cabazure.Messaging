using System.Linq.Expressions;
using Azure.Messaging.ServiceBus;

namespace Cabazure.Messaging.ServiceBus.DependencyInjection;

/// <summary>
/// Provides a fluent API for configuring Service Bus publishers with message-specific customizations.
/// </summary>
/// <typeparam name="TMessage">The type of message that will be published.</typeparam>
public class ServiceBusPublisherBuilder<TMessage>
{
    public List<Action<TMessage, ServiceBusMessage>> EventDataModifiers { get; } = [];
    public ServiceBusSenderOptions? SenderOptions { get; private set; }

    public ServiceBusPublisherBuilder<TMessage> WithMessageId(
        Func<TMessage, string> valueSelector)
    {
        EventDataModifiers.Add((m, d) => d.MessageId = valueSelector(m));
        return this;
    }

    public ServiceBusPublisherBuilder<TMessage> WithSessionId(
        Func<TMessage, string> valueSelector)
    {
        EventDataModifiers.Add((m, d) => d.SessionId = valueSelector(m));
        return this;
    }

    public ServiceBusPublisherBuilder<TMessage> WithCorrelationId(
        string name,
        Func<TMessage, string> valueSelector)
    {
        EventDataModifiers.Add((m, d) => d.CorrelationId = valueSelector(m));
        return this;
    }

    public ServiceBusPublisherBuilder<TMessage> WithProperty(
        string name,
        Func<TMessage, object> valueSelector)
    {
        EventDataModifiers.Add((m, d) => d.ApplicationProperties.Add(name, valueSelector(m)));
        return this;
    }

    public ServiceBusPublisherBuilder<TMessage> WithProperty(
        string name,
        object value)
    {
        EventDataModifiers.Add((m, d) => d.ApplicationProperties.Add(name, value));
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
        EventDataModifiers.Add((m, d) => d.PartitionKey = valueSelector(m));
        return this;
    }

    public ServiceBusPublisherBuilder<TMessage> WithPartitionKey(
        string partitionKey)
    {
        EventDataModifiers.Add((m, d) => d.PartitionKey = partitionKey);
        return this;
    }

    public ServiceBusPublisherBuilder<TMessage> WithSenderOptions(
        ServiceBusSenderOptions senderOptions)
    {
        SenderOptions = senderOptions;
        return this;
    }

    public Action<object, ServiceBusMessage>? GetEventDataModifier()
        => EventDataModifiers.Count == 0
         ? null
         : (m, d) => EventDataModifiers.ForEach(i => i.Invoke((TMessage)m, d));
}
