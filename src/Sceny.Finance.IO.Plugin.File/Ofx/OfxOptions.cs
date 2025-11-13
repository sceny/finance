using System.Text;
using Sceny.Finance.IO;

namespace Sceny.Finance.IO.Plugin.File.Ofx;

/// <summary>
/// Configuration options for OFX source reader.
/// </summary>
public sealed class OfxOptions
{
    /// <summary>Date format pattern for parsing OFX dates (default: "yyyyMMdd")</summary>
    public string DateFormat
    {
        get;
        set
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(value, nameof(value), "Date format cannot be empty.");
            field = value;
        }
    } = "yyyyMMdd";

    /// <summary>DateTime format for OFX timestamps (default: "yyyyMMddHHmmss")</summary>
    public string DateTimeFormat
    {
        get;
        set
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(value, nameof(value), "Date-time format cannot be empty.");
            field = value;
        }
    } = "yyyyMMddHHmmss";

    /// <summary>Time zone offset for date parsing (default: null, use local time)</summary>
    public TimeSpan? TimeZoneOffset { get; set; }

    /// <summary>Text encoding (default: UTF-8)</summary>
    public Encoding Encoding
    {
        get;
        set => field = value ?? Encoding.UTF8;
    } = Encoding.UTF8;
}
