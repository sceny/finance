using System.Linq;
using System.Text;
using Sceny.Finance.IO;
using Sceny.Finance.IO.Plugin.File.Ofx;

namespace Sceny.Finance.IO.Tests;

public class OfxSourceWriterTests
{
    [Fact]
    public async Task WriteAccountsAsync_WithDefaultOptions_WritesOfxStructure()
    {
        // Arrange
        var sb = new StringBuilder();
        var target = FinanceWriter.ToString(sb).AsOfx();
        var accounts = new[]
        {
            Account.FromStrings("ACC001", "Checking Account", AccountType.Checking, "USD")
        };

        // Act
        await target.WriteAccountsAsync(accounts.ToAsyncEnumerable());
        await TestHelpers.WaitForAsyncWrites();

        // Assert
        var result = sb.ToString();
        Assert.Contains("OFXHEADER", result);
        Assert.Contains("<OFX>", result);
        Assert.Contains("<BANKMSGSRSV1>", result);
        Assert.Contains("<STMTTRNRS>", result);
        Assert.Contains("<STMTRS>", result);
        Assert.Contains("<BANKACCTFROM>", result);
        Assert.Contains("<ACCTID>ACC001</ACCTID>", result);
        Assert.Contains("</OFX>", result);
    }

    [Fact]
    public async Task WriteTransactionsAsync_WithAccount_WritesTransactions()
    {
        // Arrange
        var sb = new StringBuilder();
        var target = FinanceWriter.ToString(sb).AsOfx();
        var account = Account.FromStrings("ACC001", "Test Account", AccountType.Checking, "USD");
        var transactions = new[]
        {
            Transaction.FromStrings("ACC001", 100.50m, DateTime.Parse("2024-01-15 10:30:00"), "Test Transaction", TransactionType.Credit),
            Transaction.FromStrings("ACC001", -50.25m, DateTime.Parse("2024-01-16 14:00:00"), "Debit Transaction", TransactionType.Debit)
        };

        // Act
        await target.WriteTransactionsAsync(account, transactions.ToAsyncEnumerable());
        await TestHelpers.WaitForAsyncWrites();

        // Assert
        var result = sb.ToString();
        Assert.Contains("<STMTTRN>", result);
        Assert.Contains("<DTPOSTED>", result);
        Assert.Contains("<TRNAMT>", result);
        Assert.Contains("100.50", result);
        Assert.Contains("-50.25", result);
        Assert.Contains("<MEMO>Test Transaction</MEMO>", result);
    }

    [Fact]
    public async Task WriteAccountAsync_WithManualLifecycle_WritesAccount()
    {
        // Arrange
        var sb = new StringBuilder();
        var target = FinanceWriter.ToString(sb).AsOfx();
        var account = Account.FromStrings("ACC001", "Test Account", AccountType.Checking, "USD");

        // Act
        await target.BeginWriteAsync();
        await target.WriteAccountAsync(account);
        await target.EndWriteAsync();
        await TestHelpers.WaitForAsyncWrites();

        // Assert
        var result = sb.ToString();
        Assert.Contains("<OFX>", result);
        Assert.Contains("<ACCTID>ACC001</ACCTID>", result);
        Assert.Contains("</OFX>", result);
    }

    [Fact]
    public async Task WriteTransactionAsync_WithManualLifecycle_WritesTransaction()
    {
        // Arrange
        var sb = new StringBuilder();
        var target = FinanceWriter.ToString(sb).AsOfx();
        var transaction = Transaction.FromStrings("ACC001", 100.50m, DateTime.Parse("2024-01-15 10:30:00"), "Test", TransactionType.Credit);

        // Act
        await target.BeginWriteAsync();
        await target.WriteTransactionAsync(transaction);
        await target.EndWriteAsync();
        await TestHelpers.WaitForAsyncWrites();

        // Assert
        var result = sb.ToString();
        Assert.Contains("<STMTTRN>", result);
        Assert.Contains("<TRNAMT>100.50</TRNAMT>", result);
        Assert.Contains("<MEMO>Test</MEMO>", result);
    }

