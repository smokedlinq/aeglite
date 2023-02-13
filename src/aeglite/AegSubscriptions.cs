using Azure.Messaging;
using Azure.Messaging.EventGrid;
using Microsoft.Extensions.Options;

namespace aeglite;

public class AegSubscriptions
{
    private readonly AegOptions _options;
    private readonly StorageQueueDestination _storageQueue;

    public AegSubscriptions(IOptions<AegOptions> options, StorageQueueDestination storageQueue)
    {
        _options = options.Value;
        _storageQueue = storageQueue;
    }

    public virtual async Task DeliverAsync(object @event, CancellationToken cancellationToken)
    {
        await (@event switch
        {
            CloudEvent cloudEvent => DeliverAsync(cloudEvent, cancellationToken),
            EventGridEvent eventGridEvent => DeliverAsync(eventGridEvent, cancellationToken),
            _ => throw new InvalidOperationException()
        }).ConfigureAwait(false);
    }

    private async Task DeliverAsync(CloudEvent cloudEvent, CancellationToken cancellationToken)
    {
        var topic = cloudEvent.Source;
        await DeliverAsync(topic, cloudEvent, cancellationToken).ConfigureAwait(false);
    }

    private async Task DeliverAsync(EventGridEvent eventGridEvent, CancellationToken cancellationToken)
    {
        var topic = eventGridEvent.Topic;
        await DeliverAsync(topic, eventGridEvent, cancellationToken).ConfigureAwait(false);
    }

    private async Task DeliverAsync(string topic, object content, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(topic) || !_options.Topics.ContainsKey(topic))
        {
            return;
        }

        var topicOptions = _options.Topics[topic];
        var json = BinaryData.FromObjectAsJson(content);

        foreach (var subscription in topicOptions.Subscriptions)
        {
            switch (subscription.Type)
            {
                case AegSubscriptionType.StorageQueue:
                    await _storageQueue.DeliverAsync(subscription, json, cancellationToken).ConfigureAwait(false);
                    break;
                default:
                    throw new InvalidOperationException();
            }
        }
    }
}
