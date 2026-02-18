using NLog.Extensions.Logging;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

var builder = WebApplication.CreateBuilder(args);
builder.Logging.ClearProviders();
builder.Logging.SetMinimumLevel(LogLevel.Trace);
builder.Logging.AddConsole();
builder.Logging.AddNLog("NLog.config");
// Add services to the container.

// Настройка OpenTelemetry
var otlpEndpoint = builder.Configuration.GetValue<string>("OTEL_EXPORTER_OTLP_ENDPOINT")
    ?? "http://localhost:4317";
var serviceName = builder.Configuration.GetValue<string>("OTEL_SERVICE_NAME") ?? "service-b";
var serviceVersion = "1.0.0";

var otel = builder.Services.AddOpenTelemetry()
    .WithTracing(tracing =>
    {
        tracing.SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(serviceName))
            .AddAspNetCoreInstrumentation()
            .AddOtlpExporter(opts =>
            {
                opts.Endpoint = new Uri(otlpEndpoint);
            });
    });

var app = builder.Build();

// Configure the HTTP request pipeline.
app.MapGet("/", () => $"New some id {Guid.NewGuid()}");

app.Run();