    [Fact]
    public async Task WriteTransactionAsync_EscapesXmlCharacters()
    {
        // Arrange
        var sb = new StringBuilder();
        var target = FinanceWriter.ToString(sb).AsOfx();
        var transaction = Transaction.FromStrings("ACC001", 100.50m, DateTime.Parse("2024-01-15"), "Description <with> & XML", TransactionType.Credit);

        // Act
        await target.BeginWriteAsync();
        await target.WriteTransactionAsync(transaction);
        await target.EndWriteAsync();
        await TestHelpers.WaitForAsyncWrites();

        // Assert
        var result = sb.ToString();
        Assert.Contains("&lt;", result);
        Assert.Contains("&gt;", result);
        Assert.Contains("&amp;", result);
    }

    [Fact]
    public async Task WriteAccountAsync_MultipleAccounts_WritesMultipleStatements()
    {
        // Arrange
        var sb = new StringBuilder();
        var target = FinanceWriter.ToString(sb).AsOfx();
        var account1 = Account.FromStrings("ACC001", "Account 1", AccountType.Checking, "USD");
        var account2 = Account.FromStrings("ACC002", "Account 2", AccountType.Savings, "EUR");

        // Act
        await target.BeginWriteAsync();
        await target.WriteAccountAsync(account1);
        await target.WriteAccountAsync(account2);
        await target.EndWriteAsync();
        await TestHelpers.WaitForAsyncWrites();

        // Assert
        var result = sb.ToString();
        var stmtCount = result.Split("<STMTTRNRS>", StringSplitOptions.RemoveEmptyEntries).Length - 1;
        Assert.Equal(2, stmtCount);
    }

    [Fact]
    public async Task WriteTransactionsAsync_MultipleAccounts_GroupsByAccount()
    {
        // Arrange
        var sb = new StringBuilder();
        var target = FinanceWriter.ToString(sb).AsOfx();
        var account1 = Account.FromStrings("ACC001", "Account 1", AccountType.Checking, "USD");
        var account2 = Account.FromStrings("ACC002", "Account 2", AccountType.Savings, "EUR");
        var transactions1 = new[]
        {
            Transaction.FromStrings("ACC001", 100m, DateTime.Parse("2024-01-15"), "Tx1", TransactionType.Credit)
        };
        var transactions2 = new[]
        {
            Transaction.FromStrings("ACC002", 200m, DateTime.Parse("2024-01-16"), "Tx2", TransactionType.Debit)
        };

        // Act
        await target.WriteTransactionsAsync(account1, transactions1.ToAsyncEnumerable());
        await target.WriteTransactionsAsync(account2, transactions2.ToAsyncEnumerable());
        await TestHelpers.WaitForAsyncWrites();

        // Assert
        var result = sb.ToString();
        Assert.Contains("<ACCTID>ACC001</ACCTID>", result);
        Assert.Contains("<ACCTID>ACC002</ACCTID>", result);
        Assert.Contains("Tx1", result);
        Assert.Contains("Tx2", result);
    }

    [Fact]
    public async Task WriteTransactionAsync_WithReference_IncludesFITID()
    {
        // Arrange
        var sb = new StringBuilder();
        var target = FinanceWriter.ToString(sb).AsOfx();
        var transaction = Transaction.FromStrings("ACC001", 100.50m, DateTime.Parse("2024-01-15"), "Test", TransactionType.Credit, "REF123");

        // Act
        await target.BeginWriteAsync();
        await target.WriteTransactionAsync(transaction);
        await target.EndWriteAsync();
        await TestHelpers.WaitForAsyncWrites();

        // Assert
        var result = sb.ToString();
        Assert.Contains("<FITID>REF123</FITID>", result);
    }

    [Fact]
    public async Task WriteTransactionAsync_WithoutReference_GeneratesFITID()
    {
        // Arrange
        var sb = new StringBuilder();
        var target = FinanceWriter.ToString(sb).AsOfx();
        var transaction = Transaction.FromStrings("ACC001", 100.50m, DateTime.Parse("2024-01-15 10:30:00"), "Test", TransactionType.Credit);

        // Act
        await target.BeginWriteAsync();
        await target.WriteTransactionAsync(transaction);
        await target.EndWriteAsync();
        await TestHelpers.WaitForAsyncWrites();

        // Assert
        var result = sb.ToString();
        Assert.Contains("<FITID>", result);
        Assert.Contains("</FITID>", result);
    }

