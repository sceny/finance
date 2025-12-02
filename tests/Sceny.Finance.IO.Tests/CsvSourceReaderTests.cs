using System.Linq;
using System.Text;
using Sceny.Finance.IO;
using Sceny.Finance.IO.Plugin.File.Csv;

namespace Sceny.Finance.IO.Tests;

public class CsvSourceReaderTests
{
    [Fact]
    public async Task GetAccountsAsync_ParsesAccountFromCsv()
    {
        // Arrange
        var csv = "AccountId,AccountName,Type,Currency\nACC001,Checking Account,Checking,USD\n";

        // Act
        var accounts = await FinanceReader.FromString(csv).AsCsv().GetAccountsAsync().ToListAsync();

        // Assert
        Assert.Single(accounts);
        var account = accounts[0];
        Assert.Equal("ACC001", account.Id);
        Assert.Equal("Checking Account", account.Name);
        Assert.Equal(AccountType.Checking, account.Type);
        Assert.Equal("USD", account.Currency);
    }

    [Fact]
    public async Task GetAccountsAsync_WithCustomDateFormat()
    {
        // Arrange
        var csv = "AccountId,Date,Amount\nACC001,2024-01-15,100.50\n";

        // Act
        var accounts = await FinanceReader.FromString(csv).AsCsv(options => options.DateFormat = "yyyy-MM-dd").GetAccountsAsync().ToListAsync();

        // Assert
        Assert.Single(accounts);
        Assert.Equal("ACC001", accounts[0].Id);
    }

    [Fact]
    public async Task GetAccountsAsync_WithCustomDelimiter()
    {
        // Arrange
        var csv = "AccountId;AccountName;Type\nACC001;Savings;Savings\n";

        // Act
        var accounts = await FinanceReader.FromString(csv).AsCsv(options => options.Delimiter = ';').GetAccountsAsync().ToListAsync();

        // Assert
        Assert.Single(accounts);
        Assert.Equal("ACC001", accounts[0].Id);
        Assert.Equal("Savings", accounts[0].Name);
    }

    [Fact]
    public async Task GetTransactionsAsync_ParsesTransactionsFromCsv()
    {
        // Arrange
        var csv = "AccountId,Date,Amount,Description\nACC001,2024-01-15,100.50,Test Transaction\nACC001,2024-01-16,-50.25,Debit\n";
        var account = Account<CsvAccountProperties>.FromStrings("ACC001", "Test Account", AccountType.Checking, "USD", default(CsvAccountProperties));
        // Act
        var transactions = await FinanceReader.FromString(csv).AsCsv().GetTransactionsAsync(account).ToListAsync();

        // Assert
        Assert.Equal(2, transactions.Count);
        Assert.Equal(100.50m, transactions[0].Amount);
        Assert.Equal(-50.25m, transactions[1].Amount);
        Assert.Equal("Test Transaction", transactions[0].Description);
        Assert.Equal("Debit", transactions[1].Description);
    }

    [Fact]
    public async Task GetTransactionsAsync_WithCustomColumnMapping()
    {
        // Arrange
        var csv = "ID,PostDate,Amt,Notes\nACC001,2024-01-15,100.50,Test\n";
        var account = Account<CsvAccountProperties>.FromStrings("ACC001", "Test", AccountType.Checking, "USD", default(CsvAccountProperties));

        // Act
        var transactions = await FinanceReader.FromString(csv).AsCsv(options =>
        {
            options.ColumnMapping["AccountId"] = "ID";
            options.ColumnMapping["Date"] = "PostDate";
            options.ColumnMapping["Amount"] = "Amt";
            options.ColumnMapping["Description"] = "Notes";
        }).GetTransactionsAsync(account).ToListAsync();

        // Assert
        Assert.Single(transactions);
        Assert.Equal(100.50m, transactions[0].Amount);
        Assert.Equal("Test", transactions[0].Description);
    }

    [Fact]
    public async Task GetTransactionsAsync_InfersTransactionTypeFromAmount()
    {
        // Arrange
        var csv = "AccountId,Date,Amount\nACC001,2024-01-15,100.50\nACC001,2024-01-16,-50.25\n";
        var account = Account<CsvAccountProperties>.FromStrings("ACC001", "Test", AccountType.Checking, "USD", default(CsvAccountProperties));
        // Act
        var transactions = await FinanceReader.FromString(csv).AsCsv().GetTransactionsAsync(account).ToListAsync();

        // Assert
        Assert.Equal(2, transactions.Count);
        Assert.Equal(TransactionType.Credit, transactions[0].Type);
        Assert.Equal(TransactionType.Debit, transactions[1].Type);
    }

