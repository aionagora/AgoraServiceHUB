using System.Globalization;

namespace AgoraHub360.ERP.Shared.Configuration;

public static class AppFormattingOptions
{
    private const string NumericCultureName = "en-US";
    private const string UiCultureName = "es-ES";

    public static CultureInfo BuildNumericCulture()
    {
        var culture = new CultureInfo(NumericCultureName);
        var numberFormat = (NumberFormatInfo)culture.NumberFormat.Clone();
        numberFormat.NumberDecimalSeparator = ".";
        numberFormat.NumberGroupSeparator = ",";
        numberFormat.CurrencyDecimalSeparator = ".";
        numberFormat.CurrencyGroupSeparator = ",";
        culture.NumberFormat = numberFormat;
        return culture;
    }

    public static CultureInfo BuildUiCulture()
    {
        return new CultureInfo(UiCultureName);
    }

    public static string FormatMoney(decimal value, CultureInfo? culture = null)
        => value.ToString("N2", culture ?? BuildNumericCulture());

    public static string FormatQuantity(decimal value, CultureInfo? culture = null)
        => value.ToString("N4", culture ?? BuildNumericCulture());

    public static string FormatPercent(decimal value, CultureInfo? culture = null)
        => value.ToString("N2", culture ?? BuildNumericCulture());
}
