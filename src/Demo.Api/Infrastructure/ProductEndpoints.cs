using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Demo.Api.Entities;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Demo.Api.Infrastructure;

public static class ProductEndpoints
{
    public static IEndpointRouteBuilder MapProductEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/products", async (Database db, CancellationToken cancellationToken) =>
        {
            // Latency simulation
            await Task.Delay(Random.Shared.Next(50, 100), cancellationToken);

            var products = db.List();

            return Results.Ok(products.Select(product => new ProductResponse
            {
                Id = product.Id,
                Name = product.Name,
                Quantity = product.Quantity
            }));
        });


        endpoints.MapGet("/products/{id:guid}", async (Database db, Guid id, CancellationToken cancellationToken) =>
        {
            // Latency simulation
            await Task.Delay(Random.Shared.Next(10, 500), cancellationToken);

            var product = db.Get(id);
            if(product is null)
            {
                return Results.NotFound();
            }

            return Results.Ok(new ProductResponse
            {
                Id = product.Id,
                Name = product.Name,
                Quantity = product.Quantity
            });
        }).WithName("GetProduct");


        endpoints.MapPost("/products", async (Queue queue, ProductRequest request, CancellationToken cancellationToken) =>
        {
            // Latency simulation
            await Task.Delay(Random.Shared.Next(10, 100), cancellationToken);

            if(string.IsNullOrWhiteSpace(request.Name))
            {
                return Results.BadRequest("Name is required");
            }

            var id = Guid.NewGuid();

            queue.Enqueue(id, request);

            return TypedResults.AcceptedAtRoute(
                "GetProduct",
                new { id });
        });


        endpoints.MapPut("/products/{id:guid}", async (Database db, Guid id, ProductRequest request, CancellationToken cancellationToken) =>
        {
            // Latency simulation
            await Task.Delay(Random.Shared.Next(300, 1500), cancellationToken);

            if(string.IsNullOrWhiteSpace(request.Name))
            {
                return Results.BadRequest("Name is required");
            }

            var product = db.Get(id);
            if(product is null)
            {
                return Results.NotFound();
            }

            product.Name = request.Name;
            product.Quantity = request.Quantity;

            db.Update(product);

            return Results.NoContent();
        });


        endpoints.MapDelete("/products/{id:guid}", async (Database db, Guid id, CancellationToken cancellationToken) =>
        {
            // Latency simulation
            await Task.Delay(Random.Shared.Next(200, 500), cancellationToken);

            var product = db.Get(id);
            if(product is null)
            {
                return Results.NotFound();
            }
            db.Delete(product);

            return Results.NoContent();
        });

        return endpoints;
    }
}
