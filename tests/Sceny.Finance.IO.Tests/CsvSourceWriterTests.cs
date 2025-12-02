using System.Linq;
using System.Text;
using Sceny.Finance.IO;
using Sceny.Finance.IO.Plugin.File.Csv;

namespace Sceny.Finance.IO.Tests;

public class CsvSourceWriterTests
{
    [Fact]
    public async Task WriteAccountsAsync_WithDefaultOptions_WritesHeaderAndAccounts()
    {
        // Arrange
        var sb = new StringBuilder();
        var target = FinanceWriter.ToString(sb).AsCsv();
        var accounts = new[]
        {
            Account.FromStrings("ACC001", "Checking Account", AccountType.Checking, "USD")
        };

        // Act
        await target.WriteAccountsAsync(accounts.ToAsyncEnumerable());
        await TestHelpers.WaitForAsyncWrites();

        // Assert
        var result = sb.ToString();
        Assert.Contains("AccountId", result);
        Assert.Contains("AccountName", result);
        Assert.Contains("AccountType", result);
        Assert.Contains("Currency", result);
        Assert.Contains("ACC001", result);
        Assert.Contains("Checking Account", result);
        Assert.Contains("Checking", result);
        Assert.Contains("USD", result);
    }

    [Fact]
    public async Task WriteAccountsAsync_WithoutHeaders_DoesNotWriteHeader()
    {
        // Arrange
        var sb = new StringBuilder();
        var target = FinanceWriter.ToString(sb).AsCsv(options => options.HasHeaders = false);
        var accounts = new[]
        {
            Account.FromStrings("ACC001", "Test", AccountType.Checking, "USD")
        };

        // Act
        await target.WriteAccountsAsync(accounts.ToAsyncEnumerable());
        await TestHelpers.WaitForAsyncWrites();

        // Assert
        var result = sb.ToString();
        Assert.DoesNotContain("AccountId", result);
        Assert.Contains("ACC001", result);
    }

    [Fact]
    public async Task WriteHeaderAsync_WithHasHeadersFalse_SkipsHeader()
    {
        // Arrange - Test the !_options.HasHeaders branch in WriteHeaderAsync
        var sb = new StringBuilder();
        var target = FinanceWriter.ToString(sb).AsCsv(options => options.HasHeaders = false);
        var account = Account.FromStrings("ACC001", "Test", AccountType.Checking, "USD");

        // Act
        await target.BeginWriteAsync();
        await target.WriteAccountAsync(account); // This will call WriteHeaderAsync internally
        await target.EndWriteAsync();
        await TestHelpers.WaitForAsyncWrites();

        // Assert - Header should not be written when HasHeaders is false
        var result = sb.ToString();
        Assert.DoesNotContain("AccountId", result);
        Assert.Contains("ACC001", result);
    }

    [Fact]
    public async Task WriteTransactionAsync_WithHasHeadersFalse_SkipsHeader()
    {
        // Arrange - Test the !_options.HasHeaders branch when writing transactions
        var sb = new StringBuilder();
        var target = FinanceWriter.ToString(sb).AsCsv(options => options.HasHeaders = false);
        var transaction = Transaction.FromStrings("ACC001", 100.50m, DateTime.Parse("2024-01-15"), "Test", TransactionType.Credit);

        // Act
        await target.BeginWriteAsync();
        await target.WriteTransactionAsync(transaction); // This will call WriteHeaderAsync internally
        await target.EndWriteAsync();
        await TestHelpers.WaitForAsyncWrites();

        // Assert - Header should not be written when HasHeaders is false
        var result = sb.ToString();
        Assert.DoesNotContain("AccountId", result);
        Assert.Contains("ACC001", result);
    }

