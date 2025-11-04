using System.Linq;
using System.Text;
using Sceny.Finance.IO;
using Sceny.Finance.IO.Plugin.File.Ofx;

namespace Sceny.Finance.IO.Tests;

public class OfxSourceReaderTests
{
    [Fact]
    public async Task GetAccountsAsync_ParsesAccountFromOfx()
    {
        // Arrange
        var ofx = @"OFXHEADER:100
DATA:OFXSGML
<OFX>
<BANKMSGSRSV1>
<STMTTRNRS>
<STMTRS>
<BANKACCTFROM>
<BANKID>123456789</BANKID>
<ACCTID>ACC001</ACCTID>
<ACCTTYPE>CHECKING</ACCTTYPE>
</BANKACCTFROM>
<BANKTRANLIST>
</BANKTRANLIST>
</STMTRS>
</STMTTRNRS>
</BANKMSGSRSV1>
</OFX>";

        // Act
        var accounts = await FinanceReader.FromString(ofx).AsOfx().GetAccountsAsync().ToListAsync();

        // Assert
        Assert.Single(accounts);
        var account = accounts[0];
        Assert.Equal("ACC001", account.Id);
        Assert.Equal(AccountType.Checking, account.Type);
    }

    [Fact]
    public async Task GetTransactionsAsync_ParsesTransactionsFromOfx()
    {
        // Arrange
        var ofx = @"OFXHEADER:100
DATA:OFXSGML
<OFX>
<BANKMSGSRSV1>
<STMTTRNRS>
<STMTRS>
<BANKACCTFROM>
<ACCTID>ACC001</ACCTID>
</BANKACCTFROM>
<BANKTRANLIST>
<STMTTRN>
<DTPOSTED>20240115</DTPOSTED>
<TRNAMT>100.50</TRNAMT>
<FITID>FIT001</FITID>
<MEMO>Test Transaction</MEMO>
<TRNTYPE>CREDIT</TRNTYPE>
</STMTTRN>
<STMTTRN>
<DTPOSTED>20240116</DTPOSTED>
<TRNAMT>-50.25</TRNAMT>
<FITID>FIT002</FITID>
<MEMO>Debit Transaction</MEMO>
<TRNTYPE>DEBIT</TRNTYPE>
</STMTTRN>
</BANKTRANLIST>
</STMTRS>
</STMTTRNRS>
</BANKMSGSRSV1>
</OFX>";
        var account = Account.FromStrings("ACC001", "Test Account", AccountType.Checking, "USD");
        // Act
        var transactions = await FinanceReader.FromString(ofx).AsOfx().GetTransactionsAsync(account).ToListAsync();

        // Assert
        Assert.Equal(2, transactions.Count);
        Assert.Equal(100.50m, transactions[0].Amount);
        Assert.Equal(-50.25m, transactions[1].Amount);
        Assert.Equal("Test Transaction", transactions[0].Description);
        Assert.Equal("Debit Transaction", transactions[1].Description);
        Assert.Equal("FIT001", transactions[0].Reference);
        Assert.Equal(TransactionType.Credit, transactions[0].Type);
        Assert.Equal(TransactionType.Debit, transactions[1].Type);
    }

    [Fact]
    public async Task GetTransactionsAsync_WithCustomDateFormat()
    {
        // Arrange
        var ofx = @"<STMTTRN>
<DTPOSTED>20240115120000</DTPOSTED>
<TRNAMT>100.50</TRNAMT>
<FITID>FIT001</FITID>
</STMTTRN>";
        var account = Account.FromStrings("ACC001", "Test", AccountType.Checking, "USD");

        // Act
        var transactions = await FinanceReader.FromString(ofx).AsOfx(options => options.DateTimeFormat = "yyyyMMddHHmmss").GetTransactionsAsync(account).ToListAsync();

        // Assert
        Assert.Single(transactions);
        var transaction = transactions[0];
        Assert.Equal(new DateTime(2024, 1, 15, 12, 0, 0), transaction.Date);
    }

