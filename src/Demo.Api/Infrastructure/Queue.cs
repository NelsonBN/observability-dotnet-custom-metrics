using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Demo.Api.Entities;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Demo.Api.Infrastructure;

public class Queue
{
    private readonly ConcurrentQueue<(Guid id, ProductRequest Request)> _queue = new();

    public void Enqueue(Guid id, ProductRequest product)
    {
        Metrics.IncrementQueuePending();
        _queue.Enqueue((id, product));
    }

    public (Guid Id, ProductRequest Product)? Dequeue()
    {
        if(_queue.TryDequeue(out var request))
        {
            return request;
        }

        return null;
    }



    public class Worker(
        ILogger<Worker> logger,
        Queue queue,
        Database database) : BackgroundService
    {
        private readonly ILogger<Worker> _logger = logger;
        private readonly Queue _queue = queue;
        private readonly Database _database = database;

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            (Guid Id, ProductRequest Product)? request = null;


            while(!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    request = _queue.Dequeue();

                    if(request is null)
                    {
                        const int DELAY = 5_000;
                        _logger.LogInformation("Queue is empty. Waiting for new requests by {Delay}ms.", DELAY);
                        await Task.Delay(DELAY, stoppingToken);
                        continue;
                    }

                    Metrics.IncrementMessageProcessed();

                    var stopwatch = Stopwatch.StartNew();

                    var newProduct = new Product
                    {
                        Id = request.Value.Id,
                        Name = request.Value.Product.Name,
                        Quantity = request.Value.Product.Quantity
                    };

                    // Latency simulation
                    var delay = Random.Shared.Next(500, 2_000);
                    await Task.Delay(delay, stoppingToken);


                    _database.Add(newProduct);


                    stopwatch.Stop();
                    Metrics.QueueProcessingTime.Record(stopwatch.ElapsedMilliseconds);
                    Metrics.DecrementQueuePending();

                    _logger.LogInformation(
                        "Product {Id}, Name: {Name}, Quantity: {Quantity} added to the database.",
                        newProduct.Id,
                        newProduct.Name,
                        newProduct.Quantity);
                }
                catch
                {
                    if(request is not null)
                    {
                        _queue.Enqueue(
                            request.Value.Id,
                            request.Value.Product);
                    }
                }
            }
        }
    }
}