    [Fact]
    public void CsvSourceReaderBuilder_ConfiguresOptions()
    {
        // Act
        var reader = new CsvSourceReaderBuilder()
            .WithDateFormat("MM/dd/yyyy")
            .WithDelimiter(';')
            .WithDefaultAccountType(AccountType.Savings)
            .MapColumn("Date", "PostDate")
            .Build();

        // Assert - builder should create reader without errors
        Assert.NotNull(reader);
    }

    [Fact]
    public async Task GetAccountsAsync_WithEmptyFile_ReturnsEmpty()
    {
        // Arrange
        var csv = string.Empty;
        // Act
        var accounts = await FinanceReader.FromString(csv).AsCsv().GetAccountsAsync().ToListAsync();

        // Assert
        Assert.Empty(accounts);
    }

    [Fact]
    public async Task GetAccountsAsync_WithNoHeaders_ReturnsEmpty()
    {
        // Arrange
        var csv = "ACC001,Checking Account,Checking,USD\n";

        // Act
        var accounts = await FinanceReader.FromString(csv).AsCsv(options => options.HasHeaders = false).GetAccountsAsync().ToListAsync();

        // Assert
        Assert.Empty(accounts); // No headers means no account parsing
    }

    [Fact]
    public async Task GetAccountsAsync_WithMissingAccountId_ReturnsEmpty()
    {
        // Arrange
        var csv = "AccountName,Type,Currency\nChecking Account,Checking,USD\n";
        // Act
        var accounts = await FinanceReader.FromString(csv).AsCsv().GetAccountsAsync().ToListAsync();

        // Assert
        Assert.Empty(accounts);
    }

    [Fact]
    public async Task GetAccountsAsync_WithEmptyAccountId_ReturnsEmpty()
    {
        // Arrange
        var csv = "AccountId,AccountName,Type\n,Checking Account,Checking\n";
        // Act
        var accounts = await FinanceReader.FromString(csv).AsCsv().GetAccountsAsync().ToListAsync();

        // Assert
        Assert.Empty(accounts);
    }

    [Fact]
    public async Task GetAccountsAsync_WithMissingAccountName_UsesAccountId()
    {
        // Arrange
        var csv = "AccountId,Type,Currency\nACC001,Checking,USD\n";
        // Act
        var accounts = await FinanceReader.FromString(csv).AsCsv().GetAccountsAsync().ToListAsync();

        // Assert
        Assert.Single(accounts);
        Assert.Equal("ACC001", accounts[0].Id);
        Assert.Equal("ACC001", accounts[0].Name); // Should use AccountId when Name is missing
    }

    [Fact]
    public async Task GetAccountsAsync_WithQuotedFields_ParsesCorrectly()
    {
        // Arrange
        var csv = "AccountId,AccountName\n\"ACC001\",\"Checking, Account\"\n";
        // Act
        var accounts = await FinanceReader.FromString(csv).AsCsv().GetAccountsAsync().ToListAsync();

        // Assert
        Assert.Single(accounts);
        Assert.Equal("ACC001", accounts[0].Id);
        Assert.Equal("Checking, Account", accounts[0].Name);
    }

    [Fact]
    public async Task GetAccountsAsync_WithDefaultCurrency_UsesDefault()
    {
        // Arrange
        var csv = "AccountId,AccountName\nACC001,Checking\n";
        // Act
        var accounts = await FinanceReader.FromString(csv).AsCsv().GetAccountsAsync().ToListAsync();

        // Assert
        Assert.Single(accounts);
        Assert.Equal("USD", accounts[0].Currency);
    }

    [Fact]
    public async Task GetAccountsAsync_WithCustomDefaultCurrency_UsesCustom()
    {
        // Arrange
        var csv = "AccountId,AccountName\nACC001,Checking\n";

        // Act
        var accounts = await FinanceReader.FromString(csv).AsCsv(options => options.DefaultCurrency = "EUR").GetAccountsAsync().ToListAsync();

        // Assert
        Assert.Single(accounts);
        Assert.Equal("EUR", accounts[0].Currency);
    }

