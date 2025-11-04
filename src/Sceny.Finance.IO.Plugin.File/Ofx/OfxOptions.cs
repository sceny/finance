using System.Text;

namespace Sceny.Finance.IO.Plugin.File.Ofx;

/// <summary>
/// Configuration options for OFX source reader.
/// </summary>
public sealed class OfxOptions
{
    /// <summary>Date format pattern for parsing OFX dates (default: "yyyyMMdd")</summary>
    public string DateFormat { get; set; } = "yyyyMMdd";

    /// <summary>DateTime format for OFX timestamps (default: "yyyyMMddHHmmss")</summary>
    public string DateTimeFormat { get; set; } = "yyyyMMddHHmmss";

    /// <summary>Time zone offset for date parsing (default: null, use local time)</summary>
    public TimeSpan? TimeZoneOffset { get; set; }

    /// <summary>Text encoding (default: UTF-8)</summary>
    public Encoding Encoding { get; set; } = Encoding.UTF8;
}

