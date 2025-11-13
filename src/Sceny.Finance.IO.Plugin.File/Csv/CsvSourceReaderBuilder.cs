using System.Text;

namespace Sceny.Finance.IO.Plugin.File.Csv;

/// <summary>
/// Builder for creating CsvSourceReader with fluent configuration.
/// </summary>
public sealed class CsvSourceReaderBuilder
{
    private readonly CsvOptions _options = new();

    /// <summary>
    /// Sets the date format pattern.
    /// </summary>
    public CsvSourceReaderBuilder WithDateFormat(string dateFormat)
    {
        ArgumentNullException.ThrowIfNull(dateFormat);
        _options.DateFormat = dateFormat;
        return this;
    }

    /// <summary>
    /// Sets the CSV delimiter character.
    /// </summary>
    public CsvSourceReaderBuilder WithDelimiter(char delimiter)
    {
        _options.Delimiter = delimiter;
        return this;
    }

    /// <summary>
    /// Sets the text encoding.
    /// </summary>
    public CsvSourceReaderBuilder WithEncoding(Encoding encoding)
    {
        ArgumentNullException.ThrowIfNull(encoding);
        _options.Encoding = encoding;
        return this;
    }

    /// <summary>
    /// Sets whether the CSV has headers.
    /// </summary>
    public CsvSourceReaderBuilder WithHeaders(bool hasHeaders)
    {
        _options.HasHeaders = hasHeaders;
        return this;
    }

    /// <summary>
    /// Maps a column name.
    /// </summary>
    public CsvSourceReaderBuilder MapColumn(string targetColumn, string csvColumnName)
    {
        ArgumentNullException.ThrowIfNull(targetColumn);
        ArgumentNullException.ThrowIfNull(csvColumnName);
        _options.ColumnMapping[targetColumn] = csvColumnName;
        return this;
    }

    /// <summary>
    /// Sets the default account type.
    /// </summary>
    public CsvSourceReaderBuilder WithDefaultAccountType(AccountType accountType)
    {
        _options.DefaultAccountType = accountType;
        return this;
    }

    /// <summary>
    /// Sets the default currency.
    /// </summary>
    public CsvSourceReaderBuilder WithDefaultCurrency(string currency)
    {
        _options.DefaultCurrency = currency;
        return this;
    }

    /// <summary>
    /// Builds the CsvSourceReader with the configured options.
    /// </summary>
    public CsvSourceReader Build()
    {
        return new CsvSourceReader(_options);
    }
}

