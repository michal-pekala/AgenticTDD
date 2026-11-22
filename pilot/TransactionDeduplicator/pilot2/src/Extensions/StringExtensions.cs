using System.Globalization;

namespace TransactionDeduplication.Extensions;

internal static class StringExtensions
{
    public static string Format(this string template, params object?[] args)
        => string.Format(CultureInfo.InvariantCulture, template, args);
}
