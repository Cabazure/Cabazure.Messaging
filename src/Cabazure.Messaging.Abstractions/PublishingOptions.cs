namespace Cabazure.Messaging;

public class PublishingOptions
{
    public string? ContentType { get; set; }

    public string? CorrelationId { get; set; }

    public string? MessageId { get; set; }

    public string? PartitionKey { get; set; }

    public IDictionary<string, object> Properties { get; set; }
        = new Dictionary<string, object>();
}
