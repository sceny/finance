using System.Buffers;
using System.Globalization;
using System.IO.Pipelines;
using System.Text;
using Sceny.Finance.IO;

namespace Sceny.Finance.IO.Plugin.File.Csv;

/// <summary>
/// CSV source reader implementation.
/// Parses CSV files using System.IO.Pipelines for zero-allocation streaming.
/// </summary>
public sealed class CsvSourceReader : ISourceReader
{
    private readonly CsvOptions _options;

    public CsvSourceReader(CsvOptions? options = null)
    {
        _options = options ?? new CsvOptions();
    }

    public async IAsyncEnumerable<Account> GetAccountsAsync(
        PipeReader reader,
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var sequence = await Utilities.ReadCompleteSequenceAsync(reader, cancellationToken).ConfigureAwait(false);
        var text = Utilities.SequenceToString(sequence, _options.Encoding);
        var lines = SplitLines(text);

        if (lines.Count == 0)
            yield break;

        var headerLine = lines[0];
        var columnIndices = ParseHeader(headerLine, text);

        // Parse accounts from first data row
        if (lines.Count > 1 && _options.HasHeaders)
        {
            var firstDataLine = lines[1];
            var account = ParseAccountFromLine(firstDataLine, columnIndices, text);
            if (account.HasValue)
                yield return account.Value;
        }

        Utilities.AdvanceReader(reader, sequence);
    }

    public async IAsyncEnumerable<Transaction> GetTransactionsAsync(
        Account account,
        PipeReader reader,
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var sequence = await Utilities.ReadCompleteSequenceAsync(reader, cancellationToken).ConfigureAwait(false);
        var text = Utilities.SequenceToString(sequence, _options.Encoding);
        var lines = SplitLines(text);

        if (lines.Count == 0)
            yield break;

        var headerLine = lines[0];
        var columnIndices = ParseHeader(headerLine, text);
        var startIndex = _options.HasHeaders ? 1 : 0;

        for (int i = startIndex; i < lines.Count; i++)
        {
            var line = lines[i];
            if (line.End.Value - line.Start.Value == 0)
                continue;

            var transaction = ParseTransactionFromLine(line, account, columnIndices, text);
            if (transaction.HasValue)
                yield return transaction.Value;
        }

        Utilities.AdvanceReader(reader, sequence);
    }

    private List<Range> SplitLines(string text)
    {
        var lines = new List<Range>();
        int start = 0;

        for (int i = 0; i < text.Length; i++)
        {
            if (text[i] == '\n' || (text[i] == '\r' && (i + 1 >= text.Length || text[i + 1] != '\n')))
            {
                var length = i - start;
                if (length > 0)
                    lines.Add(new Range(start, start + length));
                start = i + 1;
                if (text[i] == '\r' && i + 1 < text.Length && text[i + 1] == '\n')
                    i++; // Skip \n
            }
        }

        if (start < text.Length)
            lines.Add(new Range(start, text.Length));

        return lines;
    }

    private Dictionary<string, int> ParseHeader(Range headerRange, string text)
    {
        var indices = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        var headerLine = text[headerRange];
        var columns = SplitLine(headerLine, _options.Delimiter);

        for (int i = 0; i < columns.Count; i++)
        {
            var columnRange = columns[i];
            var columnName = text[columnRange].Trim();
            indices[columnName] = i;
        }

        return indices;
    }

    private List<Range> SplitLine(string line, char delimiter)
    {
        var ranges = new List<Range>();
        int start = 0;
        bool inQuotes = false;

        for (int i = 0; i < line.Length; i++)
        {
            if (line[i] == '"')
            {
                if (!inQuotes)
                {
                    inQuotes = true;
                    start = i + 1; // Skip opening quote
                }
                else
                {
                    inQuotes = false;
                    // End of quoted field - the range ends before the closing quote
                    if (i + 1 < line.Length && line[i + 1] == delimiter)
                    {
                        ranges.Add(new Range(start, i));
                        i++; // Skip delimiter
                        start = i + 1;
                    }
                    else if (i + 1 >= line.Length)
                    {
                        ranges.Add(new Range(start, i));
                        start = i + 1;
                    }
                }
            }
            else if (line[i] == delimiter && !inQuotes)
            {
                ranges.Add(new Range(start, i));
                start = i + 1;
            }
        }

        if (start <= line.Length)
            ranges.Add(new Range(start, line.Length));

        return ranges;
    }

