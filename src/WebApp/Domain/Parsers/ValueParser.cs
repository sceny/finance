using System;
using System.Globalization;

namespace WebApp.Domain.Parsers;

public class ValueParser
{
    public ValueParser(ValueParserSettings settings) =>
        Settings = settings ?? throw new ArgumentNullException(nameof(settings));
    public ValueParserSettings Settings { get; }
    public bool TryParseDate(ReadOnlySpan<char> value, out DateTime date)
    {
        for (var i = 0; i < Settings.DateFormats.Length; i++)
        {
            if (TryParseDate(value, Settings.DateFormats[i], out date))
                return true;
        }
        date = default;
        return false;

        static bool TryParseDate(ReadOnlySpan<char> value, ReadOnlySpan<char> format, out DateTime date) =>
            DateTime.TryParseExact(
                value,
                format,
                null,
                System.Globalization.DateTimeStyles.None,
                out date);
    }

    public bool TryParseNumber(ReadOnlySpan<char> value, out double number) =>
        double.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out number);
}

public record ValueParserSettings(
    string[] DateFormats
);