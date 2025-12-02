using System.Buffers;
using System.Globalization;
using System.IO.Pipelines;
using System.Text;
using Sceny.Finance.IO;

namespace Sceny.Finance.IO.Plugin.File.Csv;

/// <summary>
/// CSV source writer implementation.
/// Writes CSV files using System.IO.Pipelines for zero-allocation streaming.
/// </summary>
public sealed class CsvSourceWriter(CsvOptions? options = null) : ISourceWriter<CsvAccountProperties, CsvTransactionProperties>
{
    private readonly CsvOptions _options = options ?? new CsvOptions();
    private bool _headerWritten;
    private bool _isWritingAccounts;

    public async Task WriteAccountsAsync(
        PipeWriter writer,
        IAsyncEnumerable<Account<CsvAccountProperties>> accounts,
        CancellationToken cancellationToken = default)
    {
        _isWritingAccounts = true;
        await WriteHeaderAsync(writer, cancellationToken).ConfigureAwait(false);

        await foreach (var account in accounts.ConfigureAwait(false))
        {
            await WriteAccountAsync(writer, account, cancellationToken).ConfigureAwait(false);
        }
    }

    public async Task WriteTransactionsAsync(
        Account<CsvAccountProperties> account,
        PipeWriter writer,
        IAsyncEnumerable<Transaction<CsvTransactionProperties>> transactions,
        CancellationToken cancellationToken = default)
    {
        _isWritingAccounts = false;
        // For CSV, we write transactions without account grouping in the header
        // Header is written once if not already written
        if (!_headerWritten)
        {
            await WriteHeaderAsync(writer, cancellationToken).ConfigureAwait(false);
        }

        await foreach (var transaction in transactions.ConfigureAwait(false))
        {
            await WriteTransactionAsync(writer, transaction, cancellationToken).ConfigureAwait(false);
        }
    }

    public async Task WriteAccountAsync(
        PipeWriter writer,
        Account<CsvAccountProperties> account,
        CancellationToken cancellationToken = default)
    {
        _isWritingAccounts = true;
        if (!_headerWritten)
        {
            await WriteHeaderAsync(writer, cancellationToken).ConfigureAwait(false);
        }

        // Write account row - typically one row per account
        // Format: AccountId,AccountName,AccountType,Currency
        var line = FormatAccountLine(account);
        await WriteLineAsync(writer, line, cancellationToken).ConfigureAwait(false);
    }

    public async Task WriteTransactionAsync(
        PipeWriter writer,
        Transaction<CsvTransactionProperties> transaction,
        CancellationToken cancellationToken = default)
    {
        _isWritingAccounts = false;
        if (!_headerWritten)
        {
            await WriteHeaderAsync(writer, cancellationToken).ConfigureAwait(false);
        }

        // Write transaction row
        // Format: AccountId,Date,Amount,Description,Type,Reference
        var line = FormatTransactionLine(transaction);
        await WriteLineAsync(writer, line, cancellationToken).ConfigureAwait(false);
    }

    public async Task WriteHeaderAsync(
        PipeWriter writer,
        CancellationToken cancellationToken = default)
    {
        if (_headerWritten || !_options.HasHeaders)
            return;

        // Write CSV header based on column mapping
        var headerLine = BuildHeaderLine();
        await WriteLineAsync(writer, headerLine, cancellationToken).ConfigureAwait(false);
        _headerWritten = true;
    }

    public Task WriteFooterAsync(
        PipeWriter writer,
        CancellationToken cancellationToken = default)
    {
        // CSV doesn't have a footer
        return Task.CompletedTask;
    }