    private Account? ParseAccountFromLine(Range lineRange, Dictionary<string, int> columnIndices, string text)
    {
        var line = text[lineRange];
        var columns = SplitLine(line, _options.Delimiter);
        
        if (columns.Count == 0)
            return null;

        var accountId = GetColumnValue(columns, columnIndices, text, lineRange.Start.Value, "AccountId", "Id");
        var accountName = GetColumnValue(columns, columnIndices, text, lineRange.Start.Value, "AccountName", "Name");
        
        if (accountId.IsEmpty)
            return null;

        var accountTypeStr = GetColumnValue(columns, columnIndices, text, lineRange.Start.Value, "AccountType", "Type");
        var accountType = ParseAccountType(accountTypeStr);
        var currencyStr = GetColumnValue(columns, columnIndices, text, lineRange.Start.Value, "Currency");
        var currency = currencyStr.IsEmpty ? _options.DefaultCurrency : currencyStr.ToString();

        return new Account(
            accountId.ToString(),
            accountName.IsEmpty ? accountId.ToString() : accountName.ToString(),
            accountType,
            currency
        );
    }

    private Transaction? ParseTransactionFromLine(Range lineRange, Account account, Dictionary<string, int> columnIndices, string text)
    {
        var line = text[lineRange];
        var columns = SplitLine(line, _options.Delimiter);
        
        if (columns.Count == 0)
            return null;

        var dateStr = GetColumnValue(columns, columnIndices, text, lineRange.Start.Value, "Date");
        if (!DateTime.TryParseExact(dateStr, _options.DateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
            return null;

        var amountStr = GetColumnValue(columns, columnIndices, text, lineRange.Start.Value, "Amount");
        if (!decimal.TryParse(amountStr, NumberStyles.Currency, CultureInfo.InvariantCulture, out var amount))
            return null;

        var description = GetColumnValue(columns, columnIndices, text, lineRange.Start.Value, "Description", "Memo", "Notes");
        var typeStr = GetColumnValue(columns, columnIndices, text, lineRange.Start.Value, "Type", "TransactionType");
        var transactionType = ParseTransactionType(typeStr, amount);
        var reference = GetColumnValue(columns, columnIndices, text, lineRange.Start.Value, "Reference", "CheckNumber", "Ref");

        return new Transaction(
            account.Id,
            amount,
            date,
            description.IsEmpty ? string.Empty : description.ToString(),
            transactionType,
            reference.IsEmpty ? string.Empty : reference.ToString()
        );
    }

    private ReadOnlySpan<char> GetColumnValue(List<Range> columns, Dictionary<string, int> columnIndices, string text, int lineStart, params string[] possibleNames)
    {
        foreach (var name in possibleNames)
        {
            string? mappedName = null;
            if (_options.ColumnMapping.TryGetValue(name, out mappedName) && columnIndices.TryGetValue(mappedName, out var index))
            {
                if (index < columns.Count)
                {
                    var range = columns[index];
                    var offsetRange = new Range(range.Start.Value + lineStart, range.End.Value + lineStart);
                    return text[offsetRange].Trim().AsSpan();
                }
            }
            else if (columnIndices.TryGetValue(name, out var directIndex))
            {
                if (directIndex < columns.Count)
                {
                    var range = columns[directIndex];
                    var offsetRange = new Range(range.Start.Value + lineStart, range.End.Value + lineStart);
                    return text[offsetRange].Trim().AsSpan();
                }
            }
        }

        return ReadOnlySpan<char>.Empty;
    }

    private AccountType ParseAccountType(ReadOnlySpan<char> value)
    {
        if (value.IsEmpty)
            return _options.DefaultAccountType;

        return Enum.TryParse<AccountType>(value.ToString(), ignoreCase: true, out var type) ? type : _options.DefaultAccountType;
    }

    private TransactionType ParseTransactionType(ReadOnlySpan<char> value, decimal amount)
    {
        if (!value.IsEmpty && Enum.TryParse<TransactionType>(value.ToString(), ignoreCase: true, out var type))
            return type;

        // Infer from amount if type not specified
        return amount >= 0 ? TransactionType.Credit : TransactionType.Debit;
    }
}
