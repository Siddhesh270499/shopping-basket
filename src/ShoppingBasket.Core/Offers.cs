namespace ShoppingBasket.Core;

/// <summary>How a quantity of one product is charged.</summary>
public interface IOffer
{
    string Description { get; }

    /// <summary>Price in pence for <paramref name="quantity"/> units at <paramref name="unitPricePence"/> each.</summary>
    int PriceFor(int quantity, int unitPricePence);
}

/// <summary>No offer: every unit is charged at the normal price.</summary>
public sealed class NoOffer : IOffer
{
    public static readonly NoOffer Instance = new();

    private NoOffer()
    {
    }

    public string Description => "No offer";

    public int PriceFor(int quantity, int unitPricePence)
    {
        Guard.NotNegative(quantity, nameof(quantity));
        return checked(quantity * unitPricePence);
    }
}

/// <summary>
/// "Buy <see cref="Buy"/>, pay for <see cref="PayFor"/>".
/// Buy one get one free is Buy 2 / PayFor 1; three for the price of two is Buy 3 / PayFor 2.
/// Items left over after the full groups are charged at the normal price.
/// </summary>
public sealed class MultiBuyOffer : IOffer
{
    public MultiBuyOffer(int buy, int payFor, string description)
    {
        if (payFor < 1)
            throw new ArgumentOutOfRangeException(nameof(payFor), "PayFor must be at least 1.");
        if (buy <= payFor)
            throw new ArgumentOutOfRangeException(nameof(buy), "Buy must be greater than PayFor.");

        Buy = buy;
        PayFor = payFor;
        Description = description;
    }

    public static MultiBuyOffer BuyOneGetOneFree() => new(2, 1, "Buy one get one free");

    public static MultiBuyOffer ThreeForTwo() => new(3, 2, "Three for the price of two");

    public int Buy { get; }

    public int PayFor { get; }

    public string Description { get; }

    public int PriceFor(int quantity, int unitPricePence)
    {
        Guard.NotNegative(quantity, nameof(quantity));

        var fullGroups = quantity / Buy;
        var leftOver = quantity % Buy;
        var chargedUnits = (fullGroups * PayFor) + leftOver;
        return checked(chargedUnits * unitPricePence);
    }
}

internal static class Guard
{
    public static void NotNegative(int value, string name)
    {
        if (value < 0)
            throw new ArgumentOutOfRangeException(name, "Quantity cannot be negative.");
    }
}
