
namespace Cabazure.Messaging;

/// <summary>
/// Represents metadata associated with a message, containing information such as identifiers, timing, and custom properties.
/// </summary>
public class MessageMetadata
{
    public string? ContentType { get; init; }

    public string? CorrelationId { get; init; }

    public DateTimeOffset EnqueuedTime { get; init; }

    public string? MessageId { get; init; }

    public string? PartitionKey { get; init; }

    public IReadOnlyDictionary<string, object> Properties { get; init; }
        = new Dictionary<string, object>();
}
