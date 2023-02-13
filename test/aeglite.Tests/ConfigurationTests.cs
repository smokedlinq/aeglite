using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace aeglite.Tests;

public class ConfigurationTests
{
    [Fact]
    public void AegOptions_ShouldHaveSchemaCloudEvent_WhenConfigurationHasSchemaCloudEvent()
    {
        // Arrange
        using var factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureAppConfiguration((context, config) =>
                {
                    config.AddInMemoryCollection(new Dictionary<string, string?>
                    {
                        ["SCHEMA"] = nameof(AegSchema.CloudEvent)
                    });
                });
            });

        // Act
        var options = factory.Services.GetRequiredService<IOptions<AegOptions>>().Value;

        // Assert
        options.Schema.Should().Be(AegSchema.CloudEvent);
    }

    [Fact]
    public void AegOptions_ShouldHaveBufferSize_WhenConfigurationHasBufferSize()
    {
        // Arrange
        using var factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureAppConfiguration((context, config) =>
                {
                    config.AddInMemoryCollection(new Dictionary<string, string?>
                    {
                        ["BUFFERSIZE"] = "1000"
                    });
                });
            });

        // Act
        var options = factory.Services.GetRequiredService<IOptions<AegOptions>>().Value;

        // Assert
        options.BufferSize.Should().Be(1000);
    }
}