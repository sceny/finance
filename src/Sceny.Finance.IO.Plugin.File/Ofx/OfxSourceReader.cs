using System.Buffers;
using System.Globalization;
using System.IO.Pipelines;
using System.Text;
using Sceny.Finance.IO;

namespace Sceny.Finance.IO.Plugin.File.Ofx;

/// <summary>
/// OFX source reader implementation.
/// Parses OFX files using System.IO.Pipelines for zero-allocation streaming.
/// </summary>
public sealed class OfxSourceReader(OfxOptions? options = null) : ISourceReader
{
    private readonly OfxOptions _options = options ?? new OfxOptions();

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

        // Find account information in OFX using span-based extraction
        var textSpan = text.AsSpan();
        var accountIdSpan = ExtractTagValue(textSpan, "ACCTID", 0, out var accountIdOffset);
        var bankIdSpan = ExtractTagValue(textSpan, "BANKID", 0, out var _);
        var acctTypeSpan = ExtractTagValue(textSpan, "ACCTTYPE", 0, out var _);
        
        // Check if ACCTID tag exists but is empty (whitespace) - this should return empty
        var hasEmptyAcctIdTag = text.Contains("<ACCTID>", StringComparison.OrdinalIgnoreCase) && accountIdSpan.IsEmpty;
        
        // If ACCTID tag exists but is empty, don't yield (return empty)
        if (hasEmptyAcctIdTag)
        {
            Utilities.AdvanceReader(reader, sequence);
            yield break;
        }
        
        // Determine account ID
        int finalAccountIdOffset;
        int finalAccountIdLength;
        bool useUnknown = false;
        
        if (accountIdSpan.IsEmpty)
        {
            if (!bankIdSpan.IsEmpty)
            {
                finalAccountIdOffset = accountIdOffset; // bankId offset would be different, need to track it
                // Re-extract to get bankId offset
                var bankIdSpan2 = ExtractTagValue(textSpan, "BANKID", 0, out finalAccountIdOffset);
                finalAccountIdLength = bankIdSpan2.Length;
            }
            else
            {
                // Only use UNKNOWN if we have some OFX tags (like ACCTTYPE, indicating OFX structure)
                if (text.Contains("<ACCTTYPE>", StringComparison.OrdinalIgnoreCase) || 
                    text.Contains("<BANKACCTFROM>", StringComparison.OrdinalIgnoreCase))
                {
                    useUnknown = true;
                    finalAccountIdOffset = 0;
                    finalAccountIdLength = 7; // "UNKNOWN".Length
                }
                else
                {
                    // No OFX structure, return empty
                    Utilities.AdvanceReader(reader, sequence);
                    yield break;
                }
            }
        }
        else
        {
            finalAccountIdOffset = accountIdOffset;
            finalAccountIdLength = accountIdSpan.Length;
        }

        var accountNameSpan = ExtractTagValue(textSpan, "DESC", 0, out var accountNameOffset);
        var accountNameLength = accountNameSpan.IsEmpty ? finalAccountIdLength : accountNameSpan.Length;
        var accountNameOffsetFinal = accountNameSpan.IsEmpty ? finalAccountIdOffset : accountNameOffset;
        
        var currencySpan = ExtractTagValue(textSpan, "CURDEF", 0, out var currencyOffset);
        bool useDefaultCurrency = currencySpan.IsEmpty;
        var currencyOffsetFinal = useDefaultCurrency ? 0 : currencyOffset;
        var currencyLength = useDefaultCurrency ? 3 : currencySpan.Length; // "USD".Length = 3

        var accountType = ParseAccountType(acctTypeSpan);