    [Fact]
    public async Task GetAccountsAsync_WithDefaultAccountType_UsesDefault()
    {
        // Arrange
        var csv = "AccountId,AccountName\nACC001,Account\n";
        // Act
        var accounts = await FinanceReader.FromString(csv).AsCsv().GetAccountsAsync().ToListAsync();

        // Assert
        Assert.Single(accounts);
        Assert.Equal(AccountType.Other, accounts[0].Type);
    }

    [Fact]
    public async Task GetTransactionsAsync_WithEmptyFile_ReturnsEmpty()
    {
        // Arrange
        var csv = string.Empty;
        var account = Account<CsvAccountProperties>.FromStrings("ACC001", "Test", AccountType.Checking, "USD", default(CsvAccountProperties));
        // Act
        var transactions = await FinanceReader.FromString(csv).AsCsv().GetTransactionsAsync(account).ToListAsync();

        // Assert
        Assert.Empty(transactions);
    }

    [Fact]
    public async Task GetTransactionsAsync_WithInvalidDate_SkipsTransaction()
    {
        // Arrange
        var csv = "Date,Amount,Description\nINVALID-DATE,100.50,Test\n";
        var account = Account<CsvAccountProperties>.FromStrings("ACC001", "Test", AccountType.Checking, "USD", default(CsvAccountProperties));
        // Act
        var transactions = await FinanceReader.FromString(csv).AsCsv().GetTransactionsAsync(account).ToListAsync();

        // Assert
        Assert.Empty(transactions);
    }

    [Fact]
    public async Task GetTransactionsAsync_WithInvalidAmount_SkipsTransaction()
    {
        // Arrange
        var csv = "Date,Amount,Description\n2024-01-15,INVALID,Test\n";
        var account = Account<CsvAccountProperties>.FromStrings("ACC001", "Test", AccountType.Checking, "USD", default(CsvAccountProperties));
        // Act
        var transactions = await FinanceReader.FromString(csv).AsCsv().GetTransactionsAsync(account).ToListAsync();

        // Assert
        Assert.Empty(transactions);
    }

    [Fact]
    public async Task GetTransactionsAsync_WithEmptyLines_SkipsEmptyLines()
    {
        // Arrange
        var csv = "Date,Amount,Description\n2024-01-15,100.50,Test\n\n2024-01-16,-50.25,Debit\n";
        var account = Account<CsvAccountProperties>.FromStrings("ACC001", "Test", AccountType.Checking, "USD", default(CsvAccountProperties));
        // Act
        var transactions = await FinanceReader.FromString(csv).AsCsv().GetTransactionsAsync(account).ToListAsync();

        // Assert
        Assert.Equal(2, transactions.Count);
    }

    [Fact]
    public async Task GetTransactionsAsync_WithMissingDescription_UsesEmptyString()
    {
        // Arrange
        var csv = "Date,Amount\n2024-01-15,100.50\n";
        var account = Account<CsvAccountProperties>.FromStrings("ACC001", "Test", AccountType.Checking, "USD", default(CsvAccountProperties));
        // Act
        var transactions = await FinanceReader.FromString(csv).AsCsv().GetTransactionsAsync(account).ToListAsync();

        // Assert
        Assert.Single(transactions);
        Assert.Equal(string.Empty, transactions[0].Description);
    }

    [Fact]
    public async Task GetTransactionsAsync_WithMissingReference_UsesEmptyString()
    {
        // Arrange
        var csv = "Date,Amount,Description\n2024-01-15,100.50,Test\n";
        var account = Account<CsvAccountProperties>.FromStrings("ACC001", "Test", AccountType.Checking, "USD", default(CsvAccountProperties));
        // Act
        var transactions = await FinanceReader.FromString(csv).AsCsv().GetTransactionsAsync(account).ToListAsync();

        // Assert
        Assert.Single(transactions);
        Assert.Equal(string.Empty, transactions[0].Reference);
    }

    [Fact]
    public async Task GetTransactionsAsync_WithExplicitTransactionType_UsesType()
    {
        // Arrange
        var csv = "Date,Amount,Type\n2024-01-15,100.50,Transfer\n";
        var account = Account<CsvAccountProperties>.FromStrings("ACC001", "Test", AccountType.Checking, "USD", default(CsvAccountProperties));
        // Act
        var transactions = await FinanceReader.FromString(csv).AsCsv().GetTransactionsAsync(account).ToListAsync();

        // Assert
        Assert.Single(transactions);
        Assert.Equal(TransactionType.Transfer, transactions[0].Type);
    }

