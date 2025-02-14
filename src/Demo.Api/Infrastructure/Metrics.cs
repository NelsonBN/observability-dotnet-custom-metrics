using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Reflection;
using System.Threading;
using OpenTelemetry.Metrics;

namespace Demo.Api.Infrastructure;

public static class Metrics
{
    private static readonly string _name = Assembly.GetEntryAssembly()!.GetName().Name!;
    public static readonly Meter Meter = new(_name);


    public static readonly Counter<int> RequestsTotal = Meter.CreateCounter<int>(
        name: "http_requests",
        unit: "requests",
        description: "Counts the number of HTTP requests");



    private static int _messageProcessedCounter = 0;
    public static readonly ObservableCounter<int> MessageProcessedCounter = Meter.CreateObservableCounter(
        name: "messages_processed",
        observeValue: () => _messageProcessedCounter,
        unit: "items",
        description: "Total number of messages processed by the queue");
    public static void IncrementMessageProcessed()
        => Interlocked.Increment(ref _messageProcessedCounter);



    public static readonly UpDownCounter<int> NumberOfProducts = Meter.CreateUpDownCounter<int>(
        name: "number_products",
        unit: "products",
        description: "Number of products in the system");



    private static int _queuePendingItems = 0;
    public static readonly ObservableUpDownCounter<int> QueuePendingItems = Meter.CreateObservableUpDownCounter(
        name: "queue_pending",
        observeValue: () => _queuePendingItems,
        unit: "items",
        description: "Number of items in the queue");
    public static void IncrementQueuePending() => Interlocked.Increment(ref _queuePendingItems);
    public static void DecrementQueuePending() => Interlocked.Decrement(ref _queuePendingItems);



    public static readonly Gauge<double> QueueProcessingTime = Meter.CreateGauge<double>(
        name: "queue_processing_time",
        unit: "ms",
        description: "Average processing time of items in the queue");



    public static readonly ObservableGauge<long> MemoryUsed = Meter.CreateObservableGauge(
        name: "memory_used",
        observeValue: () =>
        {
            using var process = Process.GetCurrentProcess();
            return process.WorkingSet64;
        },
        unit: "bytes",
        description: "Amount of memory used by the current process in bytes");



    public static MeterProviderBuilder AddRequestDurationView(this MeterProviderBuilder meterProviderBuilder)
        => meterProviderBuilder.AddView(
            "request_duration",
            new ExplicitBucketHistogramConfiguration { Boundaries = [10, 50, 100, 200, 500, 1000, 5000] });

    public static readonly Histogram<double> RequestDuration = Meter.CreateHistogram<double>(
        name: "request_duration",
        unit: "ms",
        description: "Duration of HTTP requests in milliseconds");
    public static void RecordRequestDuration(double duration, string method, string path)
        => RequestDuration.Record(
            duration,
            KeyValuePair.Create<string, object?>("method", method),
            KeyValuePair.Create<string, object?>("path", path));
}
