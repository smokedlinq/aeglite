using Azure;
using Azure.Core.Pipeline;
using Azure.Messaging;
using Azure.Messaging.EventGrid;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace aeglite.Tests;

public class EventGridPublisherClientCompatibilityTests
{
    [Fact]
    public async Task SendEventAsync_ShouldBeSuccess_WhenSendingCloudEvent()
    {
        // Arrange
        using var factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    services.Configure<AegOptions>(options => options.Schema = AegSchema.CloudEvent);
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
        var cloudEvent = new CloudEvent("test", "test", "test");

        // Act
        var response = await publisher.SendEventAsync(cloudEvent);

        // Assert
        response.IsError.Should().BeFalse();
    }

    [Fact]
    public async Task SendEventAsync_ShouldBeSuccess_WhenSendingEventGridEvent()
    {
        // Arrange
        using var factory = new WebApplicationFactory<Program>();
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
        var eventGridEvent = new EventGridEvent("test", "test", "test", "test");

        // Act
        var response = await publisher.SendEventAsync(eventGridEvent);

        // Assert
        response.IsError.Should().BeFalse();
    }
}