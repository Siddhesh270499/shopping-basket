using ShoppingBasket.Core;

// Usage:
//   dotnet run --project src/ShoppingBasket.Cli -- Apple Apple Banana
//   dotnet run --project src/ShoppingBasket.Cli -- "Apple, Apple, Banana"
// With no arguments, item names are read from standard input (one per line or comma-separated).

var items = (args.Length > 0 ? args : ReadStdin())
    .SelectMany(a => a.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
    .ToList();

var calculator = new BasketCalculator(PriceCatalogue.CreateDefault());

try
{
    var receipt = calculator.Price(items);

    foreach (var line in receipt.Lines)
    {
        var offer = line.Offer is null ? string.Empty : $"  ({line.Offer}, saved {Money.Format(line.SavingPence)})";
        Console.WriteLine($"{line.Quantity} x {line.Product,-8} {Money.Format(line.CostPence),8}{offer}");
    }

    Console.WriteLine(new string('-', 30));
    Console.WriteLine($"Total: {Money.Format(receipt.TotalPence)}");
    return 0;
}
catch (ArgumentException ex)
{
    Console.Error.WriteLine($"Error: {ex.Message}");
    return 1;
}

static IEnumerable<string> ReadStdin()
{
    if (!Console.IsInputRedirected)
    {
        Console.WriteLine("Enter items (comma-separated or one per line), then Ctrl+Z / Ctrl+D:");
    }

    string? line;
    while ((line = Console.ReadLine()) is not null)
    {
        yield return line;
    }
}
