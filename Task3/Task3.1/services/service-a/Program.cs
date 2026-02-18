using NLog.Extensions.Logging;
using OpenTelemetry.Exporter;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using ServiceA.Models.Settings;
using System.Diagnostics;

var builder = WebApplication.CreateBuilder(args);
builder.Logging.ClearProviders();
builder.Logging.SetMinimumLevel(LogLevel.Trace);
builder.Logging.AddConsole();
builder.Logging.AddNLog("NLog.config");

// Настройка OpenTelemetry
var otlpEndpoint = builder.Configuration.GetValue<string>("OTEL_EXPORTER_OTLP_ENDPOINT")
    ?? "http://localhost:4317";
var serviceName = builder.Configuration.GetValue<string>("OTEL_SERVICE_NAME") ?? "service-a";
var serviceVersion = "1.0.0";

Console.WriteLine($"OTLEndpoint:{otlpEndpoint}");

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
