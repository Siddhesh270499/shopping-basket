namespace ShoppingBasket.Core;

/// <summary>The products the shop sells and the offer that applies to each. Names are matched ignoring case.</summary>
public sealed class PriceCatalogue
{
    private readonly Dictionary<string, CatalogueEntry> _entries = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>The prices from the task: Apple 35p, Banana 20p, Melon 50p (BOGOF), Lime 15p (3 for 2).</summary>
    public static PriceCatalogue CreateDefault() =>
        new PriceCatalogue()
            .Add(new Product("Apple", 35))
            .Add(new Product("Banana", 20))
            .Add(new Product("Melon", 50), MultiBuyOffer.BuyOneGetOneFree())
            .Add(new Product("Lime", 15), MultiBuyOffer.ThreeForTwo());

    public IReadOnlyCollection<CatalogueEntry> Entries => _entries.Values;

    public PriceCatalogue Add(Product product, IOffer? offer = null)
    {
        ArgumentNullException.ThrowIfNull(product);

        if (!_entries.TryAdd(product.Name, new CatalogueEntry(product, offer ?? NoOffer.Instance)))
            throw new ArgumentException($"'{product.Name}' is already in the catalogue.", nameof(product));

        return this;
    }

    public bool TryGet(string name, out CatalogueEntry entry) =>
        _entries.TryGetValue(name.Trim(), out entry!);
}

public sealed record CatalogueEntry(Product Product, IOffer Offer);
