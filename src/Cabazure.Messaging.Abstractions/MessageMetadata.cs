
namespace Cabazure.Messaging;

public class MessageMetadata
{
    public string? ContentType { get; init; }

    public string? CorrelationId { get; init; }

    public DateTimeOffset EnqueuedTime { get; set; }

    public string? MessageId { get; init; }

    public string? PartitionKey { get; init; }

    public IDictionary<string, object> Properties { get; init; }
        = new Dictionary<string, object>();
}