    [Fact]
    public async Task GetTransactionsAsync_WithZeroAmount_InfersCredit()
    {
        // Arrange
        var csv = "Date,Amount\n2024-01-15,0.00\n";
        var account = Account<CsvAccountProperties>.FromStrings("ACC001", "Test", AccountType.Checking, "USD", default(CsvAccountProperties));
        // Act
        var transactions = await FinanceReader.FromString(csv).AsCsv().GetTransactionsAsync(account).ToListAsync();

        // Assert
        Assert.Single(transactions);
        Assert.Equal(TransactionType.Credit, transactions[0].Type);
    }

    [Fact]
    public async Task GetTransactionsAsync_WithTabDelimiter_ParsesCorrectly()
    {
        // Arrange
        var csv = "Date\tAmount\tDescription\n2024-01-15\t100.50\tTest\n";
        var account = Account<CsvAccountProperties>.FromStrings("ACC001", "Test", AccountType.Checking, "USD", default(CsvAccountProperties));

        // Act
        var transactions = await FinanceReader.FromString(csv).AsCsv(options => options.Delimiter = '\t').GetTransactionsAsync(account).ToListAsync();

        // Assert
        Assert.Single(transactions);
        Assert.Equal(100.50m, transactions[0].Amount);
    }

    [Fact]
    public async Task GetTransactionsAsync_WithCancellation_Throws()
    {
        // Arrange
        var csv = "Date,Amount\n2024-01-15,100.50\n";
        var account = Account<CsvAccountProperties>.FromStrings("ACC001", "Test", AccountType.Checking, "USD", default(CsvAccountProperties));
        var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act & Assert
        await Assert.ThrowsAnyAsync<OperationCanceledException>(async () =>
        {
            await FinanceReader.FromString(csv).AsCsv().GetTransactionsAsync(account, cts.Token).ToListAsync();
        });
    }

    [Fact]
    public async Task GetAccountsAsync_WithCancellation_Throws()
    {
        // Arrange
        var csv = "AccountId,AccountName\nACC001,Test\n";
        var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act & Assert
        await Assert.ThrowsAnyAsync<OperationCanceledException>(async () =>
        {
            await FinanceReader.FromString(csv).AsCsv().GetAccountsAsync(cts.Token).ToListAsync();
        });
    }

    [Fact]
    public async Task GetTransactionsAsync_WithWhitespaceInFields_TrimsWhitespace()
    {
        // Arrange
        var csv = "Date,Amount,Description\n  2024-01-15  ,  100.50  ,  Test  \n";
        var account = Account<CsvAccountProperties>.FromStrings("ACC001", "Test", AccountType.Checking, "USD", default(CsvAccountProperties));
        // Act
        var transactions = await FinanceReader.FromString(csv).AsCsv().GetTransactionsAsync(account).ToListAsync();

        // Assert
        Assert.Single(transactions);
        Assert.Equal(100.50m, transactions[0].Amount);
        Assert.Equal("Test", transactions[0].Description);
    }

    [Fact]
    public async Task GetAccountsAsync_WithWhitespaceInFields_TrimsWhitespace()
    {
        // Arrange
        var csv = "AccountId,AccountName\n  ACC001  ,  Test Account  \n";
        // Act
        var accounts = await FinanceReader.FromString(csv).AsCsv().GetAccountsAsync().ToListAsync();

        // Assert
        Assert.Single(accounts);
        Assert.Equal("ACC001", accounts[0].Id);
        Assert.Equal("Test Account", accounts[0].Name);
    }

    // Line Ending Handling
    [Fact]
    public async Task GetAccountsAsync_WithCarriageReturnOnly_HandlesCorrectly()
    {
        // Arrange
        var csv = "AccountId,AccountName\rACC001,Test\r";
        
        // Act
        var accounts = await FinanceReader.FromString(csv).AsCsv().GetAccountsAsync().ToListAsync();

        // Assert
        Assert.Single(accounts);
        Assert.Equal("ACC001", accounts[0].Id);
    }

