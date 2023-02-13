using System.Net;
using System.Net.Mime;
using System.Text;
using System.Text.Json;
using Azure.Messaging.EventGrid;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using static Program;

namespace aeglite.Tests;

public class PostApiEventsTests
{
    [Fact]
    public async Task PostApiEvents_ShouldReturnOk()
    {
        // Arrange
        using var factory = new WebApplicationFactory<Program>();
        var client = factory.CreateClient();
        var eventGridEvent = new EventGridEvent("test", "test", "test", "test");
        var content = JsonContent(eventGridEvent);

        // Act
        var response = await client.PostAsync("/api/events", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task PostApiEvents_ShouldReturnBadRequest_WhenPayloadIsEventGridEventAndEndpointIsConfiguredForCloudEvent()
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
        var client = factory.CreateClient();
        var eventGridEvent = new EventGridEvent("test", "test", "test", "test");
        var content = JsonContent(eventGridEvent);

        // Act
        var response = await client.PostAsync("/api/events", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task PostApiEvents_ShouldAppendToInMemoryBuffer()
    {
        // Arrange
        using var factory = new WebApplicationFactory<Program>();
        var client = factory.CreateClient();
        var eventGridEvent = new EventGridEvent("test", "test", "test", "test");
        var content = JsonContent(eventGridEvent);

        // Act
        _ = await client.PostAsync("/api/events", content);

        // Assert
        var cache = factory.Services.GetRequiredService<IMemoryCache>();
        var buffer = cache.Get<CircularBuffer<object>>(InMemoryCacheBufferKey);

        buffer.Should().HaveCount(1);
    }

    private static HttpContent JsonContent<T>(T value)
    {
        var json = BinaryData.FromObjectAsJson(value).ToString();
        return new StringContent(json, Encoding.UTF8, MediaTypeNames.Application.Json);
    }
}