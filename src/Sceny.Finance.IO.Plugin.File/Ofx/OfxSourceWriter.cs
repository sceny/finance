using System.Buffers;
using System.Globalization;
using System.IO.Pipelines;
using System.Text;
using Sceny.Finance.IO;

namespace Sceny.Finance.IO.Plugin.File.Ofx;

/// <summary>
/// OFX source writer implementation.
/// Writes OFX files using System.IO.Pipelines for zero-allocation streaming.
/// </summary>
public sealed class OfxSourceWriter(OfxOptions? options = null) : ISourceWriter<OfxAccountProperties, OfxTransactionProperties>
{
    private readonly OfxOptions _options = options ?? new OfxOptions();
    private bool _headerWritten;
    private bool _inBankMessages;
    private Account<OfxAccountProperties>? _currentAccount;
    private bool _inTransactionList;

    public async Task WriteAccountsAsync(
        PipeWriter writer,
        IAsyncEnumerable<Account<OfxAccountProperties>> accounts,
        CancellationToken cancellationToken = default)
    {
        await WriteHeaderAsync(writer, cancellationToken).ConfigureAwait(false);

        await foreach (var account in accounts.ConfigureAwait(false))
        {
            await WriteAccountAsync(writer, account, cancellationToken).ConfigureAwait(false);
        }

        await WriteFooterAsync(writer, cancellationToken).ConfigureAwait(false);
    }

    public async Task WriteTransactionsAsync(
        Account<OfxAccountProperties> account,
        PipeWriter writer,
        IAsyncEnumerable<Transaction<OfxTransactionProperties>> transactions,
        CancellationToken cancellationToken = default)
    {
        if (!_headerWritten)
        {
            await WriteHeaderAsync(writer, cancellationToken).ConfigureAwait(false);
        }

        // Start bank message block if not already started
        if (!_inBankMessages)
        {
            await WriteAsync(writer, "<BANKMSGSRSV1>\r\n", cancellationToken).ConfigureAwait(false);
            _inBankMessages = true;
        }

        // Write account statement if this is a new account
        if (!_currentAccount.HasValue || _currentAccount.Value.Id != account.Id)
        {
            if (_currentAccount.HasValue && _inTransactionList)
            {
                // Close previous transaction list and statement
                await WriteAsync(writer, "</BANKTRANLIST>\r\n</STMTRS>\r\n</STMTTRNRS>\r\n", cancellationToken).ConfigureAwait(false);
                _inTransactionList = false;
            }

            // Start new statement response
            await WriteAsync(writer, "<STMTTRNRS>\r\n<STMTRS>\r\n", cancellationToken).ConfigureAwait(false);
            await WriteAccountBlockAsync(writer, account, cancellationToken).ConfigureAwait(false);
            await WriteAsync(writer, "<BANKTRANLIST>\r\n", cancellationToken).ConfigureAwait(false);
            _inTransactionList = true;
            _currentAccount = account;
        }

        await foreach (var transaction in transactions.ConfigureAwait(false))
        {
            await WriteTransactionAsync(writer, transaction, cancellationToken).ConfigureAwait(false);
        }
    }

    public async Task WriteAccountAsync(
        PipeWriter writer,
        Account<OfxAccountProperties> account,
        CancellationToken cancellationToken = default)
    {
        if (!_headerWritten)
        {
            await WriteHeaderAsync(writer, cancellationToken).ConfigureAwait(false);
        }

        // Start bank message block if not already started
        if (!_inBankMessages)
        {
            await WriteAsync(writer, "<BANKMSGSRSV1>\r\n", cancellationToken).ConfigureAwait(false);
            _inBankMessages = true;
        }

        // Write account statement block
        await WriteAsync(writer, "<STMTTRNRS>\r\n<STMTRS>\r\n", cancellationToken).ConfigureAwait(false);
        await WriteAccountBlockAsync(writer, account, cancellationToken).ConfigureAwait(false);
        await WriteAsync(writer, "</STMTRS>\r\n</STMTTRNRS>\r\n", cancellationToken).ConfigureAwait(false);
        _currentAccount = account;
    }

