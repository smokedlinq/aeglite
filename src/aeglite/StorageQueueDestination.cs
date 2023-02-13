using Azure.Storage.Queues;

namespace aeglite;

public class StorageQueueDestination
{
    private readonly QueueServiceClient _service = new("UseDevelopmentStorage=true");

    public virtual async Task DeliverAsync(AegSubscription subscription, BinaryData json, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(subscription.QueueName))
        {
            throw new InvalidOperationException("QueueName is required");
        }

        var client = _service.GetQueueClient(subscription.QueueName);
        await client.SendMessageAsync(json, visibilityTimeout: subscription.VisibilityTimeout, cancellationToken: cancellationToken).ConfigureAwait(false);
    }
}
