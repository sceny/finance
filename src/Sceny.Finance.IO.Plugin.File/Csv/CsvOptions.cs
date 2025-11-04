using System.Text;

namespace Sceny.Finance.IO.Plugin.File.Csv;

/// <summary>
/// Configuration options for CSV source reader.
/// </summary>
public sealed class CsvOptions
{
    /// <summary>Column name mapping (default: common CSV column names)</summary>
    public Dictionary<string, string> ColumnMapping { get; set; } = new()
    {
        { "AccountId", "AccountId" },
        { "AccountName", "AccountName" },
        { "Date", "Date" },
        { "Amount", "Amount" },
        { "Description", "Description" },
        { "Type", "Type" },
        { "Reference", "Reference" }
    };

    /// <summary>Date format pattern (default: "yyyy-MM-dd")</summary>
    public string DateFormat { get; set; } = "yyyy-MM-dd";

    /// <summary>CSV delimiter character (default: ',')</summary>
    public char Delimiter { get; set; } = ',';

    /// <summary>Text encoding (default: UTF-8)</summary>
    public Encoding Encoding { get; set; } = Encoding.UTF8;

    /// <summary>Whether the first row contains headers (default: true)</summary>
    public bool HasHeaders { get; set; } = true;

    /// <summary>Account type to assign to parsed accounts (default: Other)</summary>
    public AccountType DefaultAccountType { get; set; } = AccountType.Other;

    /// <summary>Currency code to use if not found in CSV (default: "USD")</summary>
    public string DefaultCurrency { get; set; } = "USD";
}