    public async Task WriteTransactionAsync(
        PipeWriter writer,
        Transaction<OfxTransactionProperties> transaction,
        CancellationToken cancellationToken = default)
    {
        if (!_headerWritten)
        {
            await WriteHeaderAsync(writer, cancellationToken).ConfigureAwait(false);
        }

        // If we don't have a transaction list started, start one
        if (!_inTransactionList)
        {
            if (!_inBankMessages)
            {
                await WriteAsync(writer, "<BANKMSGSRSV1>\r\n", cancellationToken).ConfigureAwait(false);
                _inBankMessages = true;
            }

            if (!_currentAccount.HasValue)
            {
                // Create a default account from transaction
                var account = new Account<OfxAccountProperties>(
                    transaction.AccountId,
                    transaction.AccountId,
                    AccountType.Other,
                    "USD",
                    default
                );
                await WriteAccountBlockAsync(writer, account, cancellationToken).ConfigureAwait(false);
                _currentAccount = account;
            }

            await WriteAsync(writer, "<STMTTRNRS>\r\n<STMTRS>\r\n", cancellationToken).ConfigureAwait(false);
            
            if (_currentAccount.HasValue)
            {
                await WriteAccountBlockAsync(writer, _currentAccount.Value, cancellationToken).ConfigureAwait(false);
            }
            
            await WriteAsync(writer, "<BANKTRANLIST>\r\n", cancellationToken).ConfigureAwait(false);
            _inTransactionList = true;
        }

        await WriteTransactionBlockAsync(writer, transaction, cancellationToken).ConfigureAwait(false);
    }

    public async Task WriteHeaderAsync(
        PipeWriter writer,
        CancellationToken cancellationToken = default)
    {
        if (_headerWritten)
            return;

        // Write OFX header
        var header = @"OFXHEADER:100
DATA:OFXSGML
VERSION:102
SECURITY:NONE
ENCODING:USASCII
CHARSET:1252
COMPRESSION:NONE
OLDFILEUID:NONE
NEWFILEUID:NONE

<OFX>
";
        await WriteAsync(writer, header, cancellationToken).ConfigureAwait(false);
        _headerWritten = true;
    }

    public async Task WriteFooterAsync(
        PipeWriter writer,
        CancellationToken cancellationToken = default)
    {
        if (_inTransactionList)
        {
            await WriteAsync(writer, "</BANKTRANLIST>\r\n</STMTRS>\r\n</STMTTRNRS>\r\n", cancellationToken).ConfigureAwait(false);
            _inTransactionList = false;
        }

        if (_inBankMessages)
        {
            await WriteAsync(writer, "</BANKMSGSRSV1>\r\n", cancellationToken).ConfigureAwait(false);
            _inBankMessages = false;
        }

        await WriteAsync(writer, "</OFX>\r\n", cancellationToken).ConfigureAwait(false);
    }

    private async Task WriteAccountBlockAsync(
        PipeWriter writer,
        Account<OfxAccountProperties> account,
        CancellationToken cancellationToken)
    {
        // Write BANKACCTFROM block
        await WriteAsync(writer, "<BANKACCTFROM>\r\n", cancellationToken).ConfigureAwait(false);
        
        var bankId = "";
        if (account.Properties is IBankId bankIdProp && !bankIdProp.BankId.IsEmpty)
        {
            bankId = bankIdProp.BankId.ToString();
        }
        await WriteTagAsync(writer, "BANKID", bankId, cancellationToken).ConfigureAwait(false);
        await WriteTagAsync(writer, "ACCTID", account.Id.ToString(), cancellationToken).ConfigureAwait(false);
        await WriteTagAsync(writer, "ACCTTYPE", GetAccountTypeString(account.Type), cancellationToken).ConfigureAwait(false);
        await WriteAsync(writer, "</BANKACCTFROM>\r\n", cancellationToken).ConfigureAwait(false);

        // Write account description and currency
        await WriteTagAsync(writer, "DESC", account.Name.ToString(), cancellationToken).ConfigureAwait(false);
        await WriteTagAsync(writer, "CURDEF", account.Currency.ToString(), cancellationToken).ConfigureAwait(false);

        // Write Extended properties as additional tags
        if (account.Properties is IExtended extendedProp && !extendedProp.Extended.IsEmpty)
        {
            foreach (var kvp in extendedProp.Extended)
            {
                await WriteTagAsync(writer, kvp.Key.ToString(), kvp.Value.ToString(), cancellationToken).ConfigureAwait(false);
            }
        }
    }

