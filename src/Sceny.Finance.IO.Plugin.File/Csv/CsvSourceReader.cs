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
public sealed class CsvSourceReader(CsvOptions? options = null) : ISourceReader
{
    private readonly CsvOptions _options = options ?? new CsvOptions();
    private static readonly ArrayPool<Range> RangePool = ArrayPool<Range>.Shared;

    public async IAsyncEnumerable<Account> GetAccountsAsync(
        PipeReader reader,
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var sequence = await Utilities.ReadCompleteSequenceAsync(reader, cancellationToken).ConfigureAwait(false);
        var text = Utilities.SequenceToString(sequence, _options.Encoding);
        var (lines, lineCount) = SplitLines(text);

        try
        {
            if (lineCount == 0)
                yield break;

            var headerLine = lines[0];
            var columnIndices = ParseHeader(headerLine, text);

            // Parse accounts from first data row
            if (lineCount > 1 && _options.HasHeaders)
            {
                var firstDataLine = lines[1];
                var account = ParseAccountFromLine(firstDataLine, columnIndices, text);
                if (account.HasValue)
                    yield return account.Value;
            }
        }
        finally
        {
            RangePool.Return(lines);
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
        var (lines, lineCount) = SplitLines(text);

        try
        {
            if (lineCount == 0)
                yield break;

            var headerLine = lines[0];
            var columnIndices = ParseHeader(headerLine, text);
            var startIndex = _options.HasHeaders ? 1 : 0;

            for (int i = startIndex; i < lineCount; i++)
            {
                var line = lines[i];
                if (line.End.Value - line.Start.Value == 0)
                    continue;

                var transaction = ParseTransactionFromLine(line, account, columnIndices, text);
                if (transaction.HasValue)
                    yield return transaction.Value;
            }
        }
        finally
        {
            RangePool.Return(lines);
        }

        Utilities.AdvanceReader(reader, sequence);
    }

    private (Range[] Ranges, int Count) SplitLines(string text)
    {
        // Estimate max lines (each line is at least 1 char, so max lines = text.Length)
        var maxLines = text.Length;
        var ranges = RangePool.Rent(maxLines);
        int count = 0;
        int start = 0;

        try
        {
            for (int i = 0; i < text.Length; i++)
            {
                if (text[i] == '\n' || (text[i] == '\r' && (i + 1 >= text.Length || text[i + 1] != '\n')))
                {
                    var length = i - start;
                    if (length > 0)
                    {
                        ranges[count++] = new Range(start, start + length);
                    }
                    start = i + 1;
                    if (text[i] == '\r' && i + 1 < text.Length && text[i + 1] == '\n')
                        i++; // Skip \n
                }
            }

            if (start < text.Length)
            {
                ranges[count++] = new Range(start, text.Length);
            }

            return (ranges, count);
        }
        catch
        {
            RangePool.Return(ranges);
            throw;
        }
    }

    private Dictionary<string, int> ParseHeader(Range headerRange, string text)
    {
        var indices = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        var headerLine = text[headerRange];
        var (columns, columnCount) = SplitLine(headerLine, _options.Delimiter);

        try
        {
            for (int i = 0; i < columnCount; i++)
            {
                var columnRange = columns[i];
                var columnName = text[columnRange].Trim();
                indices[columnName] = i;
            }

            return indices;
        }
        finally
        {
            RangePool.Return(columns);
        }
    }

    private (Range[] Ranges, int Count) SplitLine(string line, char delimiter)
    {
        // Estimate max columns (each column is at least 1 char, so max = line.Length)
        var maxColumns = line.Length;
        var ranges = RangePool.Rent(maxColumns);
        int count = 0;
        int start = 0;
        bool inQuotes = false;

        try
        {
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
                            ranges[count++] = new Range(start, i);
                            i++; // Skip delimiter
                            start = i + 1;
                        }
                        else if (i + 1 >= line.Length)
                        {
                            ranges[count++] = new Range(start, i);
                            start = i + 1;
                        }
                    }
                }
                else if (line[i] == delimiter && !inQuotes)
                {
                    ranges[count++] = new Range(start, i);
                    start = i + 1;
                }
            }

            if (start <= line.Length)
            {
                ranges[count++] = new Range(start, line.Length);
            }

            return (ranges, count);
        }
        catch
        {
            RangePool.Return(ranges);
            throw;
        }
    }

    private Account? ParseAccountFromLine(Range lineRange, Dictionary<string, int> columnIndices, string text)
    {
        var line = text[lineRange];
        var (columns, columnCount) = SplitLine(line, _options.Delimiter);
        
        try
        {
            if (columnCount == 0)
                return null;

            var accountId = GetColumnValue(columns, columnCount, columnIndices, text, lineRange.Start.Value, "AccountId", "Id");
            var accountName = GetColumnValue(columns, columnCount, columnIndices, text, lineRange.Start.Value, "AccountName", "Name");
            
            if (accountId.Length == 0)
                return null;

            var accountTypeStr = GetColumnValue(columns, columnCount, columnIndices, text, lineRange.Start.Value, "AccountType", "Type");
            var accountType = ParseAccountType(accountTypeStr, text);
            var currencyStr = GetColumnValue(columns, columnCount, columnIndices, text, lineRange.Start.Value, "Currency");
            var currency = currencyStr.Length == 0 ? new ReadOnlyString(_options.DefaultCurrency) : new ReadOnlyString(text.AsMemory(currencyStr.Start, currencyStr.Length));

            return new Account(
                new ReadOnlyString(text.AsMemory(accountId.Start, accountId.Length)),
                accountName.Length == 0 ? new ReadOnlyString(text.AsMemory(accountId.Start, accountId.Length)) : new ReadOnlyString(text.AsMemory(accountName.Start, accountName.Length)),
                accountType,
                currency
            );
        }
        finally
        {
            RangePool.Return(columns);
        }
    }

    private Transaction? ParseTransactionFromLine(Range lineRange, Account account, Dictionary<string, int> columnIndices, string text)
    {
        var line = text[lineRange];
        var (columns, columnCount) = SplitLine(line, _options.Delimiter);
        
        try
        {
            if (columnCount == 0)
                return null;

            var dateStr = GetColumnValue(columns, columnCount, columnIndices, text, lineRange.Start.Value, "Date");
            var dateSpan = dateStr.Length > 0 ? text.AsSpan(dateStr.Start, dateStr.Length) : ReadOnlySpan<char>.Empty;
            if (!DateTime.TryParseExact(dateSpan, _options.DateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
                return null;

            var amountStr = GetColumnValue(columns, columnCount, columnIndices, text, lineRange.Start.Value, "Amount");
            var amountSpan = amountStr.Length > 0 ? text.AsSpan(amountStr.Start, amountStr.Length) : ReadOnlySpan<char>.Empty;
            if (!decimal.TryParse(amountSpan, NumberStyles.Currency, CultureInfo.InvariantCulture, out var amount))
                return null;

            var description = GetColumnValue(columns, columnCount, columnIndices, text, lineRange.Start.Value, "Description", "Memo", "Notes");
            var typeStr = GetColumnValue(columns, columnCount, columnIndices, text, lineRange.Start.Value, "Type", "TransactionType");
            var transactionType = ParseTransactionType(typeStr, amount, text);
            var reference = GetColumnValue(columns, columnCount, columnIndices, text, lineRange.Start.Value, "Reference", "CheckNumber", "Ref");

            return new Transaction(
                account.Id,
                amount,
                date,
                description.Length == 0 ? default : new ReadOnlyString(text.AsMemory(description.Start, description.Length)),
                transactionType,
                reference.Length == 0 ? default : new ReadOnlyString(text.AsMemory(reference.Start, reference.Length))
            );
        }
        finally
        {
            RangePool.Return(columns);
        }
    }

    private (int Start, int Length) GetColumnValue(Range[] columns, int columnCount, Dictionary<string, int> columnIndices, string text, int lineStart, params string[] possibleNames)
    {
        foreach (var name in possibleNames)
        {
            string? mappedName = null;
            if (_options.ColumnMapping.TryGetValue(name, out mappedName) && columnIndices.TryGetValue(mappedName, out var index))
            {
                if (index < columnCount)
                {
                    var range = columns[index];
                    var offsetStart = range.Start.Value + lineStart;
                    var offsetEnd = range.End.Value + lineStart;
                    var span = text.AsSpan(offsetStart, offsetEnd - offsetStart);
                    var trimmed = span.Trim();
                    var trimStart = span.IndexOf(trimmed);
                    return (offsetStart + trimStart, trimmed.Length);
                }
            }
            else if (columnIndices.TryGetValue(name, out var directIndex))
            {
                if (directIndex < columnCount)
                {
                    var range = columns[directIndex];
                    var offsetStart = range.Start.Value + lineStart;
                    var offsetEnd = range.End.Value + lineStart;
                    var span = text.AsSpan(offsetStart, offsetEnd - offsetStart);
                    var trimmed = span.Trim();
                    var trimStart = span.IndexOf(trimmed);
                    return (offsetStart + trimStart, trimmed.Length);
                }
            }
        }

        return (0, 0);
    }

    private AccountType ParseAccountType((int Start, int Length) value, string text)
    {
        if (value.Length == 0)
            return _options.DefaultAccountType;

        // Use span-based enum parsing without ToString()
        var span = text.AsSpan(value.Start, value.Length);
        return Enum.TryParse<AccountType>(span, ignoreCase: true, out var type) ? type : _options.DefaultAccountType;
    }

    private TransactionType ParseTransactionType((int Start, int Length) value, decimal amount, string text)
    {
        if (value.Length > 0)
        {
            var span = text.AsSpan(value.Start, value.Length);
            if (Enum.TryParse<TransactionType>(span, ignoreCase: true, out var type))
                return type;
        }

        // Infer from amount if type not specified
        return amount >= 0 ? TransactionType.Credit : TransactionType.Debit;
    }
}
