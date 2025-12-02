using Sceny.Finance.IO;
using Sceny.Finance.IO.Plugin.File.Csv;

namespace Sceny.Finance.IO.Tests;

public class TransactionTests
{
    [Fact]
    public void Transaction_FromStrings_CreatesTransaction()
    {
        // Arrange
        var accountId = "ACC001";
        var amount = 100.50m;
        var date = new DateTime(2024, 1, 15);
        var description = "Test Transaction";
        var type = TransactionType.Credit;
        var reference = "REF123";
        var properties = default(CsvTransactionProperties);

        // Act
        var transaction = Transaction<CsvTransactionProperties>.FromStrings(accountId, amount, date, description, type, properties, reference);

        // Assert
        Assert.Equal(accountId, transaction.AccountId);
        Assert.Equal(amount, transaction.Amount);
        Assert.Equal(date, transaction.Date);
        Assert.Equal(description, transaction.Description);
        Assert.Equal(type, transaction.Type);
        Assert.Equal(reference, transaction.Reference);
    }

    [Fact]
    public void Transaction_Constructor_WithReadOnlyString_CreatesTransaction()
    {
        // Arrange
        var accountId = "ACC001";
        var amount = -50.25m;
        var date = new DateTime(2024, 2, 20);
        var description = "Debit Transaction";
        var type = TransactionType.Debit;
        var properties = default(CsvTransactionProperties);

        // Act
        var transaction = new Transaction<CsvTransactionProperties>(accountId, amount, date, description, type, properties);

        // Assert
        Assert.Equal("ACC001", transaction.AccountId);
        Assert.Equal(-50.25m, transaction.Amount);
        Assert.Equal(new DateTime(2024, 2, 20), transaction.Date);
        Assert.Equal("Debit Transaction", transaction.Description);
        Assert.Equal(TransactionType.Debit, transaction.Type);
    }

    [Fact]
    public void Transaction_ImplicitConversion_Works()
    {
        // Arrange
        ReadOnlyMemory<char> accountIdMemory = "ACC001".AsMemory();
        var properties = default(CsvTransactionProperties);

        // Act
        var transaction = new Transaction<CsvTransactionProperties>(accountIdMemory, 100m, DateTime.Now, "Test", TransactionType.Credit, properties);

        // Assert - implicit conversion from ReadOnlyMemory<char> to ReadOnlyString
        Assert.Equal("ACC001", transaction.AccountId);
    }

    [Theory]
    [InlineData(TransactionType.Debit)]
    [InlineData(TransactionType.Credit)]
    [InlineData(TransactionType.Transfer)]
    [InlineData(TransactionType.Buy)]
    [InlineData(TransactionType.Sell)]
    [InlineData(TransactionType.Dividend)]
    [InlineData(TransactionType.Fee)]
    [InlineData(TransactionType.Interest)]
    [InlineData(TransactionType.Adjustment)]
    [InlineData(TransactionType.Other)]
    public void Transaction_SupportsAllTransactionTypes(TransactionType transactionType)
    {
        // Act
        var transaction = Transaction<CsvTransactionProperties>.FromStrings("ACC001", 100m, DateTime.Now, "Test", transactionType, default(CsvTransactionProperties));

        // Assert
        Assert.Equal(transactionType, transaction.Type);
    }

    [Fact]
    public void Transaction_FromStrings_WithNullReference_CreatesTransaction()
    {
        // Act
        var transaction = Transaction<CsvTransactionProperties>.FromStrings("ACC001", 100m, DateTime.Now, "Test", TransactionType.Credit, default(CsvTransactionProperties), null);

        // Assert
        Assert.Equal(string.Empty, transaction.Reference);
    }
}