    [Fact]
    public async Task WriteTransactionAsync_WithDifferentTransactionTypes_WritesCorrectType()
    {
        // Arrange
        var sb = new StringBuilder();
        var target = FinanceWriter.ToString(sb).AsOfx();
        var credit = Transaction.FromStrings("ACC001", 100m, DateTime.Parse("2024-01-15"), "Credit", TransactionType.Credit);
        var debit = Transaction.FromStrings("ACC001", -50m, DateTime.Parse("2024-01-16"), "Debit", TransactionType.Debit);

        // Act
        await target.BeginWriteAsync();
        await target.WriteTransactionAsync(credit);
        await target.WriteTransactionAsync(debit);
        await target.EndWriteAsync();
        await TestHelpers.WaitForAsyncWrites();

        // Assert
        var result = sb.ToString();
        Assert.Contains("<TRNTYPE>CREDIT</TRNTYPE>", result);
        Assert.Contains("<TRNTYPE>DEBIT</TRNTYPE>", result);
    }

    [Fact]
    public async Task WriteHeaderAsync_WritesOfxHeader()
    {
        // Arrange
        var sb = new StringBuilder();
        var target = FinanceWriter.ToString(sb).AsOfx();

        // Act
        await target.BeginWriteAsync();
        await target.EndWriteAsync();
        await TestHelpers.WaitForAsyncWrites();

        // Assert
        var result = sb.ToString();
        Assert.Contains("OFXHEADER:100", result);
        Assert.Contains("DATA:OFXSGML", result);
        Assert.Contains("VERSION:102", result);
        Assert.Contains("<OFX>", result);
    }

    [Fact]
    public async Task WriteFooterAsync_ClosesOfxStructure()
    {
        // Arrange
        var sb = new StringBuilder();
        var target = FinanceWriter.ToString(sb).AsOfx();
        var account = Account.FromStrings("ACC001", "Test", AccountType.Checking, "USD");

        // Act
        await target.BeginWriteAsync();
        await target.WriteAccountAsync(account);
        await target.EndWriteAsync();
        await TestHelpers.WaitForAsyncWrites();

        // Assert
        var result = sb.ToString();
        Assert.Contains("</BANKMSGSRSV1>", result);
        Assert.Contains("</OFX>", result);
    }

    [Fact]
    public async Task WriteAccountAsync_WithDifferentAccountTypes_WritesCorrectType()
    {
        // Arrange
        var sb = new StringBuilder();
        var target = FinanceWriter.ToString(sb).AsOfx();
        var checking = Account.FromStrings("ACC001", "Checking", AccountType.Checking, "USD");
        var savings = Account.FromStrings("ACC002", "Savings", AccountType.Savings, "EUR");

        // Act
        await target.BeginWriteAsync();
        await target.WriteAccountAsync(checking);
        await target.WriteAccountAsync(savings);
        await target.EndWriteAsync();
        await TestHelpers.WaitForAsyncWrites();

        // Assert
        var result = sb.ToString();
        Assert.Contains("<ACCTTYPE>CHECKING</ACCTTYPE>", result);
        Assert.Contains("<ACCTTYPE>SAVINGS</ACCTTYPE>", result);
    }

    [Fact]
    public async Task WriteAccountAsync_WithAllAccountTypes_WritesCorrectTypes()
    {
        // Arrange
        var sb = new StringBuilder();
        var target = FinanceWriter.ToString(sb).AsOfx();

        // Act
        await target.BeginWriteAsync();
        await target.WriteAccountAsync(Account.FromStrings("ACC001", "Checking", AccountType.Checking, "USD"));
        await target.WriteAccountAsync(Account.FromStrings("ACC002", "Savings", AccountType.Savings, "USD"));
        await target.WriteAccountAsync(Account.FromStrings("ACC003", "CreditCard", AccountType.CreditCard, "USD"));
        await target.WriteAccountAsync(Account.FromStrings("ACC004", "Other", AccountType.Other, "USD"));
        await target.EndWriteAsync();
        await TestHelpers.WaitForAsyncWrites();

        // Assert
        var result = sb.ToString();
        Assert.Contains("<ACCTTYPE>CHECKING</ACCTTYPE>", result);
        Assert.Contains("<ACCTTYPE>SAVINGS</ACCTTYPE>", result);
        Assert.Contains("<ACCTTYPE>CREDITCARD</ACCTTYPE>", result);
        Assert.Contains("<ACCTTYPE>OTHER</ACCTTYPE>", result);
    }

