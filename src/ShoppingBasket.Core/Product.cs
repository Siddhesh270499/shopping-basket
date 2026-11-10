namespace ShoppingBasket.Core;

/// <summary>A product that can be bought, with its normal price per unit in pence.</summary>
public sealed record Product
{
    public Product(string name, int unitPricePence)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Product name is required.", nameof(name));
        if (unitPricePence < 0)
            throw new ArgumentOutOfRangeException(nameof(unitPricePence), "Price cannot be negative.");

        Name = name.Trim();
        UnitPricePence = unitPricePence;
    }

    public string Name { get; }

    public int UnitPricePence { get; }
}
