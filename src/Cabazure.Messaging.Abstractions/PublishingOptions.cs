namespace Cabazure.Messaging;

/// <summary>
/// Represents options that can be specified when publishing messages, including metadata and routing information.
/// </summary>
public class PublishingOptions
{
    /// <summary>
    /// Gets or sets the content type of the message.
    /// </summary>
    public string? ContentType { get; set; }

    /// <summary>
    /// Gets or sets the correlation identifier for the message, used to correlate related messages.
    /// </summary>
    public string? CorrelationId { get; set; }

    /// <summary>
    /// Gets or sets the unique identifier of the message.
    /// </summary>
    public string? MessageId { get; set; }

    /// <summary>
    /// Gets or sets the partition key used for message ordering and routing.
    /// </summary>
    public string? PartitionKey { get; set; }

    /// <summary>
    /// Gets or sets a collection of custom properties to include with the message.
    /// </summary>
    public IDictionary<string, object> Properties { get; set; }
        = new Dictionary<string, object>();
}