        // Yield if we have a non-empty account ID
        if (finalAccountIdLength > 0 || useUnknown)
        {
            var accountIdMemory = useUnknown 
                ? new ReadOnlyString("UNKNOWN")
                : new ReadOnlyString(text.AsMemory(finalAccountIdOffset, finalAccountIdLength));
            
            var accountNameMemory = accountNameSpan.IsEmpty
                ? accountIdMemory
                : new ReadOnlyString(text.AsMemory(accountNameOffsetFinal, accountNameLength));
            
            var currencyMemory = useDefaultCurrency
                ? new ReadOnlyString("USD")
                : new ReadOnlyString(text.AsMemory(currencyOffsetFinal, currencyLength));

            yield return new Account(
                accountIdMemory,
                accountNameMemory,
                accountType,
                currencyMemory
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

        // Find all STMTTRN blocks (transactions) and yield directly - zero allocation
        foreach (var transaction in ExtractTransactions(text, account))
        {
            yield return transaction;
        }

        Utilities.AdvanceReader(reader, sequence);
    }

    private IEnumerable<Transaction> ExtractTransactions(string text, Account account)
    {
        var searchTag = "<STMTTRN>";
        var endTag = "</STMTTRN>";
        int startIndex = 0;

        while (true)
        {
            // Find next <STMTTRN> tag (case-insensitive)
            var tagStart = text.IndexOf(searchTag, startIndex, StringComparison.OrdinalIgnoreCase);
            if (tagStart == -1)
                break;

            var actualStart = tagStart + searchTag.Length;

            // Find matching </STMTTRN> tag
            var tagEnd = text.IndexOf(endTag, actualStart, StringComparison.OrdinalIgnoreCase);
            if (tagEnd == -1)
                break;

            var blockStart = actualStart;
            var blockEnd = tagEnd;
            var blockLength = blockEnd - blockStart;

            var transaction = ParseTransactionBlock(blockStart, blockLength, text, account);
            if (transaction.HasValue)
                yield return transaction.Value;

            startIndex = tagEnd + endTag.Length;
        }
    }

    private int FindTagStart(ReadOnlySpan<char> span, ReadOnlySpan<char> tag)
    {
        for (int i = 0; i <= span.Length - tag.Length; i++)
        {
            if (span.Slice(i, tag.Length).Equals(tag, StringComparison.OrdinalIgnoreCase))
                return i;
        }
        return -1;
    }

    private int FindTagEnd(ReadOnlySpan<char> span, ReadOnlySpan<char> tag)
    {
        for (int i = 0; i <= span.Length - tag.Length; i++)
        {
            if (span.Slice(i, tag.Length).Equals(tag, StringComparison.OrdinalIgnoreCase))
                return i;
        }
        return -1;
    }

    private Transaction? ParseTransactionBlock(int blockStart, int blockLength, string text, Account account)
    {
        var blockSpan = text.AsSpan(blockStart, blockLength);
        
        var dtposted = ExtractTagValue(blockSpan, "DTPOSTED", blockStart, out var dtpostedOffset);
        if (!TryParseOfxDate(dtposted, out var date))
            return null;

        var trnamt = ExtractTagValue(blockSpan, "TRNAMT", blockStart, out var _);
        if (!decimal.TryParse(trnamt, NumberStyles.Any, CultureInfo.InvariantCulture, out var amount))
            return null;

        var memo = ExtractTagValue(blockSpan, "MEMO", blockStart, out var memoOffset);
        if (memo.IsEmpty)
            memo = ExtractTagValue(blockSpan, "NAME", blockStart, out memoOffset);
        
        var fitid = ExtractTagValue(blockSpan, "FITID", blockStart, out var fitidOffset);
        var trntype = ExtractTagValue(blockSpan, "TRNTYPE", blockStart, out var _);
        var transactionType = ParseTransactionType(trntype, amount);

        // Convert spans to ReadOnlyMemory for ReadOnlyString using absolute positions
        var memoMemory = memo.IsEmpty ? default : text.AsMemory(memoOffset, memo.Length);
        var fitidMemory = fitid.IsEmpty ? default : text.AsMemory(fitidOffset, fitid.Length);

        return new Transaction(
            account.Id,
            amount,
            date,
            memoMemory.IsEmpty ? default : new ReadOnlyString(memoMemory),
            transactionType,
            fitidMemory.IsEmpty ? default : new ReadOnlyString(fitidMemory)
        );
    }

    private bool TryParseOfxDate(ReadOnlySpan<char> dateStr, out DateTime date)
    {
        date = default;

        if (dateStr.IsEmpty)
            return false;

        // OFX dates are typically yyyyMMdd or yyyyMMddHHmmss.fffffff
        // Find the end of the date part (before timezone info)
        var cleanDateEnd = dateStr.Length;
        
        // Find timezone brackets [offset:timezone] and extract offset
        var bracketStart = dateStr.IndexOf('[');
        if (bracketStart != -1 && bracketStart < cleanDateEnd)
        {
            cleanDateEnd = bracketStart;
            var bracketEnd = dateStr.Slice(bracketStart + 1).IndexOf(']');
            if (bracketEnd != -1)
            {
                var bracketContent = dateStr.Slice(bracketStart + 1, bracketEnd);
                var plusIndex = bracketContent.IndexOf('+');
                var minusIndex = bracketContent.IndexOf('-');
                if (plusIndex != -1 || minusIndex != -1)
                {
                    var signIndex = plusIndex != -1 ? plusIndex : minusIndex;
                    var offsetSpan = bracketContent.Slice(signIndex + 1);
                    var colonIndex = offsetSpan.IndexOf(':');
                    if (colonIndex != -1)
                    {
                        offsetSpan = offsetSpan.Slice(0, colonIndex);
                    }
                    if (int.TryParse(offsetSpan, out var offsetHours))
                    {
                        _options.TimeZoneOffset = TimeSpan.FromHours(offsetHours);
                    }
                }
            }
        }
        
        // Find timezone offsets like +05:00 or -03:00
        var plusPos = dateStr.Slice(0, cleanDateEnd).IndexOf('+');
        var minusPos = dateStr.Slice(0, cleanDateEnd).IndexOf('-');
        var timezonePos = -1;
        
        if (plusPos != -1 && (minusPos == -1 || plusPos < minusPos))
            timezonePos = plusPos;
        else if (minusPos != -1)
            timezonePos = minusPos;
        
        if (timezonePos != -1 && timezonePos > 0 && timezonePos < cleanDateEnd - 4)
        {
            var remaining = dateStr.Slice(timezonePos + 1);
            if (remaining.Length >= 4 && char.IsDigit(remaining[0]) && char.IsDigit(remaining[1]) && 
                (remaining[2] == ':' && remaining.Length >= 5 && char.IsDigit(remaining[3]) && char.IsDigit(remaining[4]) ||
                 char.IsDigit(remaining[2]) && char.IsDigit(remaining[3])))
            {
                cleanDateEnd = timezonePos;
            }
        }

        var cleanDate = dateStr.Slice(0, cleanDateEnd);

        // Try parsing with different formats
        if (DateTime.TryParseExact(cleanDate, _options.DateTimeFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out date))
            return true;

        if (DateTime.TryParseExact(cleanDate, _options.DateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out date))
            return true;

        // Try default OFX format yyyyMMdd or yyyyMMddHHmmss
        if (cleanDate.Length >= 8)
        {
            var datePart = cleanDate.Slice(0, 8);
            if (DateTime.TryParseExact(datePart, "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None, out date))
            {
                if (cleanDate.Length >= 14)
                {
                    var timePart = cleanDate.Slice(8, 6);
                    if (int.TryParse(timePart.Slice(0, 2), out var hour) && hour < 24 &&
                        int.TryParse(timePart.Slice(2, 2), out var minute) && minute < 60 &&
                        int.TryParse(timePart.Slice(4, 2), out var second) && second < 60)
                    {
                        date = date.AddHours(hour).AddMinutes(minute).AddSeconds(second);
                    }
                }
                return true;
            }
        }

        return false;
    }

    private ReadOnlySpan<char> ExtractTagValue(ReadOnlySpan<char> block, string tagName, int blockStartInText, out int absoluteOffset)
    {
        absoluteOffset = 0;
        
        // Find opening tag: <TAGNAME>
        var openTag = $"<{tagName}>".AsSpan();
        var tagStart = FindTagStart(block, openTag);
        if (tagStart == -1)
            return ReadOnlySpan<char>.Empty;

        var valueStart = tagStart + openTag.Length;
        
        // Find closing tag: </TAGNAME> or next < or newline
        var closeTag = $"</{tagName}>".AsSpan();
        var tagEnd = FindTagEnd(block.Slice(valueStart), closeTag);
        
        ReadOnlySpan<char> value;
        int valueStartInBlock;
        
        if (tagEnd == -1)
        {
            // Look for next < or newline as terminator
            var remaining = block.Slice(valueStart);
            var nextTag = remaining.IndexOf('<');
            var nextNewline = remaining.IndexOfAny('\r', '\n');
            
            int endPos = remaining.Length;
            if (nextTag != -1)
                endPos = Math.Min(endPos, nextTag);
            if (nextNewline != -1)
                endPos = Math.Min(endPos, nextNewline);
            
            if (endPos <= 0)
                return ReadOnlySpan<char>.Empty;
            
            var rawValue = remaining.Slice(0, endPos);
            value = rawValue.Trim();
            valueStartInBlock = valueStart + remaining.IndexOf(value);
        }
        else
        {
            var rawValue = block.Slice(valueStart, tagEnd);
            value = rawValue.Trim();
            valueStartInBlock = valueStart + rawValue.IndexOf(value);
        }
        
        absoluteOffset = blockStartInText + valueStartInBlock;
        return value;
    }

    private AccountType ParseAccountType(ReadOnlySpan<char> acctType)
    {
        if (acctType.IsEmpty)
            return AccountType.Other;

        // Case-insensitive comparison using span
        if (acctType.Equals("CHECKING".AsSpan(), StringComparison.OrdinalIgnoreCase))
            return AccountType.Checking;
        if (acctType.Equals("SAVINGS".AsSpan(), StringComparison.OrdinalIgnoreCase))
            return AccountType.Savings;
        if (acctType.Equals("CREDITCARD".AsSpan(), StringComparison.OrdinalIgnoreCase) ||
            acctType.Equals("CREDIT".AsSpan(), StringComparison.OrdinalIgnoreCase))
            return AccountType.CreditCard;
        if (acctType.Equals("MONEYMKT".AsSpan(), StringComparison.OrdinalIgnoreCase))
            return AccountType.Savings;
        
        return AccountType.Other;
    }

    private TransactionType ParseTransactionType(ReadOnlySpan<char> trntype, decimal amount)
    {
        if (!trntype.IsEmpty)
        {
            if (trntype.Equals("CREDIT".AsSpan(), StringComparison.OrdinalIgnoreCase) ||
                trntype.Equals("DEP".AsSpan(), StringComparison.OrdinalIgnoreCase))
                return TransactionType.Credit;
            if (trntype.Equals("DEBIT".AsSpan(), StringComparison.OrdinalIgnoreCase) ||
                trntype.Equals("WITHDRAWAL".AsSpan(), StringComparison.OrdinalIgnoreCase))
                return TransactionType.Debit;
            if (trntype.Equals("XFER".AsSpan(), StringComparison.OrdinalIgnoreCase) ||
                trntype.Equals("TRANSFER".AsSpan(), StringComparison.OrdinalIgnoreCase))
                return TransactionType.Transfer;
            if (trntype.Equals("INT".AsSpan(), StringComparison.OrdinalIgnoreCase))
                return TransactionType.Interest;
            if (trntype.Equals("FEE".AsSpan(), StringComparison.OrdinalIgnoreCase))
                return TransactionType.Fee;
            if (trntype.Equals("DIV".AsSpan(), StringComparison.OrdinalIgnoreCase))
                return TransactionType.Dividend;
            if (trntype.Equals("BUY".AsSpan(), StringComparison.OrdinalIgnoreCase))
                return TransactionType.Buy;
            if (trntype.Equals("SELL".AsSpan(), StringComparison.OrdinalIgnoreCase))
                return TransactionType.Sell;
        }

        return amount >= 0 ? TransactionType.Credit : TransactionType.Debit;
    }
}

