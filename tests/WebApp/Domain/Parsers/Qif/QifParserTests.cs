using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using WebApp.Domain.Parsers;
using Xunit;

namespace Sceny.Finance.WebApp.Tests.Domain.Parsers.Qif;

public class QifParserTests
{
    [Theory]
    [InlineData("Cash", AccountType.Cash)]
    [InlineData("Bank", AccountType.Bank)]
    [InlineData("CCard", AccountType.CreditCard)]
    [InlineData("Invst", AccountType.Investment)]
    [InlineData("Oth A", AccountType.Asset)]
    [InlineData("Oth L", AccountType.Liability)]
    [InlineData("Invoice", AccountType.Invoice)]
    public async Task Account_standard_types(string qifType, AccountType accountType)
    {
        // arrange
        var bankRecord = $@"!Type:{qifType}
^";
        // act
        var doc = QifParser.Create(bankRecord);
        // assert
        var accounts = await doc.GetAccountsAsync().ToArrayAsync();
        accounts.Should().HaveCount(1);
        accounts[0].Should().NotBeNull();
        accounts[0].Type.Should().Be(accountType);
        (await accounts[0].GetItemsAsync().ShouldAsync()).BeEmpty();
    }

    [Fact]
    public async Task Simple_quicken_account()
    {
        // arrange
        const string bankRecord = @"!Account
NMy credit card
TCCard
DAccount Description
^
!Type:CCard
^";
        // act
        var doc = QifParser.Create(bankRecord);
        // assert
        var accounts = await doc.GetAccountsAsync().ToArrayAsync();
        accounts.Should().HaveCount(1);
        var account = accounts[0];
        account.Should().NotBeNull();
        account.Type.Should().Be(AccountType.CreditCard);
        account.Name.Should().Be("My credit card");
        account.Description.Should().Be("Account Description");
        (await account.GetItemsAsync().ShouldAsync()).BeEmpty();
    }

    [Fact]
    public async Task Multiple_standard_accounts()
    {
        // arrange
        const string bankRecord = @"!Type:CCard
^
!Type:Cash
^";
        // act
        var doc = QifParser.Create(bankRecord);
        // assert
        var accounts = await doc.GetAccountsAsync().ToArrayAsync();
        accounts.Should().HaveCount(2);
        await AssertAccountAsync(accounts[0], AccountType.CreditCard);
        await AssertAccountAsync(accounts[1], AccountType.Cash);
        static async Task AssertAccountAsync(Account account, AccountType type)
        {
            account.Should().NotBeNull();
            account.Type.Should().Be(type);
            (await account.GetItemsAsync().ShouldAsync()).BeEmpty();
        }
    }

    [Fact]
    public async Task Multiple_quicken_accounts()
    {
        // arrange
        const string bankRecord = @"!Account
NMy credit card
TCCard
DAccount Description
^
!Type:CCard
^
!Account
NMy banking
TBank
DBanking description
^
!Type:Bank
^";
        // act
        var doc = QifParser.Create(bankRecord);
        // assert
        var accounts = await doc.GetAccountsAsync().ToArrayAsync();
        accounts.Should().HaveCount(2);
        await AssertAccountAsync(
            accounts[0],
            "My credit card",
            AccountType.CreditCard,
            "Account Description");
        await AssertAccountAsync(
            accounts[1],
            "My banking",
            AccountType.Bank,
            "Banking description");
        static async Task AssertAccountAsync(
            Account account,
            string name,
            AccountType type,
            string description)
        {
            account.Should().NotBeNull();
            account.Name.Should().Be(name);
            account.Type.Should().Be(type);
            account.Description.Should().Be(description);
            (await account.GetItemsAsync().ShouldAsync()).BeEmpty();
        }
    }

    // TODO: !Type:Cat
    // TODO: !Type:Class
    // TODO: !Type:Memorized

    [Fact]
    public async Task Simple_single_record_parsing()
    {
        // arrange
        const string @record = @"!Type:Bank
D03/04/10
T-378.12
PCITY OF SPRINGFIELD
^";
        // act
        var doc = QifParser.Create(@record);
        var item = await GetSingleItemAssertedAsync(doc, AccountType.Bank);
        item.Date.Should().Be(new DateTime(2010, 03, 04));
        item.Amount.Should().Be(-378.12);
        item.Payee.Should().Be("CITY OF SPRINGFIELD");
    }

    [Fact(Skip = "Calling .ToArrayAsync will process all accounts, and do not fill the items/transactions for each account.")]
    public async Task Simple_single_record_parsing_with_ToArray_call_on_the_account()
    {
        // arrange
        const string @record = @"!Type:Bank
D03/04/10
T-378.12
PCITY OF SPRINGFIELD
^";
        // act
        var doc = QifParser.Create(@record);

        // account
        var accounts = await doc.GetAccountsAsync().ToArrayAsync();
        accounts.Should().HaveCount(1);
        var account = accounts[0];
        account.Should().NotBeNull();
        account.Type.Should().Be(AccountType.Bank);
        // item
        var items = await account.GetItemsAsync().ToArrayAsync();
        items.Should().HaveCount(1);
        var item = items[0];
        item.Date.Should().Be(new DateTime(2010, 03, 04));
        item.Amount.Should().Be(-378.12);
        item.Payee.Should().Be("CITY OF SPRINGFIELD");
    }

