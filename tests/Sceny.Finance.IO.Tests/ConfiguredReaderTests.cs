using System.Linq;
using System.Text;
using Sceny.Finance.IO;
using Sceny.Finance.IO.Plugin.File.Csv;
using Sceny.Finance.IO.Sources;

namespace Sceny.Finance.IO.Tests;

public class ConfiguredReaderTests
{
    [Fact]
    public void Constructor_WithValidSourceAndReader_DoesNotThrow()
    {
        // Arrange
        var source = new StringSource("test");
        var reader = new CsvSourceReader();

        // Act
        var configuredReader = new ConfiguredReader(source, reader);

        // Assert
        Assert.NotNull(configuredReader);
    }

    [Fact]
    public void Constructor_WithNullSource_ThrowsArgumentNullException()
    {
        // Arrange
        var reader = new CsvSourceReader();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new ConfiguredReader(null!, reader));
    }

    [Fact]
    public void Constructor_WithNullReader_ThrowsArgumentNullException()
    {
        // Arrange
        var source = new StringSource("test");

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new ConfiguredReader(source, null!));
    }

    [Fact]
    public async Task GetAccountsAsync_WithStringSource_ReturnsAccounts()
    {
        // Arrange
        var source = new StringSource("AccountId,AccountName\nACC001,Test");
        var reader = new CsvSourceReader();
        var configuredReader = new ConfiguredReader(source, reader);

        // Act
        var accounts = await configuredReader.GetAccountsAsync().ToListAsync();

        // Assert
        Assert.Single(accounts);
        Assert.Equal("ACC001", accounts[0].Id);
    }

    [Fact]
    public async Task GetAccountsAsync_WithMemorySource_ReturnsAccounts()
    {
        // Arrange
        var data = Encoding.UTF8.GetBytes("AccountId,AccountName\nACC001,Test").AsMemory();
        var source = new MemorySource(data);
        var reader = new CsvSourceReader();
        var configuredReader = new ConfiguredReader(source, reader);

        // Act
        var accounts = await configuredReader.GetAccountsAsync().ToListAsync();

        // Assert
        Assert.Single(accounts);
        Assert.Equal("ACC001", accounts[0].Id);
    }

    [Fact]
    public async Task GetTransactionsAsync_WithStringSource_ReturnsTransactions()
    {
        // Arrange
        var source = new StringSource("Date,Amount,Description\n2024-01-15,100.50,Test");
        var reader = new CsvSourceReader();
        var configuredReader = new ConfiguredReader(source, reader);
        var account = Account.FromStrings("ACC001", "Test", AccountType.Checking, "USD");

        // Act
        var transactions = await configuredReader.GetTransactionsAsync(account).ToListAsync();

        // Assert
        Assert.Single(transactions);
        Assert.Equal(100.50m, transactions[0].Amount);
    }

    [Fact]
    public async Task GetTransactionsAsync_WithMemorySource_ReturnsTransactions()
    {
        // Arrange
        var data = Encoding.UTF8.GetBytes("Date,Amount\n2024-01-15,100.50").AsMemory();
        var source = new MemorySource(data);
        var reader = new CsvSourceReader();
        var configuredReader = new ConfiguredReader(source, reader);
        var account = Account.FromStrings("ACC001", "Test", AccountType.Checking, "USD");

        // Act
        var transactions = await configuredReader.GetTransactionsAsync(account).ToListAsync();

        // Assert
        Assert.Single(transactions);
        Assert.Equal(100.50m, transactions[0].Amount);
    }

    [Fact]
    public async Task GetAccountsAsync_WithCancellation_Throws()
    {
        // Arrange
        var source = new StringSource("AccountId,AccountName\nACC001,Test");
        var reader = new CsvSourceReader();
        var configuredReader = new ConfiguredReader(source, reader);
        var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act & Assert
        await Assert.ThrowsAnyAsync<OperationCanceledException>(async () =>
        {
            await configuredReader.GetAccountsAsync(cts.Token).ToListAsync();
        });
    }

    [Fact]
    public async Task GetTransactionsAsync_WithCancellation_Throws()
    {
        // Arrange
        var source = new StringSource("Date,Amount\n2024-01-15,100.50");
        var reader = new CsvSourceReader();
        var configuredReader = new ConfiguredReader(source, reader);
        var account = Account.FromStrings("ACC001", "Test", AccountType.Checking, "USD");
        var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act & Assert
        await Assert.ThrowsAnyAsync<OperationCanceledException>(async () =>
        {
            await configuredReader.GetTransactionsAsync(account, cts.Token).ToListAsync();
        });
    }
}