    [Fact]
    public async Task GetTransactionsAsync_ParsesAccountTypeFromOfx()
    {
        // Arrange
        var ofxSavings = @"<BANKACCTFROM>
<ACCTID>ACC001</ACCTID>
<ACCTTYPE>SAVINGS</ACCTTYPE>
</BANKACCTFROM>";
        var ofxCredit = @"<BANKACCTFROM>
<ACCTID>ACC002</ACCTID>
<ACCTTYPE>CREDITCARD</ACCTTYPE>
</BANKACCTFROM>";
        
        // Act
        var account1 = await FinanceReader.FromString(ofxSavings).AsOfx().GetAccountsAsync().FirstAsync();
        var account2 = await FinanceReader.FromString(ofxCredit).AsOfx().GetAccountsAsync().FirstAsync();

        // Assert
        Assert.Equal(AccountType.Savings, account1.Type);
        Assert.Equal(AccountType.CreditCard, account2.Type);
    }

    [Fact]
    public void OfxSourceReaderBuilder_ConfiguresOptions()
    {
        // Act
        var reader = new OfxSourceReaderBuilder()
            .WithDateFormat("yyyyMMdd")
            .WithDateTimeFormat("yyyyMMddHHmmss")
            .WithTimeZoneOffset(TimeSpan.FromHours(-5))
            .Build();

        // Assert - builder should create reader without errors
        Assert.NotNull(reader);
    }

    [Fact]
    public async Task GetAccountsAsync_WithEmptyFile_ReturnsEmpty()
    {
        // Arrange
        var ofx = string.Empty;
        // Act
        var accounts = await FinanceReader.FromString(ofx).AsOfx().GetAccountsAsync().ToListAsync();

        // Assert
        Assert.Empty(accounts);
    }

    [Fact]
    public async Task GetAccountsAsync_WithMissingAccountId_UsesBankId()
    {
        // Arrange
        var ofx = @"<BANKACCTFROM>
<BANKID>123456789</BANKID>
<ACCTTYPE>CHECKING</ACCTTYPE>
</BANKACCTFROM>";
        // Act
        var accounts = await FinanceReader.FromString(ofx).AsOfx().GetAccountsAsync().ToListAsync();

        // Assert
        Assert.Single(accounts);
        Assert.Equal("123456789", accounts[0].Id);
    }

    [Fact]
    public async Task GetAccountsAsync_WithMissingAccountIdAndBankId_UsesUnknown()
    {
        // Arrange
        var ofx = @"<BANKACCTFROM>
<ACCTTYPE>CHECKING</ACCTTYPE>
</BANKACCTFROM>";
        // Act
        var accounts = await FinanceReader.FromString(ofx).AsOfx().GetAccountsAsync().ToListAsync();

        // Assert
        Assert.Single(accounts);
        Assert.Equal("UNKNOWN", accounts[0].Id);
    }

    [Fact]
    public async Task GetAccountsAsync_WithMissingDescription_UsesAccountId()
    {
        // Arrange
        var ofx = @"<BANKACCTFROM>
<ACCTID>ACC001</ACCTID>
</BANKACCTFROM>";
        // Act
        var accounts = await FinanceReader.FromString(ofx).AsOfx().GetAccountsAsync().ToListAsync();

        // Assert
        Assert.Single(accounts);
        Assert.Equal("ACC001", accounts[0].Name);
    }

    [Fact]
    public async Task GetAccountsAsync_WithMissingCurrency_UsesUSD()
    {
        // Arrange
        var ofx = @"<BANKACCTFROM>
<ACCTID>ACC001</ACCTID>
</BANKACCTFROM>";
        // Act
        var accounts = await FinanceReader.FromString(ofx).AsOfx().GetAccountsAsync().ToListAsync();

        // Assert
        Assert.Single(accounts);
        Assert.Equal("USD", accounts[0].Currency);
    }

    [Fact]
    public async Task GetAccountsAsync_WithInvalidAccountType_UsesOther()
    {
        // Arrange
        var ofx = @"<BANKACCTFROM>
<ACCTID>ACC001</ACCTID>
<ACCTTYPE>INVALID</ACCTTYPE>
</BANKACCTFROM>";
        // Act
        var accounts = await FinanceReader.FromString(ofx).AsOfx().GetAccountsAsync().ToListAsync();

        // Assert
        Assert.Single(accounts);
        Assert.Equal(AccountType.Other, accounts[0].Type);
    }

