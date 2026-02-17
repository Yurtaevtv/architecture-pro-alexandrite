using NLog.Extensions.Logging;
using OpenTelemetry.Exporter.Jaeger;
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
    .ConfigureResource(rb => rb.AddService(serviceName: builder.Environment.ApplicationName));

// Add Metrics for ASP.NET Core and our custom metrics and export via OTLP
otel.WithMetrics(metrics =>
{
    // Metrics provider from OpenTelemetry
    // Metrics provider from OpenTelemetry
    metrics.AddAspNetCoreInstrumentation()
        // Metrics provides by ASP.NET Core in .NET 8
        .AddMeter("Microsoft.AspNetCore.Hosting")
        .AddMeter("Microsoft.AspNetCore.Server.Kestrel")
        // Metrics provided by System.Net libraries
        .AddMeter("System.Net.Http")
        .AddMeter("System.Net.NameResolution");
});

// Add Tracing for ASP.NET Core and our custom ActivitySource and export via OTLP
otel.WithTracing(tracing =>
{
    tracing.AddSource("service-a")
        .ConfigureResource(res => res.AddService(serviceName: builder.Environment.ApplicationName));
    tracing.AddAspNetCoreInstrumentation();
    tracing.AddHttpClientInstrumentation();
    tracing.AddOtlpExporter();
    tracing.AddJaegerExporter();
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
