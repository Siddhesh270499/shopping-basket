using ShoppingBasket.Core;

namespace ShoppingBasket.Tests;

public class OfferAndCatalogueTests
{
    [Theory]
    [InlineData(0, 0)]
    [InlineData(1, 10)]
    [InlineData(2, 10)]
    [InlineData(9, 50)]
    public void Buy_one_get_one_free_charges_half_rounded_up(int quantity, int expectedPence)
    {
        Assert.Equal(expectedPence, MultiBuyOffer.BuyOneGetOneFree().PriceFor(quantity, 10));
    }

    [Theory]
    [InlineData(0, 0)]
    [InlineData(3, 20)]
    [InlineData(8, 60)] // two groups pay 4 units + 2 left over
    public void Three_for_two_charges_two_of_every_three(int quantity, int expectedPence)
    {
        Assert.Equal(expectedPence, MultiBuyOffer.ThreeForTwo().PriceFor(quantity, 10));
    }

    [Fact]
    public void No_offer_charges_every_unit()
    {
        Assert.Equal(70, NoOffer.Instance.PriceFor(7, 10));
    }

    [Fact]
    public void Negative_quantity_is_rejected()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => MultiBuyOffer.ThreeForTwo().PriceFor(-1, 10));
        Assert.Throws<ArgumentOutOfRangeException>(() => NoOffer.Instance.PriceFor(-1, 10));
    }

    [Theory]
    [InlineData(2, 2)] // nothing free
    [InlineData(1, 2)] // pay for more than you buy
    [InlineData(2, 0)] // everything free
    public void Invalid_multibuy_offers_are_rejected(int buy, int payFor)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new MultiBuyOffer(buy, payFor, "bad"));
    }

    [Fact]
    public void Product_rejects_blank_name_and_negative_price()
    {
        Assert.Throws<ArgumentException>(() => new Product(" ", 10));
        Assert.Throws<ArgumentOutOfRangeException>(() => new Product("Kiwi", -1));
    }

    [Fact]
    public void Catalogue_rejects_the_same_product_twice_ignoring_case()
    {
        var catalogue = new PriceCatalogue().Add(new Product("Apple", 35));

        Assert.Throws<ArgumentException>(() => catalogue.Add(new Product("APPLE", 30)));
    }

    [Fact]
    public void Default_catalogue_has_the_four_products_from_the_task()
    {
        var names = PriceCatalogue.CreateDefault().Entries.Select(e => e.Product.Name).OrderBy(n => n);

        Assert.Equal(["Apple", "Banana", "Lime", "Melon"], names);
    }

    [Theory]
    [InlineData(0, "£0.00")]
    [InlineData(5, "£0.05")]
    [InlineData(90, "£0.90")]
    [InlineData(135, "£1.35")]
    [InlineData(10_005, "£100.05")]
    public void Money_is_shown_in_pounds_and_pence(int pence, string expected)
    {
        Assert.Equal(expected, Money.Format(pence));
    }
}
