using System.Globalization;
using System.Text;
using Sceny.Finance.IO;

namespace Sceny.Finance.IO.Plugin.File.Csv;

/// <summary>
/// Configuration options for CSV source reader.
/// </summary>
public sealed class CsvOptions
{
    /// <summary>Column name mapping (default: common CSV column names)</summary>
    public Dictionary<string, string> ColumnMapping
    {
        get;
        set => field = value ?? throw new ArgumentNullException(nameof(value));
    } = new(StringComparer.OrdinalIgnoreCase)
    {
        ["AccountId"] = "AccountId",
        ["AccountName"] = "AccountName",
        ["Date"] = "Date",
        ["Amount"] = "Amount",
        ["Description"] = "Description",
        ["Type"] = "Type",
        ["Reference"] = "Reference"
    };

    /// <summary>Date format pattern (default: "yyyy-MM-dd")</summary>
    public string DateFormat
    {
        get;
        set
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(value, nameof(value), "Date format cannot be empty.");
            field = value;
        }
    } = "yyyy-MM-dd";

    /// <summary>CSV delimiter character (default: ',')</summary>
    public char Delimiter { get; set; } = ',';

    /// <summary>Text encoding (default: UTF-8)</summary>
    public Encoding Encoding
    {
        get;
        set => field = value ?? Encoding.UTF8;
    } = Encoding.UTF8;

    /// <summary>Whether the first row contains headers (default: true)</summary>
    public bool HasHeaders { get; set; } = true;

    /// <summary>Account type to assign to parsed accounts (default: Other)</summary>
    public AccountType DefaultAccountType { get; set; } = AccountType.Other;

    /// <summary>Currency code to use if not found in CSV (default: "USD")</summary>
    public string DefaultCurrency
    {
        get;
        set
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(value, nameof(value), "Currency cannot be empty.");
            field = value.ToUpperInvariant();
        }
    } = "USD";
}