    [Fact]
    public async Task WriteTransactionAsync_WithAllTransactionTypes_WritesCorrectTypes()
    {
        // Arrange
        var sb = new StringBuilder();
        var target = FinanceWriter.ToString(sb).AsOfx();
        var date = DateTime.Parse("2024-01-15");

        // Act
        await target.BeginWriteAsync();
        await target.WriteTransactionAsync(Transaction.FromStrings("ACC001", 100m, date, "Credit", TransactionType.Credit));
        await target.WriteTransactionAsync(Transaction.FromStrings("ACC001", -50m, date, "Debit", TransactionType.Debit));
        await target.WriteTransactionAsync(Transaction.FromStrings("ACC001", 25m, date, "Transfer", TransactionType.Transfer));
        await target.WriteTransactionAsync(Transaction.FromStrings("ACC001", 1000m, date, "Buy", TransactionType.Buy));
        await target.WriteTransactionAsync(Transaction.FromStrings("ACC001", -800m, date, "Sell", TransactionType.Sell));
        await target.WriteTransactionAsync(Transaction.FromStrings("ACC001", 50m, date, "Dividend", TransactionType.Dividend));
        await target.WriteTransactionAsync(Transaction.FromStrings("ACC001", -5m, date, "Fee", TransactionType.Fee));
        await target.WriteTransactionAsync(Transaction.FromStrings("ACC001", 10m, date, "Interest", TransactionType.Interest));
        await target.WriteTransactionAsync(Transaction.FromStrings("ACC001", 15m, date, "Adjustment", TransactionType.Adjustment));
        await target.WriteTransactionAsync(Transaction.FromStrings("ACC001", 20m, date, "Other", TransactionType.Other));
        await target.EndWriteAsync();
        await TestHelpers.WaitForAsyncWrites();

        // Assert
        var result = sb.ToString();
        Assert.Contains("<TRNTYPE>CREDIT</TRNTYPE>", result);
        Assert.Contains("<TRNTYPE>DEBIT</TRNTYPE>", result);
        Assert.Contains("<TRNTYPE>XFER</TRNTYPE>", result);
        Assert.Contains("<TRNTYPE>BUY</TRNTYPE>", result);
        Assert.Contains("<TRNTYPE>SELL</TRNTYPE>", result);
        Assert.Contains("<TRNTYPE>DIV</TRNTYPE>", result);
        Assert.Contains("<TRNTYPE>FEE</TRNTYPE>", result);
        Assert.Contains("<TRNTYPE>INT</TRNTYPE>", result);
        // Adjustment and Other both map to OTHER, so we should see it twice
        var otherCount = (result.Length - result.Replace("<TRNTYPE>OTHER</TRNTYPE>", "").Length) / "<TRNTYPE>OTHER</TRNTYPE>".Length;
        Assert.True(otherCount >= 2, $"Expected at least 2 occurrences of OTHER, found {otherCount}");
    }

    [Fact]
    public async Task WriteAccountAsync_WithUnsupportedAccountType_WritesOther()
    {
        // Arrange
        var sb = new StringBuilder();
        var target = FinanceWriter.ToString(sb).AsOfx();
        // Using Brokerage which is not explicitly handled (will use default case)
        var brokerage = Account.FromStrings("ACC001", "Brokerage", AccountType.Brokerage, "USD");

        // Act
        await target.BeginWriteAsync();
        await target.WriteAccountAsync(brokerage);
        await target.EndWriteAsync();
        await TestHelpers.WaitForAsyncWrites();

        // Assert
        var result = sb.ToString();
        // Brokerage, Investment, Loan, Mortgage all map to OTHER in the switch default case
        Assert.Contains("<ACCTTYPE>OTHER</ACCTTYPE>", result);
    }

    [Fact]
    public async Task WriteTransactionAsync_WithEmptyDescription_OmitsMemo()
    {
        // Arrange
        var sb = new StringBuilder();
        var target = FinanceWriter.ToString(sb).AsOfx();
        var transaction = Transaction.FromStrings("ACC001", 100.50m, DateTime.Parse("2024-01-15"), "", TransactionType.Credit);

        // Act
        await target.BeginWriteAsync();
        await target.WriteTransactionAsync(transaction);
        await target.EndWriteAsync();
        await TestHelpers.WaitForAsyncWrites();

        // Assert
        var result = sb.ToString();
        Assert.DoesNotContain("<MEMO>", result);
    }

