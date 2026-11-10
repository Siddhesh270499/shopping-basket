namespace ShoppingBasket.Core;

/// <summary>Works out the cost of a basket, given as a list of item names such as ["Apple", "Apple", "Banana"].</summary>
public sealed class BasketCalculator
{
    private readonly PriceCatalogue _catalogue;

    public BasketCalculator(PriceCatalogue catalogue)
    {
        _catalogue = catalogue ?? throw new ArgumentNullException(nameof(catalogue));
    }

    /// <summary>Total cost in pence, after offers.</summary>
    public int TotalPence(IEnumerable<string> items) => Price(items).TotalPence;

    /// <summary>Full breakdown: one line per product with quantity, cost and saving.</summary>
    /// <exception cref="ArgumentNullException">The list is null.</exception>
    /// <exception cref="ArgumentException">An item name is null or blank.</exception>
    /// <exception cref="UnknownItemException">An item is not in the catalogue.</exception>
    public BasketReceipt Price(IEnumerable<string> items)
    {
        ArgumentNullException.ThrowIfNull(items);

        // Count each product. Offers depend on how many of an item there are, not on their order in the list.
        var counts = new Dictionary<string, (CatalogueEntry Entry, int Quantity)>(StringComparer.OrdinalIgnoreCase);
        foreach (var item in items)
        {
            if (string.IsNullOrWhiteSpace(item))
                throw new ArgumentException("The basket contains an empty item name.", nameof(items));

            if (!_catalogue.TryGet(item, out var entry))
                throw new UnknownItemException(item.Trim());

            var name = entry.Product.Name;
            counts[name] = counts.TryGetValue(name, out var current)
                ? (entry, current.Quantity + 1)
                : (entry, 1);
        }

        var lines = counts.Values
            .Select(c => new ReceiptLine(
                c.Entry.Product.Name,
                c.Quantity,
                c.Entry.Product.UnitPricePence,
                c.Entry.Offer.PriceFor(c.Quantity, c.Entry.Product.UnitPricePence),
                c.Entry.Offer is NoOffer ? null : c.Entry.Offer.Description))
            .OrderBy(l => l.Product, StringComparer.OrdinalIgnoreCase)
            .ToList();

        return new BasketReceipt(lines);
    }
}

public sealed record ReceiptLine(string Product, int Quantity, int UnitPricePence, int CostPence, string? Offer)
{
    public int FullPricePence => Quantity * UnitPricePence;

    public int SavingPence => FullPricePence - CostPence;
}

public sealed record BasketReceipt(IReadOnlyList<ReceiptLine> Lines)
{
    public int TotalPence => Lines.Sum(l => l.CostPence);

    public int TotalSavingPence => Lines.Sum(l => l.SavingPence);
}

public sealed class UnknownItemException : ArgumentException
{
    public UnknownItemException(string itemName)
        : base($"'{itemName}' is not a known item.")
    {
        ItemName = itemName;
    }

    public string ItemName { get; }
}