    [Fact]
    public async Task GetAccountsAsync_WithCarriageReturnLineFeed_HandlesCorrectly()
    {
        // Arrange
        var csv = "AccountId,AccountName\r\nACC001,Test\r\n";
        
        // Act
        var accounts = await FinanceReader.FromString(csv).AsCsv().GetAccountsAsync().ToListAsync();

        // Assert
        Assert.Single(accounts);
        Assert.Equal("ACC001", accounts[0].Id);
    }

    [Fact]
    public async Task GetAccountsAsync_WithMixedLineEndings_HandlesCorrectly()
    {
        // Arrange
        var csv = "AccountId,AccountName\nACC001,Test\r\nACC002,Test2\n";
        
        // Act
        var accounts = await FinanceReader.FromString(csv).AsCsv().GetAccountsAsync().ToListAsync();

        // Assert
        // CSV reader only returns the first account from the first data row
        Assert.Single(accounts);
        Assert.Equal("ACC001", accounts[0].Id);
    }

    // Quoted Field Handling
    [Fact]
    public async Task GetAccountsAsync_WithQuotedFieldAtEnd_HandlesCorrectly()
    {
        // Arrange
        var csv = "AccountId,AccountName\n\"ACC001\",\"Test\"";
        
        // Act
        var accounts = await FinanceReader.FromString(csv).AsCsv().GetAccountsAsync().ToListAsync();

        // Assert
        Assert.Single(accounts);
        Assert.Equal("ACC001", accounts[0].Id);
    }

    [Fact]
    public async Task GetAccountsAsync_WithQuotedFieldAndDelimiterAtEnd_HandlesCorrectly()
    {
        // Arrange
        var csv = "AccountId,AccountName\n\"ACC001\",\"Test,\"";
        
        // Act
        var accounts = await FinanceReader.FromString(csv).AsCsv().GetAccountsAsync().ToListAsync();

        // Assert
        Assert.Single(accounts);
        Assert.Equal("ACC001", accounts[0].Id);
    }

    [Fact]
    public async Task GetAccountsAsync_WithQuotedFieldFollowedByDelimiter_HandlesCorrectly()
    {
        // Arrange
        var csv = "AccountId,AccountName\n\"ACC001\",\"Test\",";
        
        // Act
        var accounts = await FinanceReader.FromString(csv).AsCsv().GetAccountsAsync().ToListAsync();

        // Assert
        Assert.Single(accounts);
        Assert.Equal("ACC001", accounts[0].Id);
    }

    [Fact]
    public async Task GetAccountsAsync_WithQuotedFieldEndingAtLineEnd_HandlesCorrectly()
    {
        // Arrange
        var csv = "AccountId,AccountName\n\"ACC001\",\"Test Account\"";
        
        // Act
        var accounts = await FinanceReader.FromString(csv).AsCsv().GetAccountsAsync().ToListAsync();

        // Assert
        Assert.Single(accounts);
        Assert.Equal("ACC001", accounts[0].Id);
        Assert.Equal("Test Account", accounts[0].Name);
    }

    // Column Mapping
    [Fact]
    public async Task GetAccountsAsync_WithMappedColumnName_UsesMapping()
    {
        // Arrange
        var csv = "ID,Name\nACC001,Test";
        
        // Act
        var accounts = await FinanceReader.FromString(csv).AsCsv(options =>
        {
            options.ColumnMapping["AccountId"] = "ID";
            options.ColumnMapping["AccountName"] = "Name";
        }).GetAccountsAsync().ToListAsync();

        // Assert
        Assert.Single(accounts);
        Assert.Equal("ACC001", accounts[0].Id);
    }

    [Fact]
    public async Task GetAccountsAsync_WithUnmappedColumn_UsesDirectLookup()
    {
        // Arrange
        var csv = "AccountId,AccountName\nACC001,Test";
        
        // Act
        var accounts = await FinanceReader.FromString(csv).AsCsv(options =>
        {
            // Remove AccountId from mapping to force direct lookup
            options.ColumnMapping.Remove("AccountId");
        }).GetAccountsAsync().ToListAsync();

        // Assert
        Assert.Single(accounts);
        Assert.Equal("ACC001", accounts[0].Id);
    }

    [Fact]
    public async Task GetAccountsAsync_WithColumnIndexOutOfRange_HandlesGracefully()
    {
        // Arrange
        var csv = "AccountId\nACC001";
        
        // Act
        var accounts = await FinanceReader.FromString(csv).AsCsv().GetAccountsAsync().ToListAsync();

        // Assert
        // Should handle missing columns gracefully
        Assert.Single(accounts);
    }

