using Azure;
using Azure.Core.Pipeline;
using Azure.Messaging.EventGrid;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace aeglite.Tests;

public class AegSubscriptionsTests
{
    [Fact]
    public async Task ShouldDeliverEventToStorageQueue_WhenTopicIsConfiguredToForwardToStorageQueue()
    {
        // Arrange
        var storageQueueSubscription = Substitute.For<StorageQueueDestination>();
        using var factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    services.Configure<AegOptions>(options => options.Topics = new()
                    {
                        ["test"] = new AegTopic
                        {
                            Subscriptions = new[]
                            {
                                new AegSubscription
                                {
                                    Type = AegSubscriptionType.StorageQueue,
                                    QueueName = "events"
                                }
                            }
                        }
                    });

                    services.AddSingleton<StorageQueueDestination>(storageQueueSubscription);
                });
            });

        var endpoint = new Uri($"http://{factory.Server.BaseAddress.Host}:{factory.Server.BaseAddress.Port}/api/events");
        var credential = new AzureSasCredential("sas");
        var options = new EventGridPublisherClientOptions
        {
            Transport = new HttpClientTransport(factory.CreateClient()),
            Retry =
            {
                MaxRetries = 0
            }
        };
        var publisher = new EventGridPublisherClient(endpoint, credential, options);
        var eventGridEvent = new EventGridEvent("test", "test", "test", "test")
        {
            Topic = "test"
        };

        // Act
        _ = await publisher.SendEventAsync(eventGridEvent);

        // Assert
        await storageQueueSubscription.Received()
            .DeliverAsync(Arg.Any<AegSubscription>(), Arg.Any<BinaryData>(), Arg.Any<CancellationToken>());
    }
}