    [Fact]
    public async Task Simple_multiple_record_parsing()
    {
        // arrange
        const string @record = @"!Type:Bank
D03/03/10
T-379.00
PCITY OF SPRINGFIELD
^
D03/04/10
T-20.28
PYOUR LOCAL SUPERMARKET
^
D07/11/20
T-421.35
PSPRINGFIELD WATER UTILITY
^";
        // act
        var doc = QifParser.Create(@record);
        // assert

        var accountsEnumerator = doc.GetAccountsAsync().GetAsyncEnumerator(default);
        (await accountsEnumerator.MoveNextAsync()).Should().BeTrue();

        var itemsEnumerator = accountsEnumerator.Current.GetItemsAsync().GetAsyncEnumerator(default);

        (await itemsEnumerator.MoveNextAsync()).Should().BeTrue();
        AssertItem(
            itemsEnumerator.Current,
            new DateTime(2010, 3, 3),
            -379.0,
            "CITY OF SPRINGFIELD");

        (await itemsEnumerator.MoveNextAsync()).Should().BeTrue();
        AssertItem(
            itemsEnumerator.Current,
            new DateTime(2010, 3, 4),
            -20.28,
            "YOUR LOCAL SUPERMARKET");

        (await itemsEnumerator.MoveNextAsync()).Should().BeTrue();
        AssertItem(
            itemsEnumerator.Current,
            new DateTime(2020, 7, 11),
            -421.35,
            "SPRINGFIELD WATER UTILITY");

        (await accountsEnumerator.MoveNextAsync()).Should().BeFalse();

        static void AssertItem(
            AccountItem item,
            DateTime date,
            double amount,
            string payee)
        {
            item.Should().NotBeNull();
            item.Date.Should().Be(date);
            item.Amount.Should().Be(amount);
            item.Payee.Should().Be(payee);
        }
    }

    [Theory]
    [InlineData("25 December 2006", 2006, 12, 25)]
    [InlineData("3/4/10", 2010, 03, 04)]
    [InlineData("3/4/2010", 2010, 03, 04)]
    [InlineData("03/4/2010", 2010, 03, 04)]
    [InlineData("3/04/2010", 2010, 03, 04)]
    public async Task Single_record_D_parsing(
        string date,
        int expectedYear,
        int expectedMonth,
        int expectedDay)
    {
        // arrange
        var @record = $@"!Type:Bank
D{date}
^";
        var doc = QifParser.Create(@record);

        // assert
        var item = await GetSingleItemAssertedAsync(doc, AccountType.Bank);
        item.Date.Should().Be(new DateTime(expectedYear, expectedMonth, expectedDay));
    }

    private static async Task<AccountItem> GetSingleItemAssertedAsync(Document doc, AccountType type)
    {
        AccountItem? item = null;
        var accountsRead = 0;
        await foreach (var account in doc.GetAccountsAsync())
        {
            (++accountsRead).Should().Be(1);
            // account
            account.Should().NotBeNull();
            account.Type.Should().Be(AccountType.Bank);
            // items
            var items = await account.GetItemsAsync().ToArrayAsync();
            items.Should().HaveCount(1);
            item = items[0];
        }
        item.Should().NotBeNull();
        return item ?? throw new NullReferenceException(nameof(item));
    }

    [Theory]
    [InlineData("25 DECEMBER 2006", 2006, 12, 25)]
    [InlineData("25 Dec 2006", 2006, 12, 25)]
    [InlineData("12/25'6", 2006, 12, 25)]
    [InlineData("12/25 '6", 2006, 12, 25)]
    [InlineData("12/25'12", 2012, 12, 25)]
    [InlineData("12/25 '12", 2012, 12, 25)]
    [InlineData("12/25'2022", 2022, 12, 25)]
    [InlineData("12/25 '2022", 2022, 12, 25)]
    [InlineData("3/4/10", 2010, 03, 04)]
    [InlineData("3/4/2010", 2010, 03, 04)]
    [InlineData("03/4/2010", 2010, 03, 04)]
    [InlineData("3/04/2010", 2010, 03, 04)]
    public void Qif_dates_values_parsing(
        string date,
        int expectedYear,
        int expectedMonth,
        int expectedDay)
    {
        // arrange
        var valueParser = QifParser.ValueParser;
        // act
        var result = valueParser.TryParseDate(date, out var parsedDate);
        // assert
        result.Should().BeTrue();
        parsedDate.Should().Be(new DateTime(expectedYear, expectedMonth, expectedDay));
    }
}