    [Fact]
    public async Task WriteTransactionsAsync_WithDefaultOptions_WritesHeaderAndTransactions()
    {
        // Arrange
        var sb = new StringBuilder();
        var target = FinanceWriter.ToString(sb).AsCsv();
        var account = Account.FromStrings("ACC001", "Test", AccountType.Checking, "USD");
        var transactions = new[]
        {
            Transaction.FromStrings("ACC001", 100.50m, DateTime.Parse("2024-01-15"), "Test Transaction", TransactionType.Credit),
            Transaction.FromStrings("ACC001", -50.25m, DateTime.Parse("2024-01-16"), "Debit Transaction", TransactionType.Debit)
        };

        // Act
        await target.WriteTransactionsAsync(account, transactions.ToAsyncEnumerable());
        await TestHelpers.WaitForAsyncWrites();

        // Assert
        var result = sb.ToString();
        Assert.Contains("AccountId", result);
        Assert.Contains("Date", result);
        Assert.Contains("Amount", result);
        Assert.Contains("Description", result);
        Assert.Contains("Type", result);
        Assert.Contains("Reference", result);
        Assert.Contains("ACC001", result);
        Assert.Contains("100.50", result);
        Assert.Contains("-50.25", result);
        Assert.Contains("Test Transaction", result);
    }

    [Fact]
    public async Task WriteAccountAsync_WithManualLifecycle_WritesAccount()
    {
        // Arrange
        var sb = new StringBuilder();
        var target = FinanceWriter.ToString(sb).AsCsv();
        var account = Account.FromStrings("ACC001", "Test Account", AccountType.Checking, "USD");

        // Act
        await target.BeginWriteAsync();
        await target.WriteAccountAsync(account);
        await target.EndWriteAsync();
        await TestHelpers.WaitForAsyncWrites();

        // Assert
        var result = sb.ToString();
        Assert.Contains("AccountId", result);
        Assert.Contains("ACC001", result);
    }

    [Fact]
    public async Task WriteTransactionAsync_WithManualLifecycle_WritesTransaction()
    {
        // Arrange
        var sb = new StringBuilder();
        var target = FinanceWriter.ToString(sb).AsCsv();
        var transaction = Transaction.FromStrings("ACC001", 100.50m, DateTime.Parse("2024-01-15"), "Test", TransactionType.Credit);

        // Act
        await target.BeginWriteAsync();
        await target.WriteTransactionAsync(transaction);
        await target.EndWriteAsync();
        await TestHelpers.WaitForAsyncWrites();

        // Assert
        var result = sb.ToString();
        Assert.Contains("AccountId", result);
        Assert.Contains("Date", result);
        Assert.Contains("100.50", result);
    }

    [Fact]
    public async Task WriteAccountAsync_EscapesCommas()
    {
        // Arrange
        var sb = new StringBuilder();
        var target = FinanceWriter.ToString(sb).AsCsv();
        var account = Account.FromStrings("ACC001", "Account, With, Commas", AccountType.Checking, "USD");

        // Act
        await target.BeginWriteAsync();
        await target.WriteAccountAsync(account);
        await target.EndWriteAsync();
        await TestHelpers.WaitForAsyncWrites();

        // Assert
        var result = sb.ToString();
        Assert.Contains("\"Account, With, Commas\"", result);
    }

    [Fact]
    public async Task WriteTransactionAsync_EscapesQuotes()
    {
        // Arrange
        var sb = new StringBuilder();
        var target = FinanceWriter.ToString(sb).AsCsv();
        var transaction = Transaction.FromStrings("ACC001", 100.50m, DateTime.Parse("2024-01-15"), "Description \"with\" quotes", TransactionType.Credit);

        // Act
        await target.BeginWriteAsync();
        await target.WriteTransactionAsync(transaction);
        await target.EndWriteAsync();
        await TestHelpers.WaitForAsyncWrites();

        // Assert
        var result = sb.ToString();
        Assert.Contains("\"Description \"\"with\"\" quotes\"", result);
    }

    [Fact]
    public async Task WriteTransactionAsync_EscapesNewlines()
    {
        // Arrange
        var sb = new StringBuilder();
        var target = FinanceWriter.ToString(sb).AsCsv();
        var transaction = Transaction.FromStrings("ACC001", 100.50m, DateTime.Parse("2024-01-15"), "Description\nwith\nnewlines", TransactionType.Credit);

        // Act
        await target.BeginWriteAsync();
        await target.WriteTransactionAsync(transaction);
        await target.EndWriteAsync();
        await TestHelpers.WaitForAsyncWrites();

        // Assert
        var result = sb.ToString();
        Assert.Contains("\"Description", result);
        Assert.Contains("newlines\"", result);
    }