    private async Task WriteTransactionBlockAsync(
        PipeWriter writer,
        Transaction<OfxTransactionProperties> transaction,
        CancellationToken cancellationToken)
    {
        await WriteAsync(writer, "<STMTTRN>\r\n", cancellationToken).ConfigureAwait(false);

        // Write transaction date (OFX format: yyyyMMddHHmmss)
        var dateStr = transaction.Date.ToString(_options.DateTimeFormat, CultureInfo.InvariantCulture);
        await WriteTagAsync(writer, "DTPOSTED", dateStr, cancellationToken).ConfigureAwait(false);

        // Write transaction amount
        var amountStr = transaction.Amount.ToString("F2", CultureInfo.InvariantCulture);
        await WriteTagAsync(writer, "TRNAMT", amountStr, cancellationToken).ConfigureAwait(false);

        // Write transaction type
        await WriteTagAsync(writer, "TRNTYPE", GetTransactionTypeString(transaction.Type), cancellationToken).ConfigureAwait(false);

        // Write memo/description
        if (!transaction.Description.IsEmpty)
        {
            await WriteTagAsync(writer, "MEMO", transaction.Description.ToString(), cancellationToken).ConfigureAwait(false);
        }

        // Write FITID (reference/check number)
        if (!transaction.Reference.IsEmpty)
        {
            await WriteTagAsync(writer, "FITID", transaction.Reference.ToString(), cancellationToken).ConfigureAwait(false);
        }
        else
        {
            // Generate a FITID if not provided (use date + amount hash)
            var fitid = $"{dateStr}{amountStr.Replace(".", "").Replace("-", "")}";
            await WriteTagAsync(writer, "FITID", fitid, cancellationToken).ConfigureAwait(false);
        }

        // Write Extended properties as additional tags
        if (transaction.Properties is IExtended extendedProp && !extendedProp.Extended.IsEmpty)
        {
            foreach (var kvp in extendedProp.Extended)
            {
                await WriteTagAsync(writer, kvp.Key.ToString(), kvp.Value.ToString(), cancellationToken).ConfigureAwait(false);
            }
        }

        await WriteAsync(writer, "</STMTTRN>\r\n", cancellationToken).ConfigureAwait(false);
    }

    private static string GetAccountTypeString(AccountType type)
    {
        return type switch
        {
            AccountType.Checking => "CHECKING",
            AccountType.Savings => "SAVINGS",
            AccountType.CreditCard => "CREDITCARD",
            AccountType.Other => "OTHER",
            _ => "OTHER"
        };
    }

    private static string GetTransactionTypeString(TransactionType type)
    {
        return type switch
        {
            TransactionType.Credit => "CREDIT",
            TransactionType.Debit => "DEBIT",
            TransactionType.Transfer => "XFER",
            TransactionType.Buy => "BUY",
            TransactionType.Sell => "SELL",
            TransactionType.Dividend => "DIV",
            TransactionType.Fee => "FEE",
            TransactionType.Interest => "INT",
            TransactionType.Adjustment => "OTHER",
            TransactionType.Other => "OTHER",
            _ => "OTHER"
        };
    }

    private async Task WriteTagAsync(
        PipeWriter writer,
        string tagName,
        string value,
        CancellationToken cancellationToken)
    {
        await WriteAsync(writer, $"<{tagName}>", cancellationToken).ConfigureAwait(false);
        
        if (!string.IsNullOrEmpty(value))
        {
            // Escape XML special characters in value
            var escapedValue = EscapeXml(value);
            await WriteAsync(writer, escapedValue, cancellationToken).ConfigureAwait(false);
        }
        
        await WriteAsync(writer, $"</{tagName}>\r\n", cancellationToken).ConfigureAwait(false);
    }

    private static string EscapeXml(string value)
    {
        // Simple XML escaping - replace special characters
        return value
            .Replace("&", "&amp;")
            .Replace("<", "&lt;")
            .Replace(">", "&gt;")
            .Replace("\"", "&quot;")
            .Replace("'", "&apos;");
    }

    private async Task WriteAsync(
        PipeWriter writer,
        string text,
        CancellationToken cancellationToken)
    {
        var encoding = _options.Encoding;
        var maxBytes = encoding.GetMaxByteCount(text.Length);
        var span = writer.GetSpan(maxBytes);
        
        var bytesWritten = encoding.GetBytes(text.AsSpan(), span);
        writer.Advance(bytesWritten);

        await writer.FlushAsync(cancellationToken).ConfigureAwait(false);
    }
}