    private string BuildHeaderLine()
    {
        // Build header from column mapping - use reverse mapping to get column names
        var columns = new List<string>();
        
        // Determine which columns we'll write based on what we're currently writing
        // If we're writing accounts, use account columns; if transactions, use transaction columns
        if (_isWritingAccounts)
        {
            // Account columns
            var accountIdColumn = GetColumnName("AccountId");
            var accountNameColumn = GetColumnName("AccountName");
            var accountTypeColumn = GetColumnName("AccountType");
            var currencyColumn = GetColumnName("Currency");
            
            columns.Add(accountIdColumn);
            columns.Add(accountNameColumn);
            columns.Add(accountTypeColumn);
            columns.Add(currencyColumn);
            
            // Properties columns
            columns.Add(GetColumnName("BankName"));
            columns.Add(GetColumnName("BranchCode"));
        }
        else
        {
            // Transaction columns
            var accountIdColumn = GetColumnName("AccountId");
            var dateColumn = GetColumnName("Date");
            var amountColumn = GetColumnName("Amount");
            var descriptionColumn = GetColumnName("Description");
            var typeColumn = GetColumnName("Type");
            var referenceColumn = GetColumnName("Reference");
            
            columns.Add(accountIdColumn);
            columns.Add(dateColumn);
            columns.Add(amountColumn);
            columns.Add(descriptionColumn);
            columns.Add(typeColumn);
            columns.Add(referenceColumn);
        }

        return string.Join(_options.Delimiter, columns);
    }

    private string GetColumnName(string targetColumn)
    {
        // Check if there's a mapping, otherwise use the target column name
        if (_options.ColumnMapping.TryGetValue(targetColumn, out var mappedName))
        {
            return mappedName;
        }
        return targetColumn;
    }

    private string FormatAccountLine(Account<CsvAccountProperties> account)
    {
        var parts = new List<string>
        {
            EscapeCsvField(account.Id.ToString()),
            EscapeCsvField(account.Name.ToString()),
            account.Type.ToString(),
            EscapeCsvField(account.Currency.ToString())
        };

        if (account.Properties is IBankName bankNameProp && !bankNameProp.BankName.IsEmpty)
        {
            parts.Add(EscapeCsvField(bankNameProp.BankName.ToString()));
        }

        if (account.Properties is IBranchCode branchCodeProp && !branchCodeProp.BranchCode.IsEmpty)
        {
            parts.Add(EscapeCsvField(branchCodeProp.BranchCode.ToString()));
        }

        if (account.Properties is IExtended extendedProp && !extendedProp.Extended.IsEmpty)
        {
            foreach (var kvp in extendedProp.Extended)
            {
                parts.Add(EscapeCsvField(kvp.Value.ToString()));
            }
        }

        return string.Join(_options.Delimiter, parts);
    }

    private string FormatTransactionLine(Transaction<CsvTransactionProperties> transaction)
    {
        var parts = new List<string>
        {
            EscapeCsvField(transaction.AccountId.ToString()),
            transaction.Date.ToString(_options.DateFormat, CultureInfo.InvariantCulture),
            transaction.Amount.ToString("F2", CultureInfo.InvariantCulture),
            EscapeCsvField(transaction.Description.ToString()),
            transaction.Type.ToString(),
            EscapeCsvField(transaction.Reference.ToString())
        };

        if (transaction.Properties is IExtended extendedProp && !extendedProp.Extended.IsEmpty)
        {
            foreach (var kvp in extendedProp.Extended)
            {
                parts.Add(EscapeCsvField(kvp.Value.ToString()));
            }
        }

        return string.Join(_options.Delimiter, parts);
    }

    private static string EscapeCsvField(string field)
    {
        // Escape field if it contains delimiter, newline, or quote
        if (string.IsNullOrEmpty(field))
            return field;

        if (field.Contains(',') || field.Contains('\n') || field.Contains('\r') || field.Contains('"'))
        {
            // Replace " with ""
            field = field.Replace("\"", "\"\"");
            // Wrap in quotes
            return $"\"{field}\"";
        }

        return field;
    }

    private async Task WriteLineAsync(
        PipeWriter writer,
        string line,
        CancellationToken cancellationToken)
    {
        // Encode line to bytes using zero-allocation pattern
        var encoding = _options.Encoding;
        var lineWithNewline = line + "\r\n";
        var maxBytes = encoding.GetMaxByteCount(lineWithNewline.Length);
        var span = writer.GetSpan(maxBytes);
        
        var bytesWritten = encoding.GetBytes(lineWithNewline.AsSpan(), span);
        writer.Advance(bytesWritten);

        await writer.FlushAsync(cancellationToken).ConfigureAwait(false);
    }
}

