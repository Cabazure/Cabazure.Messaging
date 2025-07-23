namespace Cabazure.Messaging;

/// <summary>
/// Represents options that can be specified when publishing messages, including metadata and routing information.
/// </summary>
public class PublishingOptions
{
    public string? ContentType { get; set; }

    public string? CorrelationId { get; set; }

    public string? MessageId { get; set; }

    public string? PartitionKey { get; set; }

    public IDictionary<string, object> Properties { get; set; }
        = new Dictionary<string, object>();
}
