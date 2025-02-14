using System;
using System.Collections.Generic;
using Demo.Api.Entities;

namespace Demo.Api.Infrastructure;

public class Database
{
    private readonly Dictionary<Guid, Product> _products = [];

    public void Add(Product product)
    {
        Metrics.NumberOfProducts.Add(1);

        _products.Add(product.Id, product);
    }

    public Product? Get(Guid id)
        => _products.TryGetValue(id, out var product) ? product : null;

    public void Update(Product product)
        => _products[product.Id] = product;

    public void Delete(Product product)
    {
        Metrics.NumberOfProducts.Add(-1);

        _products.Remove(product.Id);
    }

    public IEnumerable<Product> List()
        => _products.Values;

    public bool Any(Guid id)
        => _products.ContainsKey(id);
}