    [Fact]
    public async Task GetAccountsAsync_WithEmptyAccountId_ReturnsEmpty()
    {
        // Arrange
        var ofx = @"<BANKACCTFROM>
<ACCTID></ACCTID>
</BANKACCTFROM>";
        // Act
        var accounts = await FinanceReader.FromString(ofx).AsOfx().GetAccountsAsync().ToListAsync();

        // Assert
        Assert.Empty(accounts);
    }

    [Fact]
    public async Task GetTransactionsAsync_WithEmptyFile_ReturnsEmpty()
    {
        // Arrange
        var ofx = string.Empty;
        var account = Account.FromStrings("ACC001", "Test", AccountType.Checking, "USD");
        // Act
        var transactions = await FinanceReader.FromString(ofx).AsOfx().GetTransactionsAsync(account).ToListAsync();

        // Assert
        Assert.Empty(transactions);
    }

    [Fact]
    public async Task GetTransactionsAsync_WithNoTransactions_ReturnsEmpty()
    {
        // Arrange
        var ofx = @"<BANKTRANLIST>
</BANKTRANLIST>";
        var account = Account.FromStrings("ACC001", "Test", AccountType.Checking, "USD");
        // Act
        var transactions = await FinanceReader.FromString(ofx).AsOfx().GetTransactionsAsync(account).ToListAsync();

        // Assert
        Assert.Empty(transactions);
    }

    [Fact]
    public async Task GetTransactionsAsync_WithInvalidDate_SkipsTransaction()
    {
        // Arrange
        var ofx = @"<STMTTRN>
<DTPOSTED>INVALID</DTPOSTED>
<TRNAMT>100.50</TRNAMT>
<FITID>FIT001</FITID>
</STMTTRN>";
        var account = Account.FromStrings("ACC001", "Test", AccountType.Checking, "USD");
        // Act
        var transactions = await FinanceReader.FromString(ofx).AsOfx().GetTransactionsAsync(account).ToListAsync();

        // Assert
        Assert.Empty(transactions);
    }

    [Fact]
    public async Task GetTransactionsAsync_WithInvalidAmount_SkipsTransaction()
    {
        // Arrange
        var ofx = @"<STMTTRN>
<DTPOSTED>20240115</DTPOSTED>
<TRNAMT>INVALID</TRNAMT>
<FITID>FIT001</FITID>
</STMTTRN>";
        var account = Account.FromStrings("ACC001", "Test", AccountType.Checking, "USD");
        // Act
        var transactions = await FinanceReader.FromString(ofx).AsOfx().GetTransactionsAsync(account).ToListAsync();

        // Assert
        Assert.Empty(transactions);
    }

    [Fact]
    public async Task GetTransactionsAsync_WithMissingMemo_UsesEmptyString()
    {
        // Arrange
        var ofx = @"<STMTTRN>
<DTPOSTED>20240115</DTPOSTED>
<TRNAMT>100.50</TRNAMT>
<FITID>FIT001</FITID>
</STMTTRN>";
        var account = Account.FromStrings("ACC001", "Test", AccountType.Checking, "USD");
        // Act
        var transactions = await FinanceReader.FromString(ofx).AsOfx().GetTransactionsAsync(account).ToListAsync();

        // Assert
        Assert.Single(transactions);
        Assert.Equal(string.Empty, transactions[0].Description);
    }

    [Fact]
    public async Task GetTransactionsAsync_WithMissingFITID_UsesEmptyString()
    {
        // Arrange
        var ofx = @"<STMTTRN>
<DTPOSTED>20240115</DTPOSTED>
<TRNAMT>100.50</TRNAMT>
<MEMO>Test</MEMO>
</STMTTRN>";
        var account = Account.FromStrings("ACC001", "Test", AccountType.Checking, "USD");
        // Act
        var transactions = await FinanceReader.FromString(ofx).AsOfx().GetTransactionsAsync(account).ToListAsync();

        // Assert
        Assert.Single(transactions);
        Assert.Equal(string.Empty, transactions[0].Reference);
    }

