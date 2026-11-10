using System.Globalization;

namespace ShoppingBasket.Core;

/// <summary>Prices are kept as whole pence (int) so there are no rounding errors; this turns them into £0.00 text.</summary>
public static class Money
{
    public static string Format(int pence) =>
        (pence < 0 ? "-" : string.Empty) + "£" + (Math.Abs(pence) / 100m).ToString("0.00", CultureInfo.InvariantCulture);
}
