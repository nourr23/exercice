namespace ReceptionTracker.Domain.Orders;

/// <summary>
/// Progress of a reception, both in product lines ("3 / 5 lines")
/// and in units, i.e. the sum of expected quantities ("60 / 110 items").
/// </summary>
public sealed record ReceptionProgress(int ReceivedLines, int TotalLines, int ReceivedUnits, int TotalUnits)
{
    public static ReceptionProgress From(IEnumerable<ProductLine> products)
    {
        int receivedLines = 0, totalLines = 0, receivedUnits = 0, totalUnits = 0;

        foreach (var product in products)
        {
            totalLines++;
            totalUnits += product.ExpectedQuantity;

            if (product.IsReceived)
            {
                receivedLines++;
                receivedUnits += product.ExpectedQuantity;
            }
        }

        return new ReceptionProgress(receivedLines, totalLines, receivedUnits, totalUnits);
    }

    /// <summary>
    /// The status is derived from the product lines only: this is what keeps
    /// cartons, pallets and the order consistent with their children.
    /// </summary>
    public ReceptionStatus Status =>
        ReceivedLines == 0 ? ReceptionStatus.Pending :
        ReceivedLines == TotalLines ? ReceptionStatus.Received :
        ReceptionStatus.PartiallyReceived;
}