using Sceny.Finance.IO;

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

        // Act
        var account = Account.FromStrings(id, name, type, currency);

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

        // Act
        var account = new Account(id, name, type, currency);

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

        // Act
        var account = new Account(idMemory, "Name", AccountType.Checking, "USD");

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
        var account = Account.FromStrings("ID", "Name", accountType, "USD");

        // Assert
        Assert.Equal(accountType, account.Type);
    }
}
