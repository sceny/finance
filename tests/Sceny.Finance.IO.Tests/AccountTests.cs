using Sceny.Finance.IO;
using Sceny.Finance.IO.Plugin.File.Csv;

namespace Sceny.Finance.IO.Tests;

public class AccountTests
{
    [Fact]
    public void Account_FromStrings_CreatesAccount()
    {
        // Arrange
        var id = "ACC001";
        var name = "Checking Account";
        var type = AccountType.Checking;
        var currency = "USD";
        var properties = default(CsvAccountProperties);

        // Act
        var account = Account<CsvAccountProperties>.FromStrings(id, name, type, currency, properties);

        // Assert
        Assert.Equal(id, account.Id);
        Assert.Equal(name, account.Name);
        Assert.Equal(type, account.Type);
        Assert.Equal(currency, account.Currency);
    }

    [Fact]
    public void Account_Constructor_WithReadOnlyString_CreatesAccount()
    {
        // Arrange
        var id = "ACC001";
        var name = "Savings Account";
        var type = AccountType.Savings;
        var currency = "EUR";
        var properties = default(CsvAccountProperties);

        // Act
        var account = new Account<CsvAccountProperties>(id, name, type, currency, properties);

        // Assert
        Assert.Equal("ACC001", account.Id);
        Assert.Equal("Savings Account", account.Name);
        Assert.Equal(AccountType.Savings, account.Type);
        Assert.Equal("EUR", account.Currency);
    }

    [Fact]
    public void Account_ImplicitConversion_Works()
    {
        // Arrange
        ReadOnlyMemory<char> idMemory = "ACC001".AsMemory();
        var properties = default(CsvAccountProperties);

        // Act
        var account = new Account<CsvAccountProperties>(idMemory, "Name", AccountType.Checking, "USD", properties);

        // Assert - implicit conversion from ReadOnlyMemory<char> to ReadOnlyString
        Assert.Equal("ACC001", account.Id);
    }

    [Theory]
    [InlineData(AccountType.Checking)]
    [InlineData(AccountType.Savings)]
    [InlineData(AccountType.CreditCard)]
    [InlineData(AccountType.Brokerage)]
    [InlineData(AccountType.Investment)]
    [InlineData(AccountType.Loan)]
    [InlineData(AccountType.Mortgage)]
    [InlineData(AccountType.Other)]
    public void Account_SupportsAllAccountTypes(AccountType accountType)
    {
        // Act
        var account = Account<CsvAccountProperties>.FromStrings("ID", "Name", accountType, "USD", default(CsvAccountProperties));

        // Assert
        Assert.Equal(accountType, account.Type);
    }
}
