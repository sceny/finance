using System.Text;

namespace Sceny.Finance.IO.Plugin.File.Ofx;

/// <summary>
/// Builder for creating OfxSourceReader with fluent configuration.
/// </summary>
public sealed class OfxSourceReaderBuilder
{
    private readonly OfxOptions _options = new();

    /// <summary>
    /// Sets the date format pattern for OFX dates.
    /// </summary>
    public OfxSourceReaderBuilder WithDateFormat(string dateFormat)
    {
        _options.DateFormat = dateFormat;
        return this;
    }

    /// <summary>
    /// Sets the DateTime format for OFX timestamps.
    /// </summary>
    public OfxSourceReaderBuilder WithDateTimeFormat(string dateTimeFormat)
    {
        _options.DateTimeFormat = dateTimeFormat;
        return this;
    }

    /// <summary>
    /// Sets the time zone offset for date parsing.
    /// </summary>
    public OfxSourceReaderBuilder WithTimeZoneOffset(TimeSpan? offset)
    {
        _options.TimeZoneOffset = offset;
        return this;
    }

    /// <summary>
    /// Sets the text encoding.
    /// </summary>
    public OfxSourceReaderBuilder WithEncoding(Encoding encoding)
    {
        _options.Encoding = encoding;
        return this;
    }

    /// <summary>
    /// Builds the OfxSourceReader with the configured options.
    /// </summary>
    public OfxSourceReader Build()
    {
        return new OfxSourceReader(_options);
    }
}

