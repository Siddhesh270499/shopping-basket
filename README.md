# Shopping Basket – price calculator (C#)

Calculates the total cost of a basket of shopping, given as a list of item names such as `["Apple", "Apple", "Banana"]`.

| Item | Price | Offer |
|---|---|---|
| Apple | 35p | none |
| Banana | 20p | none |
| Melon | 50p | Buy one get one free |
| Lime | 15p | Three for the price of two |

## How to run

Requires the .NET 8 SDK.

```bash
# Tests (53 cases)
dotnet test

# Command line
dotnet run --project src/ShoppingBasket.Cli -- Apple Apple Banana
dotnet run --project src/ShoppingBasket.Cli -- "Apple, Melon, Melon, Lime, Lime, Lime"

# Small web API + page (open http://localhost:5000)
dotnet run --project src/ShoppingBasket.Api --urls http://localhost:5000
```

Example output:

```
2 x Apple       £0.70
1 x Banana      £0.20
3 x Lime        £0.30  (Three for the price of two, saved £0.15)
3 x Melon       £1.00  (Buy one get one free, saved £0.50)
------------------------------
Total: £2.20
```

## Design

```
src/
  ShoppingBasket.Core     the pricing logic (no UI, no I/O) – everything below is tested here
    Product               name + unit price in pence
    IOffer                how N units of one product are charged
      NoOffer             N x price
      MultiBuyOffer       "buy X, pay for Y": BOGOF = (2,1), 3-for-2 = (3,2)
    PriceCatalogue        products + their offer, looked up ignoring case
    BasketCalculator      counts items, asks each product's offer for its cost, returns a receipt
    BasketReceipt         one line per product (quantity, cost, saving) + totals
    Money                 formats pence as £0.00
  ShoppingBasket.Cli      console front end
  ShoppingBasket.Api      ASP.NET Core minimal API + one static HTML page
tests/
  ShoppingBasket.Tests    xUnit tests
```

How a basket is priced:

1. Validate the input (list not null, no blank names, every name known).
2. Count how many of each product there are. Offers depend on quantity, not on where items appear in the list, so `["Melon", "Apple", "Melon"]` still gets the melon offer.
3. For each product, ask its offer for the cost of that quantity.
4. Add the line costs together.

Key decisions:

- **Money is whole pence (`int`)**, never `double`, so there are no rounding errors. It is formatted as pounds only for display.
- **Offers are separate classes behind `IOffer`.** Both offers in the task are the same rule ("buy X, pay for Y"), so one `MultiBuyOffer` class covers them. A new kind of offer (for example "2 for £1") is a new `IOffer` class; the calculator does not change (open/closed principle). A test adds an "Orange, four for three" product to show this.
- **Prices live in a catalogue, not in the calculator**, so they can come from config or a database later.
- **Unknown items are rejected** with an `UnknownItemException` that names the item, rather than being silently priced at 0. An empty basket costs £0.00.
- **Names are matched ignoring case and surrounding spaces** (`"apple"`, `" APPLE "`).
- **Checked arithmetic** turns an overflow on huge baskets into an error instead of a wrong total.

Assumptions:

- An offer only applies to identical items (two melons, not a melon and a lime).
- Items left over after a full offer group are charged at the normal price (3 melons = 2 x 50p).
- The input is the list of names only; quantities are expressed by repeating the name, as in the task.

## Test cases

| # | Scenario | Input | Expected |
|---|---|---|---|
| 1 | Empty basket | `[]` | 0p |
| 2 | One of each product | `Apple` / `Banana` / `Melon` / `Lime` | 35p / 20p / 50p / 15p |
| 3 | Example from the task | `Apple, Apple, Banana` | 90p |
| 4 | Apples have no offer | 2 / 5 apples | 70p / 175p |
| 5 | Bananas have no offer | 3 bananas | 60p |
| 6 | Melon BOGOF | 1 / 2 / 3 / 4 / 5 melons | 50 / 50 / 100 / 100 / 150p |
| 7 | Lime 3-for-2 | 1 / 2 / 3 / 4 / 5 / 6 / 7 limes | 15 / 30 / 30 / 45 / 60 / 60 / 75p |
| 8 | Mixed basket | Apple, Banana, 2 Melons, 3 Limes | 135p |
| 9 | Offer items not next to each other | `Melon, Apple, Melon` / `Lime, Banana, Lime, Apple, Lime` | 85p / 85p |
| 10 | Order does not matter | a basket and the same basket reversed | same total |
| 11 | Case and spaces ignored | `apple, " APPLE "` | 70p |
| 12 | Large basket | 1000 limes | 10,005p |
| 13 | Unknown item | `Apple, Mango` | `UnknownItemException` naming "Mango" |
| 14 | Null basket | `null` | `ArgumentNullException` |
| 15 | Blank item | `""`, `"   "`, `null` inside the list | `ArgumentException` |
| 16 | Receipt detail | 3 Melons, Apple, 3 Limes | per-line quantity, cost and saving; total 165p, saving 65p |
| 17 | New product added via the catalogue | 5 Oranges at 40p, four for three | 160p |
| 18 | Offer maths directly | BOGOF 0 / 1 / 2 / 9 units; 3-for-2 0 / 3 / 8 units | 0 / 10 / 10 / 50; 0 / 20 / 60 (at 10p) |
| 19 | Invalid input to offers | negative quantity; offers like (2,2), (1,2), (2,0) | rejected |
| 20 | Invalid products | blank name, negative price, duplicate name | rejected |
| 21 | Money formatting | 0, 5, 90, 135, 10005 pence | £0.00, £0.05, £0.90, £1.35, £100.05 |

## Possible next steps

- Load prices and offers from configuration or a database.
- Offers across different products (for example "meal deal").
- Return a receipt that lists each offer applied as its own line.
- Persist baskets behind the API, and an Angular or React front end in place of the plain HTML page.
