using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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


    private static readonly List<string> _randomTags = ["xpto", "wyz", "abs"];
    private static readonly List<string> _randomServer = ["s1", "f5", "mini5g"];

    public static IEndpointRouteBuilder MapMetricsGeneratorEndpoints(this IEndpointRouteBuilder endpoints)
    {
        static KeyValuePair<string, object?>[] GenerateValueTags(string? myTag, string? myContext)
        {
            if(string.IsNullOrWhiteSpace(myTag))
            {
                myTag = _randomTags[Random.Shared.Next(0, _randomTags.Count)];
            }

            if(string.IsNullOrWhiteSpace(myContext))
            {
                myContext = _randomServer[Random.Shared.Next(0, _randomServer.Count)];
            }

            return [
                new("MyTag", myTag),
                new("MyContext", myContext)];
        }

        endpoints.MapPost("/metrics-generator/counter", ([FromQuery] string? myTag = null, [FromQuery] string? myContext = null) =>
        {
            var delta = Random.Shared.Next(1, 5);

            var tags = GenerateValueTags(myTag, myContext);

            _counter.Add(delta, tags);
            return Results.Ok(new { delta, tags });
        });
        endpoints.MapPost("/metrics-generator/counter/{delta:int}", (int delta, [FromQuery] string? myTag = null, [FromQuery] string? myContext = null) =>
        {
            var tags = GenerateValueTags(myTag, myContext);

            _counter.Add(delta, tags);
            return Results.Ok(new { delta, tags });
        });



        endpoints.MapPost("/metrics-generator/up-down-counter", ([FromQuery] string? myTag = null, [FromQuery] string? myContext = null) =>
        {
            var tags = GenerateValueTags(myTag, myContext);

            var delta = Random.Shared.Next(1, 10) - 5;
            if(delta == 0)
            {
                delta = 1;
            }

            _upDownCounter.Add(delta, tags);
            return Results.Ok(new { delta, tags });
        });
        endpoints.MapPost("/metrics-generator/up-down-counter/{delta:int}", (int delta, [FromQuery] string? myTag = null, [FromQuery] string? myContext = null) =>
        {
            var tags = GenerateValueTags(myTag, myContext);

            _upDownCounter.Add(delta, tags);
            return Results.Ok(new { delta, tags });
        });



        endpoints.MapPost("/metrics-generator/gauge", ([FromQuery] string? myTag = null, [FromQuery] string? myContext = null) =>
        {
            var tags = GenerateValueTags(myTag, myContext);

            var value = Random.Shared.Next(1, 1_000);
            _gauge.Record(value, tags);
            return Results.Ok(new { value, tags });
        });
        endpoints.MapPost("/metrics-generator/gauge/{value:int}", (int value, [FromQuery] string? myTag = null, [FromQuery] string? myContext = null) =>
        {
            var tags = GenerateValueTags(myTag, myContext);

            _gauge.Record(value, tags);
            return Results.Ok(new { value, tags });
        });



        endpoints.MapPost("/metrics-generator/histogram", ([FromQuery] string? myTag = null, [FromQuery] string? myContext = null) =>
        {
            var tags = GenerateValueTags(myTag, myContext);

            var value = Random.Shared.Next(1, 6_000);
            _histogram.Record(value, tags);
            return Results.Ok(new { value, tags });
        });
        endpoints.MapPost("/metrics-generator/histogram/{value:int}", (int value, [FromQuery] string? myTag = null, [FromQuery] string? myContext = null) =>
        {
            var tags = GenerateValueTags(myTag, myContext);

            _histogram.Record(value, tags);
            return Results.Ok(new { value, tags });
        });

        return endpoints;
    }
}
