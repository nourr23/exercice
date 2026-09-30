using ReceptionTracker.Domain.Common;
using ReceptionTracker.Domain.Orders;

namespace ReceptionTracker.Domain.Tests.Orders;

public class OrderReceptionTests
{
    // PAL-01 ─┬─ CART-01-A: TSH-RED-M (50), SHO-BLK-42 (10)
    //         └─ CART-01-B: SHO-BLK-43 (8)
    // PAL-02 ─── CART-02-A: BAL-FOOT-5 (20)
    private static Order CreateOrder()
    {
        var order = new Order("CMD-2026");

        var pallet1 = order.AddPallet("PAL-01");
        var cartonA = pallet1.AddCarton("CART-01-A");
        cartonA.AddProduct("TSH-RED-M", "T-Shirt Sport", "Rouge", "M", 50);
        cartonA.AddProduct("SHO-BLK-42", "Baskets Running", "Noir", "42", 10);
        var cartonB = pallet1.AddCarton("CART-01-B");
        cartonB.AddProduct("SHO-BLK-43", "Baskets Running", "Noir", "43", 8);

        var pallet2 = order.AddPallet("PAL-02");
        var cartonC = pallet2.AddCarton("CART-02-A");
        cartonC.AddProduct("BAL-FOOT-5", "Ballon de Football", "Blanc", "5", 20);

        return order;
    }

    private static Pallet Pallet(Order order, string code) => order.Pallets.Single(p => p.Code == code);

    private static Carton Carton(Order order, string code) =>
        order.Pallets.SelectMany(p => p.Cartons).Single(c => c.Code == code);

    [Fact]
    public void New_order_is_pending_everywhere()
    {
        var order = CreateOrder();

        Assert.Equal(ReceptionStatus.Pending, order.Status);
        Assert.All(order.Pallets, p => Assert.Equal(ReceptionStatus.Pending, p.Status));
        Assert.All(order.Products, p => Assert.False(p.IsReceived));
    }

    // Business rule: products checked one by one → carton received
    [Fact]
    public void Receiving_all_products_one_by_one_marks_the_carton_as_received()
    {
        var order = CreateOrder();
        var carton = Carton(order, "CART-01-A");

        carton.Products.First().SetReceived(true);
        Assert.Equal(ReceptionStatus.PartiallyReceived, carton.Status);

        carton.Products.Last().SetReceived(true);
        Assert.Equal(ReceptionStatus.Received, carton.Status);
    }

    // Business rule: one product unchecked → carton and pallet partially received
    [Fact]
    public void Unchecking_a_product_makes_carton_and_pallet_partially_received()
    {
        var order = CreateOrder();
        var pallet = Pallet(order, "PAL-01");
        var cartonA = Carton(order, "CART-01-A");
        var cartonB = Carton(order, "CART-01-B");
        pallet.SetReceived(true);

        cartonA.Products.First().SetReceived(false);

        Assert.Equal(ReceptionStatus.PartiallyReceived, cartonA.Status);
        Assert.Equal(ReceptionStatus.PartiallyReceived, pallet.Status);
        Assert.Equal(ReceptionStatus.Received, cartonB.Status);
    }

    // Carton-level validation
    [Fact]
    public void Receiving_a_carton_receives_all_its_products_only()
    {
        var order = CreateOrder();
        var pallet = Pallet(order, "PAL-01");
        var cartonA = Carton(order, "CART-01-A");
        var cartonB = Carton(order, "CART-01-B");

        cartonA.SetReceived(true);

        Assert.All(cartonA.Products, p => Assert.True(p.IsReceived));
        Assert.Equal(ReceptionStatus.Pending, cartonB.Status);
        Assert.Equal(ReceptionStatus.PartiallyReceived, pallet.Status);
    }

    // Pallet-level validation
    [Fact]
    public void Receiving_a_pallet_receives_all_its_cartons_and_products()
    {
        var order = CreateOrder();
        var pallet1 = Pallet(order, "PAL-01");
        var pallet2 = Pallet(order, "PAL-02");

        pallet1.SetReceived(true);

        Assert.All(pallet1.Cartons, c => Assert.Equal(ReceptionStatus.Received, c.Status));
        Assert.All(pallet1.Products, p => Assert.True(p.IsReceived));
        Assert.Equal(ReceptionStatus.Received, pallet1.Status);
        Assert.Equal(ReceptionStatus.Pending, pallet2.Status);
        Assert.Equal(ReceptionStatus.PartiallyReceived, order.Status);
    }

    [Fact]
    public void Unreceiving_a_pallet_resets_it_to_pending()
    {
        var order = CreateOrder();
        var pallet = Pallet(order, "PAL-01");
        pallet.SetReceived(true);

        pallet.SetReceived(false);

        Assert.Equal(ReceptionStatus.Pending, pallet.Status);
        Assert.All(pallet.Products, p => Assert.False(p.IsReceived));
    }

    [Fact]
    public void Order_is_received_when_every_pallet_is_received()
    {
        var order = CreateOrder();

        foreach (var pallet in order.Pallets)
        {
            pallet.SetReceived(true);
        }

        Assert.Equal(ReceptionStatus.Received, order.Status);
    }

    [Fact]
    public void Progress_counts_lines_and_units()
    {
        var order = CreateOrder();

        Carton(order, "CART-01-A").SetReceived(true);

        Assert.Equal(new ReceptionProgress(ReceivedLines: 2, TotalLines: 4, ReceivedUnits: 60, TotalUnits: 88),
            order.GetProgress());
    }

    [Fact]
    public void Find_methods_return_the_matching_element_or_null()
    {
        var order = CreateOrder();

        var pallet = order.FindPallet("PAL-01");

        Assert.NotNull(pallet);
        Assert.Equal("CART-01-B", pallet.FindCarton("CART-01-B")?.Code);
        Assert.Null(pallet.FindCarton("CART-02-A"));
        Assert.Null(order.FindPallet("PAL-99"));
    }

    [Fact]
    public void Adding_a_duplicate_pallet_code_throws()
    {
        var order = CreateOrder();

        Assert.Throws<DomainException>(() => order.AddPallet("PAL-01"));
    }

    [Fact]
    public void Adding_a_duplicate_carton_code_to_a_pallet_throws()
    {
        var order = CreateOrder();

        Assert.Throws<DomainException>(() => Pallet(order, "PAL-01").AddCarton("CART-01-A"));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public void Product_line_requires_a_positive_expected_quantity(int quantity)
    {
        var carton = new Carton("CART-X");

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            carton.AddProduct("REF", "Name", "Color", "Size", quantity));
    }
}