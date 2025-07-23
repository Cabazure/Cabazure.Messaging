
namespace Cabazure.Messaging;

/// <summary>
/// Represents metadata associated with a message, containing information such as identifiers, timing, and custom properties.
/// </summary>
public class MessageMetadata
{
    /// <summary>
    /// Gets or sets the content type of the message.
    /// </summary>
    public string? ContentType { get; init; }

    /// <summary>
    /// Gets or sets the correlation identifier for the message, used to correlate related messages.
    /// </summary>
    public string? CorrelationId { get; init; }

    /// <summary>
    /// Gets or sets the time when the message was enqueued.
    /// </summary>
    public DateTimeOffset EnqueuedTime { get; init; }

    /// <summary>
    /// Gets or sets the unique identifier of the message.
    /// </summary>
    public string? MessageId { get; init; }

    /// <summary>
    /// Gets or sets the partition key used for message ordering and routing.
    /// </summary>
    public string? PartitionKey { get; init; }

    /// <summary>
    /// Gets or sets a collection of custom properties associated with the message.
    /// </summary>
    public IReadOnlyDictionary<string, object> Properties { get; init; }
        = new Dictionary<string, object>();
}
