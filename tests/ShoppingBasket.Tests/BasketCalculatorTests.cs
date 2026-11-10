using ShoppingBasket.Core;

namespace ShoppingBasket.Tests;

public class BasketCalculatorTests
{
    private readonly BasketCalculator _calculator = new(PriceCatalogue.CreateDefault());

    private int Total(params string[] items) => _calculator.TotalPence(items);

    [Fact]
    public void Empty_basket_costs_nothing()
    {
        Assert.Equal(0, Total());
    }

    [Theory]
    [InlineData("Apple", 35)]
    [InlineData("Banana", 20)]
    [InlineData("Melon", 50)]
    [InlineData("Lime", 15)]
    public void Single_item_costs_its_unit_price(string item, int expectedPence)
    {
        Assert.Equal(expectedPence, Total(item));
    }

    [Fact]
    public void Example_from_the_task_two_apples_and_a_banana()
    {
        Assert.Equal(90, Total("Apple", "Apple", "Banana"));
    }

    [Theory]
    [InlineData(2, 70)]
    [InlineData(5, 175)]
    public void Apples_have_no_offer(int quantity, int expectedPence)
    {
        Assert.Equal(expectedPence, Total(Enumerable.Repeat("Apple", quantity).ToArray()));
    }

    [Theory]
    [InlineData(3, 60)]
    public void Bananas_have_no_offer(int quantity, int expectedPence)
    {
        Assert.Equal(expectedPence, Total(Enumerable.Repeat("Banana", quantity).ToArray()));
    }

    [Theory]
    [InlineData(1, 50)]   // no pair yet, pay for one
    [InlineData(2, 50)]   // buy one get one free
    [InlineData(3, 100)]  // one free pair + one at full price
    [InlineData(4, 100)]  // two free pairs
    [InlineData(5, 150)]
    public void Melons_are_buy_one_get_one_free(int quantity, int expectedPence)
    {
        Assert.Equal(expectedPence, Total(Enumerable.Repeat("Melon", quantity).ToArray()));
    }

    [Theory]
    [InlineData(1, 15)]
    [InlineData(2, 30)]   // offer needs three
    [InlineData(3, 30)]   // three for the price of two
    [InlineData(4, 45)]
    [InlineData(5, 60)]
    [InlineData(6, 60)]   // two full groups
    [InlineData(7, 75)]
    public void Limes_are_three_for_the_price_of_two(int quantity, int expectedPence)
    {
        Assert.Equal(expectedPence, Total(Enumerable.Repeat("Lime", quantity).ToArray()));
    }

    [Fact]
    public void Mixed_basket_applies_each_offer_to_its_own_item()
    {
        // Apple 35 + Banana 20 + 2 Melons 50 (BOGOF) + 3 Limes 30 (3 for 2) = 135
        Assert.Equal(135, Total("Apple", "Banana", "Melon", "Melon", "Lime", "Lime", "Lime"));
    }

    [Fact]
    public void Offers_apply_even_when_items_are_not_next_to_each_other()
    {
        Assert.Equal(85, Total("Melon", "Apple", "Melon"));                  // 50 + 35
        Assert.Equal(85, Total("Lime", "Banana", "Lime", "Apple", "Lime"));  // 30 + 20 + 35
    }

    [Fact]
    public void Order_of_items_does_not_change_the_total()
    {
        string[] basket = ["Lime", "Apple", "Melon", "Lime", "Banana", "Melon", "Lime", "Apple"];
        var reversed = basket.Reverse().ToArray();

        Assert.Equal(Total(basket), Total(reversed));
    }

    [Fact]
    public void Item_names_ignore_case_and_surrounding_spaces()
    {
        Assert.Equal(70, Total("apple", " APPLE "));
        Assert.Equal(50, Total("melon", "Melon"));
    }

    [Fact]
    public void Large_basket_is_priced_correctly()
    {
        // 1000 limes: 333 full groups (pay for 666) + 1 left over = 667 x 15p
        Assert.Equal(10_005, Total(Enumerable.Repeat("Lime", 1000).ToArray()));
    }

    [Fact]
    public void Unknown_item_is_rejected_with_its_name()
    {
        var error = Assert.Throws<UnknownItemException>(() => Total("Apple", "Mango"));

        Assert.Equal("Mango", error.ItemName);
    }

    [Fact]
    public void Null_basket_is_rejected()
    {
        Assert.Throws<ArgumentNullException>(() => _calculator.TotalPence(null!));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Blank_item_name_is_rejected(string? item)
    {
        Assert.Throws<ArgumentException>(() => _calculator.TotalPence(["Apple", item!]));
    }

    [Fact]
    public void Receipt_shows_quantity_cost_and_saving_per_item()
    {
        var receipt = _calculator.Price(["Melon", "Melon", "Melon", "Apple", "Lime", "Lime", "Lime"]);

        var melon = Assert.Single(receipt.Lines, l => l.Product == "Melon");
        Assert.Equal(3, melon.Quantity);
        Assert.Equal(100, melon.CostPence);
        Assert.Equal(50, melon.SavingPence);
        Assert.Equal("Buy one get one free", melon.Offer);

        var apple = Assert.Single(receipt.Lines, l => l.Product == "Apple");
        Assert.Null(apple.Offer);
        Assert.Equal(0, apple.SavingPence);

        Assert.Equal(165, receipt.TotalPence);  // 100 + 35 + 30
        Assert.Equal(65, receipt.TotalSavingPence); // 50 (melon) + 15 (lime)
    }

    [Fact]
    public void Catalogue_can_be_extended_without_changing_the_calculator()
    {
        var catalogue = PriceCatalogue.CreateDefault()
            .Add(new Product("Orange", 40), new MultiBuyOffer(buy: 4, payFor: 3, "Four for three"));
        var calculator = new BasketCalculator(catalogue);

        Assert.Equal(160, calculator.TotalPence(Enumerable.Repeat("Orange", 5).ToArray())); // 4 for 120 + 40
    }
}
