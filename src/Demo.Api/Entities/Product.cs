using System;

namespace Demo.Api.Entities;

public class Product
{
    public required Guid Id { get; set; }
    public required string Name { get; set; }
    public int Quantity { get; set; }
}
