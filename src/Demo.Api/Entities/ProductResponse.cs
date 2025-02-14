using System;

namespace Demo.Api.Entities;

public record ProductResponse
{
    public Guid Id { get; init; }
    public string? Name { get; init; }
    public int Quantity { get; init; }
};