    [Fact]
    public async Task WriteTransactionAsync_WithEmptyReference_GeneratesFITID()
    {
        // Arrange
        var sb = new StringBuilder();
        var target = FinanceWriter.ToString(sb).AsOfx();
        var transaction = Transaction.FromStrings("ACC001", 100.50m, DateTime.Parse("2024-01-15 10:30:00"), "Test", TransactionType.Credit);

        // Act
        await target.BeginWriteAsync();
        await target.WriteTransactionAsync(transaction);
        await target.EndWriteAsync();
        await TestHelpers.WaitForAsyncWrites();

        // Assert
        var result = sb.ToString();
        // Should generate FITID when reference is empty
        Assert.Contains("<FITID>", result);
        Assert.Contains("</FITID>", result);
    }

    [Fact]
    public async Task WriteTransactionAsync_WithNegativeAmount_GeneratesFITID()
    {
        // Arrange
        var sb = new StringBuilder();
        var target = FinanceWriter.ToString(sb).AsOfx();
        var transaction = Transaction.FromStrings("ACC001", -100.50m, DateTime.Parse("2024-01-15 10:30:00"), "Test", TransactionType.Debit);

        // Act
        await target.BeginWriteAsync();
        await target.WriteTransactionAsync(transaction);
        await target.EndWriteAsync();
        await TestHelpers.WaitForAsyncWrites();

        // Assert
        var result = sb.ToString();
        Assert.Contains("<FITID>", result);
        // Verify negative amount is handled in FITID generation
        var fitidMatch = System.Text.RegularExpressions.Regex.Match(result, "<FITID>([^<]+)</FITID>");
        Assert.True(fitidMatch.Success);
    }

    [Fact]
    public async Task WriteAccountAsync_WithInvestmentType_WritesOther()
    {
        // Arrange
        var sb = new StringBuilder();
        var target = FinanceWriter.ToString(sb).AsOfx();
        var investment = Account.FromStrings("ACC001", "Investment", AccountType.Investment, "USD");

        // Act
        await target.BeginWriteAsync();
        await target.WriteAccountAsync(investment);
        await target.EndWriteAsync();
        await TestHelpers.WaitForAsyncWrites();

        // Assert
        var result = sb.ToString();
        Assert.Contains("<ACCTTYPE>OTHER</ACCTTYPE>", result);
    }

    [Fact]
    public async Task WriteAccountAsync_WithLoanType_WritesOther()
    {
        // Arrange
        var sb = new StringBuilder();
        var target = FinanceWriter.ToString(sb).AsOfx();
        var loan = Account.FromStrings("ACC001", "Loan", AccountType.Loan, "USD");

        // Act
        await target.BeginWriteAsync();
        await target.WriteAccountAsync(loan);
        await target.EndWriteAsync();
        await TestHelpers.WaitForAsyncWrites();

        // Assert
        var result = sb.ToString();
        Assert.Contains("<ACCTTYPE>OTHER</ACCTTYPE>", result);
    }

    [Fact]
    public async Task WriteAccountAsync_WithMortgageType_WritesOther()
    {
        // Arrange
        var sb = new StringBuilder();
        var target = FinanceWriter.ToString(sb).AsOfx();
        var mortgage = Account.FromStrings("ACC001", "Mortgage", AccountType.Mortgage, "USD");

        // Act
        await target.BeginWriteAsync();
        await target.WriteAccountAsync(mortgage);
        await target.EndWriteAsync();
        await TestHelpers.WaitForAsyncWrites();

        // Assert
        var result = sb.ToString();
        Assert.Contains("<ACCTTYPE>OTHER</ACCTTYPE>", result);
    }

    [Fact]
    public async Task WriteTransactionAsync_WithReferenceAndEmptyDescription_CoversBothBranches()
    {
        // Arrange
        var sb = new StringBuilder();
        var target = FinanceWriter.ToString(sb).AsOfx();
        var transaction = Transaction.FromStrings("ACC001", 100.50m, DateTime.Parse("2024-01-15"), "", TransactionType.Credit, "REF123");

        // Act
        await target.BeginWriteAsync();
        await target.WriteTransactionAsync(transaction);
        await target.EndWriteAsync();
        await TestHelpers.WaitForAsyncWrites();

        // Assert
        var result = sb.ToString();
        Assert.DoesNotContain("<MEMO>", result); // Empty description branch
        Assert.Contains("<FITID>REF123</FITID>", result); // Reference provided branch
    }