    // Account Type Parsing
    [Fact]
    public async Task GetAccountsAsync_WithInvalidAccountType_UsesDefault()
    {
        // Arrange
        var csv = "AccountId,AccountName,Type\nACC001,Test,INVALID";
        
        // Act
        var accounts = await FinanceReader.FromString(csv).AsCsv(options =>
        {
            options.DefaultAccountType = AccountType.Savings;
        }).GetAccountsAsync().ToListAsync();

        // Assert
        Assert.Single(accounts);
        Assert.Equal(AccountType.Savings, accounts[0].Type);
    }

    [Fact]
    public async Task GetAccountsAsync_WithEmptyAccountType_UsesDefault()
    {
        // Arrange
        var csv = "AccountId,AccountName,Type\nACC001,Test,";
        
        // Act
        var accounts = await FinanceReader.FromString(csv).AsCsv(options =>
        {
            options.DefaultAccountType = AccountType.Savings;
        }).GetAccountsAsync().ToListAsync();

        // Assert
        Assert.Single(accounts);
        Assert.Equal(AccountType.Savings, accounts[0].Type);
    }

    [Fact]
    public async Task GetAccountsAsync_WithInvalidAccountTypeEnum_UsesDefault()
    {
        // Arrange
        var csv = "AccountId,AccountName,Type\nACC001,Test,NOTVALID";
        
        // Act
        var accounts = await FinanceReader.FromString(csv).AsCsv(options =>
        {
            options.DefaultAccountType = AccountType.Savings;
        }).GetAccountsAsync().ToListAsync();

        // Assert
        Assert.Single(accounts);
        Assert.Equal(AccountType.Savings, accounts[0].Type);
    }

    // Transaction Type Inference
    [Fact]
    public async Task GetTransactionsAsync_WithInvalidTransactionType_UsesAmountInference()
    {
        // Arrange
        var csv = "Date,Amount,Type\n2024-01-15,100.50,INVALID";
        var account = Account<CsvAccountProperties>.FromStrings("ACC001", "Test", AccountType.Checking, "USD", default(CsvAccountProperties));
        
        // Act
        var transactions = await FinanceReader.FromString(csv).AsCsv().GetTransactionsAsync(account).ToListAsync();

        // Assert
        Assert.Single(transactions);
        Assert.Equal(TransactionType.Credit, transactions[0].Type);
    }

    [Fact]
    public async Task GetTransactionsAsync_WithEmptyTransactionType_UsesAmountInference()
    {
        // Arrange
        var csv = "Date,Amount,Type\n2024-01-15,100.50,";
        var account = Account<CsvAccountProperties>.FromStrings("ACC001", "Test", AccountType.Checking, "USD", default(CsvAccountProperties));
        
        // Act
        var transactions = await FinanceReader.FromString(csv).AsCsv().GetTransactionsAsync(account).ToListAsync();

        // Assert
        Assert.Single(transactions);
        Assert.Equal(TransactionType.Credit, transactions[0].Type);
    }

    [Fact]
    public async Task GetTransactionsAsync_WithInvalidTransactionTypeEnum_UsesAmountInference()
    {
        // Arrange
        var csv = "Date,Amount,Type\n2024-01-15,-50.25,NOTVALID";
        var account = Account<CsvAccountProperties>.FromStrings("ACC001", "Test", AccountType.Checking, "USD", default(CsvAccountProperties));
        
        // Act
        var transactions = await FinanceReader.FromString(csv).AsCsv().GetTransactionsAsync(account).ToListAsync();

        // Assert
        Assert.Single(transactions);
        Assert.Equal(TransactionType.Debit, transactions[0].Type);
    }

    [Fact]
    public void CsvSourceReaderBuilder_WithAllOptions_ConfiguresCorrectly()
    {
        // Act
        var reader = new CsvSourceReaderBuilder()
            .WithDateFormat("MM/dd/yyyy")
            .WithDelimiter(';')
            .WithEncoding(Encoding.UTF8)
            .WithHeaders(true)
            .WithDefaultAccountType(AccountType.Savings)
            .WithDefaultCurrency("EUR")
            .MapColumn("Date", "PostDate")
            .MapColumn("Amount", "Amt")
            .Build();

        // Assert - builder should create reader without errors
        Assert.NotNull(reader);
    }

}

