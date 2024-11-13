namespace Cabazure.Messaging;

public class MessageMetadata
{
    public string? ContentType { get; init; }

    public string? CorrelationId { get; init; }

    public string? MessageId { get; init; }

    public string? PartitionKey { get; init; }

    public IReadOnlyDictionary<string, object> Properties { get; init; }
        = new Dictionary<string, object>();
}
