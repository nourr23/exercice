namespace ReceptionTracker.Domain.Orders;

public class Carton
{
    private readonly List<ProductLine> _products = [];
    public IReadOnlyCollection<ProductLine> Products => _products;
    public int Id { get; private set; }
    public string Code { get; private set; }

    // Required by EF Core.
    private Carton()
    {
        Code = null!;
    }

    // Required by EF Core
    public Carton(string code)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(code);
        Code = code;
    }

    public ProductLine AddProduct(string reference, string name, string color, string size, int expectedQuantity)
    {
        var product = new ProductLine(reference, name, color, size, expectedQuantity);
        _products.Add(product);
        return product;
    }

    public void SetReceived(bool isReceived)
    {
        foreach (var product in _products)
        {
            product.SetReceived(isReceived);
        }
    }

    public ReceptionProgress GetProgress() => ReceptionProgress.From(_products);

    public ReceptionStatus Status => GetProgress().Status;
}