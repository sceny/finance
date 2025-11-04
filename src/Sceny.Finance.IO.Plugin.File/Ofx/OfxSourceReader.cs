using System.Buffers;
using System.Globalization;
using System.IO.Pipelines;
using System.Text;
using System.Text.RegularExpressions;
using Sceny.Finance.IO;

namespace Sceny.Finance.IO.Plugin.File.Ofx;

/// <summary>
/// OFX source reader implementation.
/// Parses OFX files using System.IO.Pipelines for zero-allocation streaming.
/// </summary>
public sealed class OfxSourceReader : ISourceReader
{
    private readonly OfxOptions _options;

    public OfxSourceReader(OfxOptions? options = null)
    {
        _options = options ?? new OfxOptions();
    }

    public async IAsyncEnumerable<Account> GetAccountsAsync(
        PipeReader reader,
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var sequence = await Utilities.ReadCompleteSequenceAsync(reader, cancellationToken).ConfigureAwait(false);
        
        // If sequence is empty, return empty
        if (sequence.Length == 0)
        {
            Utilities.AdvanceReader(reader, sequence);
            yield break;
        }
        
        var text = Utilities.SequenceToString(sequence, _options.Encoding);

        // If file is empty, return empty
        if (text.Length == 0 || string.IsNullOrWhiteSpace(text))
        {
            Utilities.AdvanceReader(reader, sequence);
            yield break;
        }
        
        // If no OFX tags at all, return empty
        if (!text.Contains('<'))
        {
            Utilities.AdvanceReader(reader, sequence);
            yield break;
        }

        // Find account information in OFX
        var accountId = ExtractTagValue(text, "ACCTID");
        var bankId = ExtractTagValue(text, "BANKID");
        var acctType = ExtractTagValue(text, "ACCTTYPE");
        
        // Check if ACCTID tag exists but is empty (whitespace) - this should return empty
        var hasEmptyAcctIdTag = text.Contains("<ACCTID>", StringComparison.OrdinalIgnoreCase) && 
                                string.IsNullOrWhiteSpace(accountId);
        
        // If ACCTID tag exists but is empty, don't yield (return empty)
        if (hasEmptyAcctIdTag)
        {
            Utilities.AdvanceReader(reader, sequence);
            yield break;
        }
        
        // If ACCTID is missing (tag doesn't exist or null), try bankId, then UNKNOWN
        // Only use UNKNOWN if we have some OFX structure (not completely empty file)
        if (accountId == null || string.IsNullOrWhiteSpace(accountId))
        {
            accountId = bankId;
            if (string.IsNullOrWhiteSpace(accountId))
            {
                // Only use UNKNOWN if we have some OFX tags (like ACCTTYPE, indicating OFX structure)
                if (text.Contains("<ACCTTYPE>", StringComparison.OrdinalIgnoreCase) || 
                    text.Contains("<BANKACCTFROM>", StringComparison.OrdinalIgnoreCase))
                {
                    accountId = "UNKNOWN";
                }
                else
                {
                    // No OFX structure, return empty
                    Utilities.AdvanceReader(reader, sequence);
                    yield break;
                }
            }
        }

        var accountName = ExtractTagValue(text, "DESC") ?? accountId ?? "UNKNOWN";
        var accountType = ParseAccountType(acctType);
        var currency = ExtractTagValue(text, "CURDEF") ?? "USD";

        // Yield if we have a non-empty account ID
        if (!string.IsNullOrWhiteSpace(accountId))
        {
            yield return new Account(
                accountId!,
                accountName!,
                accountType,
                currency
            );
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

        // Find all STMTTRN blocks (transactions)
        var transactions = ExtractTransactions(text, account);

        foreach (var transaction in transactions)
        {
            yield return transaction;
        }

        Utilities.AdvanceReader(reader, sequence);
    }

    private List<Transaction> ExtractTransactions(string text, Account account)
    {
        var transactions = new List<Transaction>();
        
        // Pattern to match STMTTRN blocks
        var pattern = @"<STMTTRN>(.*?)</STMTTRN>";
        var matches = Regex.Matches(text, pattern, RegexOptions.Singleline | RegexOptions.IgnoreCase);

        foreach (Match match in matches)
        {
            var block = match.Groups[1].Value;
            var transaction = ParseTransactionBlock(block, account);
            
            if (transaction.HasValue)
                transactions.Add(transaction.Value);
        }

        return transactions;
    }

    private Transaction? ParseTransactionBlock(string block, Account account)
    {
        var dtposted = ExtractTagValue(block, "DTPOSTED");
        if (!TryParseOfxDate(dtposted, out var date))
            return null;

        var trnamt = ExtractTagValue(block, "TRNAMT");
        if (!decimal.TryParse(trnamt, NumberStyles.Any, CultureInfo.InvariantCulture, out var amount))
            return null;

        var memo = ExtractTagValue(block, "MEMO") ?? ExtractTagValue(block, "NAME") ?? string.Empty;
        var fitid = ExtractTagValue(block, "FITID") ?? string.Empty;
        var trntype = ExtractTagValue(block, "TRNTYPE");
        var transactionType = ParseTransactionType(trntype, amount);

        return new Transaction(
            account.Id,
            amount,
            date,
            memo,
            transactionType,
            fitid
        );
    }

    private bool TryParseOfxDate(string? dateStr, out DateTime date)
    {
        date = default;

        if (string.IsNullOrWhiteSpace(dateStr))
            return false;

        // OFX dates are typically yyyyMMdd or yyyyMMddHHmmss.fffffff
        // Remove timezone offset if present
        var cleanDate = dateStr;
        if (cleanDate.Contains('[') || cleanDate.Contains('+') || cleanDate.Contains('-'))
        {
            var timeZoneMatch = Regex.Match(cleanDate, @"\[([+-]\d+):\w+\]");
            if (timeZoneMatch.Success && int.TryParse(timeZoneMatch.Groups[1].Value, out var offsetHours))
            {
                _options.TimeZoneOffset = TimeSpan.FromHours(offsetHours);
            }
            
            cleanDate = Regex.Replace(cleanDate, @"\[.*?\]", "");
            cleanDate = Regex.Replace(cleanDate, @"[+-]\d{2}:?\d{2}", "");
        }

        // Try parsing with different formats
        if (DateTime.TryParseExact(cleanDate, _options.DateTimeFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out date))
            return true;

        if (DateTime.TryParseExact(cleanDate, _options.DateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out date))
            return true;

        // Try default OFX format
        if (cleanDate.Length >= 8)
        {
            var datePart = cleanDate.Substring(0, 8);
            if (DateTime.TryParseExact(datePart, "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None, out date))
            {
                if (cleanDate.Length >= 14)
                {
                    var timePart = cleanDate.Substring(8, 6);
                    if (int.TryParse(timePart, out var hour) && hour < 24 &&
                        int.TryParse(timePart.Substring(2, 2), out var minute) && minute < 60 &&
                        int.TryParse(timePart.Substring(4, 2), out var second) && second < 60)
                    {
                        date = date.AddHours(hour).AddMinutes(minute).AddSeconds(second);
                    }
                }
                return true;
            }
        }

        return false;
    }

    private string? ExtractTagValue(string text, string tagName)
    {
        // Pattern: <TAGNAME>value</TAGNAME> or <TAGNAME>value
        var pattern = $@"<{tagName}>(.*?)(</{tagName}>|[\r\n<])";
        var match = Regex.Match(text, pattern, RegexOptions.IgnoreCase | RegexOptions.Singleline);
        
        if (match.Success && match.Groups.Count > 1)
        {
            var value = match.Groups[1].Value.Trim();
            return string.IsNullOrWhiteSpace(value) ? null : value;
        }

        return null;
    }

    private AccountType ParseAccountType(string? acctType)
    {
        if (string.IsNullOrWhiteSpace(acctType))
            return AccountType.Other;

        return acctType.ToUpperInvariant() switch
        {
            "CHECKING" => AccountType.Checking,
            "SAVINGS" => AccountType.Savings,
            "CREDITCARD" or "CREDIT" => AccountType.CreditCard,
            "MONEYMKT" => AccountType.Savings,
            _ => AccountType.Other
        };
    }

    private TransactionType ParseTransactionType(string? trntype, decimal amount)
    {
        if (!string.IsNullOrWhiteSpace(trntype))
        {
            return trntype.ToUpperInvariant() switch
            {
                "CREDIT" or "DEP" => TransactionType.Credit,
                "DEBIT" or "WITHDRAWAL" => TransactionType.Debit,
                "XFER" or "TRANSFER" => TransactionType.Transfer,
                "INT" => TransactionType.Interest,
                "FEE" => TransactionType.Fee,
                "DIV" => TransactionType.Dividend,
                "BUY" => TransactionType.Buy,
                "SELL" => TransactionType.Sell,
                _ => amount >= 0 ? TransactionType.Credit : TransactionType.Debit
            };
        }

        return amount >= 0 ? TransactionType.Credit : TransactionType.Debit;
    }
}

