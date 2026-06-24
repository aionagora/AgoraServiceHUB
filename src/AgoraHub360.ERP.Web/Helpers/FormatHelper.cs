using System.Globalization;

namespace AgoraHub360.ERP.Web.Helpers;

public static class FormatHelper
{
    private static readonly CultureInfo Culture = BuildCulture();

    private static CultureInfo BuildCulture()
    {
        var culture = new CultureInfo("en-US");
        var numberFormat = (NumberFormatInfo)culture.NumberFormat.Clone();
        numberFormat.NumberDecimalSeparator = ".";
        numberFormat.NumberGroupSeparator = ",";
        numberFormat.CurrencyDecimalSeparator = ".";
        numberFormat.CurrencyGroupSeparator = ",";
        culture.NumberFormat = numberFormat;
        return culture;
    }

    public static string Money(decimal value) => value.ToString("N2", Culture);

    public static string Quantity(decimal value) => value.ToString("N4", Culture);

    public static string Percent(decimal value) => value.ToString("N2", Culture);
}
