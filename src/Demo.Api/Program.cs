using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using Demo.Api.Infrastructure;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

var builder = WebApplication.CreateSlimBuilder(args);

builder.Services
    .AddOpenTelemetry()
    .UseOtlpExporter()
    .ConfigureResource(builder => builder
        .AddService(
            serviceName: Assembly.GetEntryAssembly()!.GetName().Name!,
            serviceVersion: Assembly.GetEntryAssembly()!.GetName().Version!.ToString(),
            serviceInstanceId: Environment.MachineName)
        .AddTelemetrySdk())
    .WithMetrics(options => options
        .AddMeter(Metrics.Meter.Name)
        .AddRequestDurationView()
        .AddMeter(MetricsGeneratorEndpoints.Meter.Name)
        .AddGeneratorView()
        .AddPrometheusExporter()
        .AddConsoleExporter());


builder.Services.AddSingleton<Database>();

builder.Services
    .AddSingleton<Queue>()
    .AddHostedService<Queue.Worker>();


var app = builder.Build();

app.UseRouting();
app.MapPrometheusScrapingEndpoint();


app.UseWhen(
    p => p.Request.Path.StartsWithSegments("/products", StringComparison.InvariantCultureIgnoreCase),
    config => config.Use(async (context, next) =>
    {
        var stopwatch = Stopwatch.StartNew();
        try
        {
            await next(context);
        }
        finally
        {
            string? path = default;

            if(context.GetEndpoint() is RouteEndpoint route)
            {
                path = route.RoutePattern.RawText;
            }

            if(string.IsNullOrWhiteSpace(path))
            {
                path = context.Request.Path.Value ?? "";
            }

            Metrics.RequestsTotal.Add(
                1,
                KeyValuePair.Create<string, object?>("method", context.Request.Method),
                KeyValuePair.Create<string, object?>("path", path),
                KeyValuePair.Create<string, object?>("status", context.Response.StatusCode));

            stopwatch.Stop();
            Metrics.RecordRequestDuration(
                stopwatch.ElapsedMilliseconds,
                context.Request.Method,
                path);
        }
    }));


app.MapProductEndpoints();
app.MapMetricsGeneratorEndpoints();


await app.RunAsync();
