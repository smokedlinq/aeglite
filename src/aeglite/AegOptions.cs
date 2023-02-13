namespace aeglite;

public class AegOptions
{
    public AegSchema Schema { get; set; } = AegSchema.EventGrid;
    public int BufferSize { get; set; } = 100;
    public Dictionary<string, AegTopic> Topics { get; set; } = new();
}

public enum AegSchema
{
    EventGrid = 0,
    CloudEvent = 1
}

public class AegTopic
{
    public IEnumerable<AegSubscription> Subscriptions { get; set; } = Array.Empty<AegSubscription>();
}

public class AegSubscription
{
    public AegSubscriptionType Type { get; set; }
    public string? QueueName { get; set; }
    public TimeSpan VisibilityTimeout { get; set; }= TimeSpan.FromDays(7);
}

public enum AegSubscriptionType
{
    StorageQueue
}
