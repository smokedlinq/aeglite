using Microsoft.Extensions.Options;
using aeglite;
using Azure.Messaging.EventGrid;
using Azure.Messaging;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Caching.Memory;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddEnvironmentVariables("AEG_");

builder.Services.AddOptions<AegOptions>()
    .Configure<IConfiguration>((options, configuration) => configuration.Bind(options));

builder.Services.AddMemoryCache();

builder.Services.AddSingleton<AegSubscriptions>();
builder.Services.AddSingleton<StorageQueueDestination>();

var app = builder.Build();

app.MapGet("/healthz", () => Results.Ok());

app.MapPost("/api/events", async (HttpContext context, CancellationToken cancellationToken) =>
{
    var options = context.RequestServices.GetRequiredService<IOptions<AegOptions>>().Value;
    var json = await BinaryData.FromStreamAsync(context.Request.Body, cancellationToken).ConfigureAwait(false);

    if (!TryParseContent(options.Schema, json, out var events))
    {
        return Results.BadRequest();
    }

    var cache = context.RequestServices.GetRequiredService<IMemoryCache>();
    var buffer = cache.GetOrCreate(InMemoryCacheBufferKey, _ => new CircularBuffer<object>(options.BufferSize))
                ?? throw new InvalidOperationException("Memory cache failed to initialize the event buffer.");
    var subscriptions = context.RequestServices.GetRequiredService<AegSubscriptions>();

    foreach (var @event in events)
    {
        buffer.Append(@event);
        await subscriptions.DeliverAsync(@event, cancellationToken).ConfigureAwait(false);
    }

    return Results.Ok();
});

app.Run();

static bool TryParseContent(AegSchema schema, BinaryData json, [MaybeNullWhen(false)] out IEnumerable<object> result)
{
    try
    {
        result = schema switch
        {
            AegSchema.EventGrid => EventGridEvent.ParseMany(json),
            AegSchema.CloudEvent => CloudEvent.ParseMany(json),
            _ => throw new InvalidOperationException("Invalid schema")
        };
        return true;
    }
    catch
    {
        result = null;
        return false;
    }
}

public partial class Program
{
    public const string InMemoryCacheBufferKey = "events";

    protected Program()
    {
    }
}