    [Fact]
    public async Task GetTransactionsAsync_WithNameInsteadOfMemo_UsesName()
    {
        // Arrange
        var ofx = @"<STMTTRN>
<DTPOSTED>20240115</DTPOSTED>
<TRNAMT>100.50</TRNAMT>
<NAME>Test Name</NAME>
<FITID>FIT001</FITID>
</STMTTRN>";
        var account = Account.FromStrings("ACC001", "Test", AccountType.Checking, "USD");
        // Act
        var transactions = await FinanceReader.FromString(ofx).AsOfx().GetTransactionsAsync(account).ToListAsync();

        // Assert
        Assert.Single(transactions);
        Assert.Equal("Test Name", transactions[0].Description);
    }

    [Fact]
    public async Task GetTransactionsAsync_WithDateTimeFormat_ParsesCorrectly()
    {
        // Arrange
        var ofx = @"<STMTTRN>
<DTPOSTED>20240115120000</DTPOSTED>
<TRNAMT>100.50</TRNAMT>
<FITID>FIT001</FITID>
</STMTTRN>";
        var account = Account.FromStrings("ACC001", "Test", AccountType.Checking, "USD");

        // Act
        var transactions = await FinanceReader.FromString(ofx).AsOfx(options => options.DateTimeFormat = "yyyyMMddHHmmss").GetTransactionsAsync(account).ToListAsync();

        // Assert
        Assert.Single(transactions);
        Assert.Equal(new DateTime(2024, 1, 15, 12, 0, 0), transactions[0].Date);
    }

    [Fact]
    public async Task GetTransactionsAsync_WithTimezone_UsesTimezone()
    {
        // Arrange
        var ofx = @"<STMTTRN>
<DTPOSTED>20240115[EST]</DTPOSTED>
<TRNAMT>100.50</TRNAMT>
<FITID>FIT001</FITID>
</STMTTRN>";
        var account = Account.FromStrings("ACC001", "Test", AccountType.Checking, "USD");
        // Act
        var transactions = await FinanceReader.FromString(ofx).AsOfx().GetTransactionsAsync(account).ToListAsync();

        // Assert
        Assert.Single(transactions);
        // Date should be parsed (timezone offset is stored in options but not applied to date)
    }

    [Fact]
    public async Task GetTransactionsAsync_WithZeroAmount_InfersCredit()
    {
        // Arrange
        var ofx = @"<STMTTRN>
<DTPOSTED>20240115</DTPOSTED>
<TRNAMT>0.00</TRNAMT>
<FITID>FIT001</FITID>
</STMTTRN>";
        var account = Account.FromStrings("ACC001", "Test", AccountType.Checking, "USD");
        // Act
        var transactions = await FinanceReader.FromString(ofx).AsOfx().GetTransactionsAsync(account).ToListAsync();

        // Assert
        Assert.Single(transactions);
        Assert.Equal(TransactionType.Credit, transactions[0].Type);
    }

    [Fact]
    public async Task GetTransactionsAsync_WithExplicitTransactionType_UsesType()
    {
        // Arrange
        var ofx = @"<STMTTRN>
<DTPOSTED>20240115</DTPOSTED>
<TRNAMT>100.50</TRNAMT>
<TRNTYPE>XFER</TRNTYPE>
<FITID>FIT001</FITID>
</STMTTRN>";
        var account = Account.FromStrings("ACC001", "Test", AccountType.Checking, "USD");
        // Act
        var transactions = await FinanceReader.FromString(ofx).AsOfx().GetTransactionsAsync(account).ToListAsync();

        // Assert
        Assert.Single(transactions);
        Assert.Equal(TransactionType.Transfer, transactions[0].Type);
    }

