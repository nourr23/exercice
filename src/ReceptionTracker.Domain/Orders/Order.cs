using ReceptionTracker.Domain.Common;

namespace ReceptionTracker.Domain.Orders;

/// <summary>
/// A supplier order (the delivery) waiting to be received.
/// Aggregate root: pallets, cartons and products are always changed through it.
/// </summary>
public class Order
{
    public int Id { get; private set; }
    public string Reference { get; private set; }


    public ReceptionStatus Status => GetProgress().Status;

    private readonly List<Pallet> _pallets = [];
    public IReadOnlyCollection<Pallet> Pallets => _pallets;

    public IEnumerable<ProductLine> Products => _pallets.SelectMany(p => p.Products);

    // Required by EF Core
    private Order() { Reference = null!; }

    public Order(string reference)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(reference);
        Reference = reference;
    }


    public Pallet AddPallet(string code)
    {
        if (_pallets.Any(p => p.Code == code))
        {
            throw new DomainException($"Pallet '{code}' already exists on a Pallet '{code}'.");
        }
        var pallet = new Pallet(code);
        _pallets.Add(pallet);
        return pallet;
    }
    
    public ProductLine? FindProductLine(int id) => Products.SingleOrDefault(p => p.Id == id);  

    public ReceptionProgress GetProgress() => ReceptionProgress.From(Products);
    public Pallet? FindPallet(string code) => _pallets.SingleOrDefault(p => p.Code == code);
}