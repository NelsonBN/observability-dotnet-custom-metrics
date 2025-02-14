namespace Demo.Api.Entities;

public record ProductRequest
{
    public required string Name { get; init; }
    public int Quantity { get; init; }
};