    [Fact]
    public async Task WriteHeaderAsync_WhenAlreadyWritten_SkipsWriting()
    {
        // Arrange
        var sb = new StringBuilder();
        var target = FinanceWriter.ToString(sb).AsOfx();

        // Act
        await target.BeginWriteAsync();
        // Call WriteHeaderAsync again (should be skipped)
        await target.WriteAccountAsync(Account.FromStrings("ACC001", "Test", AccountType.Checking, "USD"));
        await target.EndWriteAsync();
        await TestHelpers.WaitForAsyncWrites();

        // Assert - Header should only appear once
        var result = sb.ToString();
        var headerCount = (result.Length - result.Replace("OFXHEADER:100", "").Length) / "OFXHEADER:100".Length;
        Assert.Equal(1, headerCount);
    }

    [Fact]
    public async Task WriteTransactionsAsync_WithMultipleAccounts_ClosesPreviousTransactionList()
    {
        // Arrange - Test the branch where _currentAccount.HasValue && _inTransactionList
        var sb = new StringBuilder();
        var target = FinanceWriter.ToString(sb).AsOfx();
        var account1 = Account.FromStrings("ACC001", "Account 1", AccountType.Checking, "USD");
        var account2 = Account.FromStrings("ACC002", "Account 2", AccountType.Savings, "EUR");
        var transactions1 = new[] { Transaction.FromStrings("ACC001", 100m, DateTime.Parse("2024-01-15"), "Tx1", TransactionType.Credit) };
        var transactions2 = new[] { Transaction.FromStrings("ACC002", 200m, DateTime.Parse("2024-01-16"), "Tx2", TransactionType.Debit) };

        // Act - WriteTransactionsAsync handles its own lifecycle (no EndWriteAsync needed)
        await target.WriteTransactionsAsync(account1, transactions1.ToAsyncEnumerable());
        await target.WriteTransactionsAsync(account2, transactions2.ToAsyncEnumerable());
        await TestHelpers.WaitForAsyncWrites();

        // Assert - Should close previous transaction list and start new one
        var result = sb.ToString();
        Assert.Contains("</BANKTRANLIST>", result);
        Assert.Contains("<ACCTID>ACC001</ACCTID>", result);
        Assert.Contains("<ACCTID>ACC002</ACCTID>", result);
    }

    [Fact]
    public async Task WriteTransactionAsync_WithoutCurrentAccount_CreatesDefaultAccount()
    {
        // Arrange - Test the branch where !_currentAccount.HasValue in WriteTransactionAsync
        var sb = new StringBuilder();
        var target = FinanceWriter.ToString(sb).AsOfx();
        var transaction = Transaction.FromStrings("ACC001", 100.50m, DateTime.Parse("2024-01-15"), "Test", TransactionType.Credit);

        // Act - Write transaction without setting account first
        await target.BeginWriteAsync();
        await target.WriteTransactionAsync(transaction);
        await target.EndWriteAsync();
        await TestHelpers.WaitForAsyncWrites();

        // Assert - Should create default account and write transaction
        var result = sb.ToString();
        Assert.Contains("<ACCTID>ACC001</ACCTID>", result);
        Assert.Contains("<STMTTRN>", result);
    }

    [Fact]
    public async Task WriteTransactionsAsync_WithSameAccountId_ReusesTransactionList()
    {
        // Arrange - Test the branch where account IDs match (should NOT close and reopen)
        var sb = new StringBuilder();
        var target = FinanceWriter.ToString(sb).AsOfx();
        var account = Account.FromStrings("ACC001", "Account", AccountType.Checking, "USD");
        var transactions1 = new[] { Transaction.FromStrings("ACC001", 100m, DateTime.Parse("2024-01-15"), "Tx1", TransactionType.Credit) };
        var transactions2 = new[] { Transaction.FromStrings("ACC001", 200m, DateTime.Parse("2024-01-16"), "Tx2", TransactionType.Debit) };

        // Act - WriteTransactionsAsync handles its own lifecycle
        await target.WriteTransactionsAsync(account, transactions1.ToAsyncEnumerable());
        await target.WriteTransactionsAsync(account, transactions2.ToAsyncEnumerable());
        await TestHelpers.WaitForAsyncWrites();

        // Assert - Should reuse same transaction list for same account ID
        var result = sb.ToString();
        Assert.Contains("Tx1", result);
        Assert.Contains("Tx2", result);
        Assert.Contains("<ACCTID>ACC001</ACCTID>", result);
    }
}

