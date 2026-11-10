using ShoppingBasket.Core;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton(PriceCatalogue.CreateDefault());
builder.Services.AddSingleton<BasketCalculator>();

var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();

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
