using System;
using System.Diagnostics.Metrics;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using OpenTelemetry.Metrics;

namespace Demo.Api.Infrastructure;

public static class MetricsGeneratorEndpoints
{
    public static readonly Meter Meter = new("Metrics.Generator");

    private static readonly Counter<int> _counter = Meter.CreateCounter<int>(
        name: "test_generator_counter",
        unit: "un",
        description: "Counts the number of test generator");

    private static readonly UpDownCounter<int> _upDownCounter = Meter.CreateUpDownCounter<int>(
        name: "test_generator_up_down_counter",
        unit: "un",
        description: "Counts the number of test generator");

    private static readonly Gauge<int> _gauge = Meter.CreateGauge<int>(
        name: "test_generator_gauge",
        unit: "un",
        description: "Gauge of test generator");

    public static MeterProviderBuilder AddGeneratorView(this MeterProviderBuilder meterProviderBuilder)
        => meterProviderBuilder.AddView(
            "test_generator_histogram",
            new ExplicitBucketHistogramConfiguration { Boundaries = [10, 50, 100, 200, 500, 1000, 5000] });
    private static readonly Histogram<int> _histogram = Meter.CreateHistogram<int>(
        name: "test_generator_histogram",
        unit: "un",
        description: "Histogram of test generator");



    public static IEndpointRouteBuilder MapMetricsGeneratorEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/metrics-generator/counter", () =>
        {
            var delta = Random.Shared.Next(1, 5);
            _counter.Add(delta);
            return Results.Ok(new { delta });
        });
        endpoints.MapPost("/metrics-generator/counter/{delta:int}", (int delta) =>
        {
            _counter.Add(delta);
            return Results.Ok(new { delta });
        });



        endpoints.MapPost("/metrics-generator/up-down-counter", () =>
        {
            var delta = Random.Shared.Next(1, 10) - 5;
            if(delta == 0)
            {
                delta = 1;
            }

            _upDownCounter.Add(delta);
            return Results.Ok(new { delta });
        });
        endpoints.MapPost("/metrics-generator/up-down-counter/{delta:int}", (int delta) =>
        {
            _upDownCounter.Add(delta);
            return Results.Ok(new { delta });
        });



        endpoints.MapPost("/metrics-generator/gauge", () =>
        {
            var value = Random.Shared.Next(1, 1_000);
            _gauge.Record(value);
            return Results.Ok(new { value });
        });
        endpoints.MapPost("/metrics-generator/gauge/{value:int}", (int value) =>
        {
            _gauge.Record(value);
            return Results.Ok(new { value });
        });



        endpoints.MapPost("/metrics-generator/histogram", () =>
        {
            var value = Random.Shared.Next(1, 6_000);
            _histogram.Record(value);
            return Results.Ok(new { value });
        });
        endpoints.MapPost("/metrics-generator/histogram/{value:int}", (int value) =>
        {
            _histogram.Record(value);
            return Results.Ok(new { value });
        });

        return endpoints;
    }
}
