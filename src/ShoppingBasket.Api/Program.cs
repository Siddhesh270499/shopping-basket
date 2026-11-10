using ShoppingBasket.Core;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton(PriceCatalogue.CreateDefault());
builder.Services.AddSingleton<BasketCalculator>();

var app = builder.Build();

// Serves the Angular app once it has been built into wwwroot (cd client && npm run build).
app.UseDefaultFiles();
app.UseStaticFiles();

// Shown only when the UI has not been built: the API itself still works.
var uiBuilt = app.Environment.WebRootPath is { } webRoot && File.Exists(Path.Combine(webRoot, "index.html"));
if (!uiBuilt)
{
    app.MapGet("/", () => Results.Text(
        "Shopping Basket API is running. Endpoints: GET /api/products, POST /api/basket/price. " +
        "For the UI, run the Angular app in client/ (npm start) or build it into wwwroot (npm run build)."));
}

// Products and their offers, for the UI.
app.MapGet("/api/products", (PriceCatalogue catalogue) =>
    catalogue.Entries
        .OrderBy(e => e.Product.Name)
        .Select(e => new
        {
            e.Product.Name,
            e.Product.UnitPricePence,
            Offer = e.Offer is NoOffer ? null : e.Offer.Description,
        }));

// Body: ["Apple", "Apple", "Banana"]  ->  receipt with lines, total and saving.
app.MapPost("/api/basket/price", (string[] items, BasketCalculator calculator) =>
{
    try
    {
        var receipt = calculator.Price(items);
        return Results.Ok(new
        {
            receipt.Lines,
            receipt.TotalPence,
            Total = Money.Format(receipt.TotalPence),
            receipt.TotalSavingPence,
            TotalSaving = Money.Format(receipt.TotalSavingPence),
        });
    }
    catch (ArgumentException ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

app.Run();