    [Fact]
    public async Task WriteAccountAsync_WithCustomDelimiter_UsesDelimiter()
    {
        // Arrange
        var sb = new StringBuilder();
        var target = FinanceWriter.ToString(sb).AsCsv(options => options.Delimiter = ';');
        var account = Account.FromStrings("ACC001", "Test", AccountType.Checking, "USD");

        // Act
        await target.BeginWriteAsync();
        await target.WriteAccountAsync(account);
        await target.EndWriteAsync();
        await TestHelpers.WaitForAsyncWrites();

        // Assert
        var result = sb.ToString();
        Assert.Contains(";", result); // Verify semicolon delimiter is used
    }

    [Fact]
    public async Task WriteAccountAsync_WithCustomDateFormat_UsesFormat()
    {
        // Arrange
        var sb = new StringBuilder();
        var target = FinanceWriter.ToString(sb).AsCsv(options => options.DateFormat = "MM/dd/yyyy");
        var transaction = Transaction.FromStrings("ACC001", 100.50m, DateTime.Parse("2024-01-15"), "Test", TransactionType.Credit);

        // Act
        await target.BeginWriteAsync();
        await target.WriteTransactionAsync(transaction);
        await target.EndWriteAsync();
        await TestHelpers.WaitForAsyncWrites();

        // Assert
        var result = sb.ToString();
        Assert.Contains("01/15/2024", result);
    }

    [Fact]
    public async Task WriteHeaderAsync_CalledTwice_WritesOnlyOnce()
    {
        // Arrange
        var sb = new StringBuilder();
        var target = FinanceWriter.ToString(sb).AsCsv();
        var account = Account.FromStrings("ACC001", "Test", AccountType.Checking, "USD");

        // Act
        await target.BeginWriteAsync();
        await target.WriteAccountAsync(account);
        await target.WriteAccountAsync(account);
        await target.EndWriteAsync();
        await TestHelpers.WaitForAsyncWrites();

        // Assert
        var result = sb.ToString();
        var headerCount = result.Split('\r', '\n', StringSplitOptions.RemoveEmptyEntries)
            .Count(line => line.Contains("AccountId"));
        Assert.Equal(1, headerCount); // Only one header line
    }

    [Fact]
    public async Task WriteFooterAsync_ForCsv_DoesNothing()
    {
        // Arrange
        var sb = new StringBuilder();
        var target = FinanceWriter.ToString(sb).AsCsv();
        var account = Account.FromStrings("ACC001", "Test", AccountType.Checking, "USD");

        // Act
        await target.BeginWriteAsync();
        await target.WriteAccountAsync(account);
        await TestHelpers.WaitForAsyncWrites();
        var beforeFooter = sb.Length;
        await target.EndWriteAsync();
        await TestHelpers.WaitForAsyncWrites();
        var afterFooter = sb.Length;

        // Assert - Footer doesn't add anything for CSV
        Assert.Equal(beforeFooter, afterFooter);
    }

    [Fact]
    public async Task WriteAccountsAsync_MultipleAccounts_WritesAll()
    {
        // Arrange
        var sb = new StringBuilder();
        var target = FinanceWriter.ToString(sb).AsCsv();
        var accounts = new[]
        {
            Account.FromStrings("ACC001", "Account 1", AccountType.Checking, "USD"),
            Account.FromStrings("ACC002", "Account 2", AccountType.Savings, "EUR")
        };

        // Act
        await target.WriteAccountsAsync(accounts.ToAsyncEnumerable());
        await TestHelpers.WaitForAsyncWrites();

        // Assert
        var result = sb.ToString();
        Assert.Contains("ACC001", result);
        Assert.Contains("ACC002", result);
        Assert.Contains("Account 1", result);
        Assert.Contains("Account 2", result);
    }
}

