using NLog.Extensions.Logging;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using ServiceA.Models.Settings;

var builder = WebApplication.CreateBuilder(args);
builder.Logging.ClearProviders();
builder.Logging.SetMinimumLevel(LogLevel.Trace);
builder.Logging.AddConsole();
builder.Logging.AddNLog("NLog.config");
// Add services to the container.


var otel = builder.Services.AddOpenTelemetry()
    .ConfigureResource(rb => rb.AddService("service-a"));

// Add Metrics for ASP.NET Core and our custom metrics and export via OTLP
otel.WithMetrics(metrics =>
{
    // Metrics provider from OpenTelemetry
    metrics.AddAspNetCoreInstrumentation();
});

// Add Tracing for ASP.NET Core and our custom ActivitySource and export via OTLP
otel.WithTracing(tracing =>
{
    tracing.AddAspNetCoreInstrumentation();
    tracing.AddHttpClientInstrumentation();
    tracing.AddOtlpExporter();
});

var settings = builder.Configuration.GetSection("Service").Get<ClientSettings>();

builder.Services.AddHttpClient("service_b")
                .ConfigureHttpClient(hc => hc.BaseAddress = new Uri(settings!.Url));



var app = builder.Build();

// Configure the HTTP request pipeline.

app.MapGet("/", async (IHttpClientFactory hcf) =>
{
    HttpClient client = hcf.CreateClient("service_b");

    HttpResponseMessage response = await client.GetAsync("/");

    return await response.Content.ReadAsStringAsync();
});

app.Run();