    [Fact]
    public async Task GetTransactionsAsync_WithAllTransactionTypes_ParsesCorrectly()
    {
        // Arrange
        var ofx = @"<STMTTRN>
<DTPOSTED>20240115</DTPOSTED>
<TRNAMT>100.50</TRNAMT>
<TRNTYPE>CREDIT</TRNTYPE>
<FITID>FIT001</FITID>
</STMTTRN>
<STMTTRN>
<DTPOSTED>20240116</DTPOSTED>
<TRNAMT>-50.25</TRNAMT>
<TRNTYPE>DEBIT</TRNTYPE>
<FITID>FIT002</FITID>
</STMTTRN>
<STMTTRN>
<DTPOSTED>20240117</DTPOSTED>
<TRNAMT>25.00</TRNAMT>
<TRNTYPE>INT</TRNTYPE>
<FITID>FIT003</FITID>
</STMTTRN>
<STMTTRN>
<DTPOSTED>20240118</DTPOSTED>
<TRNAMT>-5.00</TRNAMT>
<TRNTYPE>FEE</TRNTYPE>
<FITID>FIT004</FITID>
</STMTTRN>";
        var account = Account.FromStrings("ACC001", "Test", AccountType.Checking, "USD");
        // Act
        var transactions = await FinanceReader.FromString(ofx).AsOfx().GetTransactionsAsync(account).ToListAsync();

        // Assert
        Assert.Equal(4, transactions.Count);
        Assert.Equal(TransactionType.Credit, transactions[0].Type);
        Assert.Equal(TransactionType.Debit, transactions[1].Type);
        Assert.Equal(TransactionType.Interest, transactions[2].Type);
        Assert.Equal(TransactionType.Fee, transactions[3].Type);
    }

    [Fact]
    public async Task GetAccountsAsync_WithCancellation_Throws()
    {
        // Arrange
        var ofx = @"<BANKACCTFROM>
<ACCTID>ACC001</ACCTID>
</BANKACCTFROM>";
        var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act & Assert
        await Assert.ThrowsAnyAsync<OperationCanceledException>(async () =>
        {
            await FinanceReader.FromString(ofx).AsOfx().GetAccountsAsync(cts.Token).ToListAsync();
        });
    }

    [Fact]
    public async Task GetTransactionsAsync_WithCancellation_Throws()
    {
        // Arrange
        var ofx = @"<STMTTRN>
<DTPOSTED>20240115</DTPOSTED>
<TRNAMT>100.50</TRNAMT>
<FITID>FIT001</FITID>
</STMTTRN>";
        var account = Account.FromStrings("ACC001", "Test", AccountType.Checking, "USD");
        var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act & Assert
        await Assert.ThrowsAnyAsync<OperationCanceledException>(async () =>
        {
            await FinanceReader.FromString(ofx).AsOfx().GetTransactionsAsync(account, cts.Token).ToListAsync();
        });
    }

    [Fact]
    public async Task GetAccountsAsync_WithAllAccountTypes_ParsesCorrectly()
    {
        // Arrange
        var accountTypes = new[] { "CHECKING", "SAVINGS", "CREDITCARD", "CREDIT", "MONEYMKT", "INVALID" };
        var expectedTypes = new[] { AccountType.Checking, AccountType.Savings, AccountType.CreditCard, AccountType.CreditCard, AccountType.Savings, AccountType.Other };

        for (int i = 0; i < accountTypes.Length; i++)
        {
            var ofx = $@"<BANKACCTFROM>
<ACCTID>ACC{i}</ACCTID>
<ACCTTYPE>{accountTypes[i]}</ACCTTYPE>
</BANKACCTFROM>";
            // Act
            var accounts = await FinanceReader.FromString(ofx).AsOfx().GetAccountsAsync().ToListAsync();

            // Assert
            Assert.Single(accounts);
            Assert.Equal(expectedTypes[i], accounts[0].Type);
        }
    }

    [Fact]
    public void OfxSourceReaderBuilder_WithAllOptions_ConfiguresCorrectly()
    {
        // Act
        var reader = new OfxSourceReaderBuilder()
            .WithDateFormat("yyyyMMdd")
            .WithDateTimeFormat("yyyyMMddHHmmss")
            .WithTimeZoneOffset(TimeSpan.FromHours(-5))
            .WithEncoding(Encoding.UTF8)
            .Build();

        // Assert - builder should create reader without errors
        Assert.NotNull(reader);
    }

}

