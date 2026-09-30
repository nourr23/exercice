namespace ReceptionTracker.Domain.Orders;

/// <summary>
/// A product variant (size + color) expected in a carton.
/// This is the only level that stores reception state.
/// </summary>
public class ProductLine
{
    public int Id { get; private set; }
    public string Reference { get; private set; }
    public string Name { get; private set; }
    public string Color { get; private set; }
    public string Size { get; private set; }
    public int ExpectedQuantity { get; private set; }
    public bool IsReceived { get; private set; }

    // Required by EF Core.
    private ProductLine()
    {
        Reference = Name = Color = Size = null!;
    }

    public ProductLine(string reference, string name, string color, string size, int expectedQuantity)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(reference);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(color);
        ArgumentException.ThrowIfNullOrWhiteSpace(size);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(expectedQuantity);

        Reference = reference;
        Name = name;
        Color = color;
        Size = size;
        ExpectedQuantity = expectedQuantity;
    }

    public void SetReceived(bool isReceived)    
    {
        IsReceived = isReceived;
    }
}