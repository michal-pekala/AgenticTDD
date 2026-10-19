using System.Globalization;

namespace TransactionDeduplicator.Pilot;

public static class StringExtensions
{
    public static string Format(this string template, params object?[] arguments)
        => string.Format(CultureInfo.InvariantCulture, template, arguments);
}
