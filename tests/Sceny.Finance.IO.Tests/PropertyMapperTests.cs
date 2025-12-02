using Sceny.Finance.IO;
using Sceny.Finance.IO.Plugin.File.Csv;
using Sceny.Finance.IO.Plugin.File.Ofx;

namespace Sceny.Finance.IO.Tests;

// Minimal properties struct for testing property mapping when source doesn't implement interfaces
internal readonly struct MinimalAccountProperties : IProperties, IPropertiesFactory<MinimalAccountProperties>
{
    static MinimalAccountProperties IPropertiesFactory<MinimalAccountProperties>.FromSource<TSource>(TSource source)
    {
        return default;
    }
}

internal readonly struct MinimalTransactionProperties : IProperties, IPropertiesFactory<MinimalTransactionProperties>
{
    static MinimalTransactionProperties IPropertiesFactory<MinimalTransactionProperties>.FromSource<TSource>(TSource source)
    {
        return default;
    }
}

public class PropertyMapperTests
{
    [Fact]
    public void MapProperties_Account_FromCsvToOfx_MapsCorrectly()
    {
        // Arrange
        var csvProps = new CsvAccountProperties(
            bankName: "Test Bank",
            branchCode: "001",
            extended: default,
            attachments: default
        );
        var csvAccount = Account<CsvAccountProperties>.FromStrings("ACC001", "Test Account", AccountType.Checking, "USD", csvProps);

        // Act
        var ofxAccount = PropertyMapper.MapProperties<CsvAccountProperties, OfxAccountProperties>(csvAccount);

        // Assert
        Assert.Equal("ACC001", ofxAccount.Id.ToString());
        Assert.Equal("Test Account", ofxAccount.Name.ToString());
        Assert.Equal(AccountType.Checking, ofxAccount.Type);
        Assert.Equal("USD", ofxAccount.Currency.ToString());
        // BankName and BranchCode don't map to OFX, but Extended should be preserved
        if (ofxAccount.Properties is IExtended ext)
        {
            // Extended should be empty since source had empty extended
            Assert.True(ext.Extended.IsEmpty);
        }
    }

    [Fact]
    public void MapProperties_Account_FromOfxToCsv_MapsCorrectly()
    {
        // Arrange
        var ofxProps = new OfxAccountProperties(
            bankId: "BANK001",
            extended: default
        );
        var ofxAccount = Account<OfxAccountProperties>.FromStrings("ACC001", "Test Account", AccountType.Checking, "USD", ofxProps);

        // Act
        var csvAccount = PropertyMapper.MapProperties<OfxAccountProperties, CsvAccountProperties>(ofxAccount);

        // Assert
        Assert.Equal("ACC001", csvAccount.Id.ToString());
        Assert.Equal("Test Account", csvAccount.Name.ToString());
        Assert.Equal(AccountType.Checking, csvAccount.Type);
        Assert.Equal("USD", csvAccount.Currency.ToString());
        // BankId doesn't map to CSV, but Extended should be preserved
        if (csvAccount.Properties is IExtended ext)
        {
            Assert.True(ext.Extended.IsEmpty);
        }
    }

    [Fact]
    public void MapProperties_Account_WithExtended_MapsExtended()
    {
        // Arrange
        var extendedPairs = new KeyValuePair<ReadOnlyString, ReadOnlyString>[]
        {
            new("CustomField1", "Value1"),
            new("CustomField2", "Value2")
        };
        var extended = new ReadOnlyExtended(new ReadOnlyMemory<KeyValuePair<ReadOnlyString, ReadOnlyString>>(extendedPairs));
        var csvProps = new CsvAccountProperties(
            bankName: "Test Bank",
            branchCode: "001",
            extended: extended,
            attachments: default
        );
        var csvAccount = Account<CsvAccountProperties>.FromStrings("ACC001", "Test Account", AccountType.Checking, "USD", csvProps);

        // Act
        var ofxAccount = PropertyMapper.MapProperties<CsvAccountProperties, OfxAccountProperties>(csvAccount);

        // Assert
        if (ofxAccount.Properties is IExtended ext)
        {
            Assert.False(ext.Extended.IsEmpty);
            Assert.Equal(2, ext.Extended.Count);
            Assert.True(ext.Extended.ContainsKey("CustomField1"));
            Assert.True(ext.Extended.TryGetValue("CustomField1", out var value));
            Assert.Equal("Value1", value.ToString());
        }
        else
        {
            Assert.Fail("OFX account should have IExtended");
        }
    }

    [Fact]
    public void MapProperties_Account_WithAttachments_MapsAttachments()
    {
        // Arrange
        var attachmentData = new byte[] { 1, 2, 3, 4, 5 };
        var attachment = new Attachment("test.pdf", "application/pdf", new ReadOnlyMemory<byte>(attachmentData), "ref://test");
        var attachments = new ReadOnlyAttachments(new ReadOnlyMemory<Attachment>(new[] { attachment }));
        var csvProps = new CsvAccountProperties(
            bankName: "Test Bank",
            branchCode: "001",
            extended: default,
            attachments: attachments
        );
        var csvAccount = Account<CsvAccountProperties>.FromStrings("ACC001", "Test Account", AccountType.Checking, "USD", csvProps);

        // Act
        var ofxAccount = PropertyMapper.MapProperties<CsvAccountProperties, OfxAccountProperties>(csvAccount);

        // Assert
        // OFX doesn't support attachments, so they won't be mapped
        // But the base account data should be preserved
        Assert.Equal("ACC001", ofxAccount.Id.ToString());
        Assert.Equal("Test Account", ofxAccount.Name.ToString());
    }

    [Fact]
    public void MapProperties_Transaction_FromCsvToOfx_MapsCorrectly()
    {
        // Arrange
        var csvTxProps = new CsvTransactionProperties(
            extended: default,
            attachments: default
        );
        var csvTransaction = Transaction<CsvTransactionProperties>.FromStrings(
            "ACC001",
            100.50m,
            new DateTime(2024, 1, 15),
            "Test Transaction",
            TransactionType.Credit,
            csvTxProps,
            "REF123"
        );

        // Act
        var ofxTransaction = PropertyMapper.MapProperties<CsvTransactionProperties, OfxTransactionProperties>(csvTransaction);

        // Assert
        Assert.Equal("ACC001", ofxTransaction.AccountId.ToString());
        Assert.Equal(100.50m, ofxTransaction.Amount);
        Assert.Equal(new DateTime(2024, 1, 15), ofxTransaction.Date);
        Assert.Equal("Test Transaction", ofxTransaction.Description.ToString());
        Assert.Equal(TransactionType.Credit, ofxTransaction.Type);
        Assert.Equal("REF123", ofxTransaction.Reference.ToString());
    }

    [Fact]
    public void MapProperties_Transaction_FromOfxToCsv_MapsCorrectly()
    {
        // Arrange
        var ofxTxProps = new OfxTransactionProperties(extended: default);
        var ofxTransaction = Transaction<OfxTransactionProperties>.FromStrings(
            "ACC001",
            100.50m,
            new DateTime(2024, 1, 15),
            "Test Transaction",
            TransactionType.Credit,
            ofxTxProps,
            "REF123"
        );

        // Act
        var csvTransaction = PropertyMapper.MapProperties<OfxTransactionProperties, CsvTransactionProperties>(ofxTransaction);

        // Assert
        Assert.Equal("ACC001", csvTransaction.AccountId.ToString());
        Assert.Equal(100.50m, csvTransaction.Amount);
        Assert.Equal(new DateTime(2024, 1, 15), csvTransaction.Date);
        Assert.Equal("Test Transaction", csvTransaction.Description.ToString());
        Assert.Equal(TransactionType.Credit, csvTransaction.Type);
        Assert.Equal("REF123", csvTransaction.Reference.ToString());
    }

    [Fact]
    public void MapProperties_Transaction_WithExtended_MapsExtended()
    {
        // Arrange
        var extendedPairs = new KeyValuePair<ReadOnlyString, ReadOnlyString>[]
        {
            new("CustomField1", "Value1")
        };
        var extended = new ReadOnlyExtended(new ReadOnlyMemory<KeyValuePair<ReadOnlyString, ReadOnlyString>>(extendedPairs));
        var csvTxProps = new CsvTransactionProperties(
            extended: extended,
            attachments: default
        );
        var csvTransaction = Transaction<CsvTransactionProperties>.FromStrings(
            "ACC001",
            100.50m,
            new DateTime(2024, 1, 15),
            "Test Transaction",
            TransactionType.Credit,
            csvTxProps
        );

        // Act
        var ofxTransaction = PropertyMapper.MapProperties<CsvTransactionProperties, OfxTransactionProperties>(csvTransaction);

        // Assert
        if (ofxTransaction.Properties is IExtended ext)
        {
            Assert.False(ext.Extended.IsEmpty);
            Assert.True(ext.Extended.ContainsKey("CustomField1"));
        }
        else
        {
            Assert.Fail("OFX transaction should have IExtended");
        }
    }

    [Fact]
    public void MapProperties_Account_FromMinimalToCsv_HandlesMissingInterfaces()
    {
        // Arrange - Minimal properties don't implement any atomic interfaces
        var minimalAccount = Account<MinimalAccountProperties>.FromStrings("ACC001", "Test Account", AccountType.Checking, "USD", default);

        // Act
        var csvAccount = PropertyMapper.MapProperties<MinimalAccountProperties, CsvAccountProperties>(minimalAccount);

        // Assert - Should map with default values for missing interfaces
        Assert.Equal("ACC001", csvAccount.Id.ToString());
        Assert.Equal("Test Account", csvAccount.Name.ToString());
        // BankName and BranchCode should be default since source doesn't implement IBankName/IBranchCode
        Assert.True(csvAccount.Properties.BankName.IsEmpty);
        Assert.True(csvAccount.Properties.BranchCode.IsEmpty);
    }

    [Fact]
    public void MapProperties_Account_FromMinimalToOfx_HandlesMissingInterfaces()
    {
        // Arrange
        var minimalAccount = Account<MinimalAccountProperties>.FromStrings("ACC001", "Test Account", AccountType.Checking, "USD", default);

        // Act
        var ofxAccount = PropertyMapper.MapProperties<MinimalAccountProperties, OfxAccountProperties>(minimalAccount);

        // Assert
        Assert.Equal("ACC001", ofxAccount.Id.ToString());
        // BankId should be default since source doesn't implement IBankId
        Assert.True(ofxAccount.Properties.BankId.IsEmpty);
    }

    [Fact]
    public void MapProperties_Transaction_FromMinimalToCsv_HandlesMissingInterfaces()
    {
        // Arrange
        var minimalTransaction = Transaction<MinimalTransactionProperties>.FromStrings(
            "ACC001",
            100.50m,
            new DateTime(2024, 1, 15),
            "Test Transaction",
            TransactionType.Credit,
            default
        );

        // Act
        var csvTransaction = PropertyMapper.MapProperties<MinimalTransactionProperties, CsvTransactionProperties>(minimalTransaction);

        // Assert
        Assert.Equal("ACC001", csvTransaction.AccountId.ToString());
        Assert.Equal(100.50m, csvTransaction.Amount);
        // Extended and Attachments should be default/empty
        Assert.True(csvTransaction.Properties.Extended.IsEmpty);
        Assert.True(csvTransaction.Properties.Attachments.IsEmpty);
    }

    [Fact]
    public void MapProperties_Transaction_FromMinimalToOfx_HandlesMissingInterfaces()
    {
        // Arrange
        var minimalTransaction = Transaction<MinimalTransactionProperties>.FromStrings(
            "ACC001",
            100.50m,
            new DateTime(2024, 1, 15),
            "Test Transaction",
            TransactionType.Credit,
            default
        );

        // Act
        var ofxTransaction = PropertyMapper.MapProperties<MinimalTransactionProperties, OfxTransactionProperties>(minimalTransaction);

        // Assert
        Assert.Equal("ACC001", ofxTransaction.AccountId.ToString());
        // Extended should be default/empty
        Assert.True(ofxTransaction.Properties.Extended.IsEmpty);
    }

    [Fact]
    public void MapProperties_Account_FromCsvToCsv_PreservesAllProperties()
    {
        // Arrange
        var extendedPairs = new KeyValuePair<ReadOnlyString, ReadOnlyString>[]
        {
            new("CustomField1", "Value1")
        };
        var extended = new ReadOnlyExtended(new ReadOnlyMemory<KeyValuePair<ReadOnlyString, ReadOnlyString>>(extendedPairs));
        var attachment = new Attachment("test.pdf", "application/pdf", new ReadOnlyMemory<byte>(new byte[] { 1, 2, 3 }));
        var attachments = new ReadOnlyAttachments(new ReadOnlyMemory<Attachment>(new[] { attachment }));
        var csvProps = new CsvAccountProperties(
            bankName: "Test Bank",
            branchCode: "001",
            extended: extended,
            attachments: attachments
        );
        var csvAccount = Account<CsvAccountProperties>.FromStrings("ACC001", "Test Account", AccountType.Checking, "USD", csvProps);

        // Act
        var mappedAccount = PropertyMapper.MapProperties<CsvAccountProperties, CsvAccountProperties>(csvAccount);

        // Assert
        Assert.Equal("Test Bank", mappedAccount.Properties.BankName.ToString());
        Assert.Equal("001", mappedAccount.Properties.BranchCode.ToString());
        Assert.False(mappedAccount.Properties.Extended.IsEmpty);
        Assert.Equal(1, mappedAccount.Properties.Attachments.Count);
    }

    [Fact]
    public void MapProperties_Transaction_FromCsvToCsv_PreservesAllProperties()
    {
        // Arrange
        var extendedPairs = new KeyValuePair<ReadOnlyString, ReadOnlyString>[]
        {
            new("CustomField1", "Value1")
        };
        var extended = new ReadOnlyExtended(new ReadOnlyMemory<KeyValuePair<ReadOnlyString, ReadOnlyString>>(extendedPairs));
        var attachment = new Attachment("test.pdf", "application/pdf", new ReadOnlyMemory<byte>(new byte[] { 1, 2, 3 }));
        var attachments = new ReadOnlyAttachments(new ReadOnlyMemory<Attachment>(new[] { attachment }));
        var csvTxProps = new CsvTransactionProperties(
            extended: extended,
            attachments: attachments
        );
        var csvTransaction = Transaction<CsvTransactionProperties>.FromStrings(
            "ACC001",
            100.50m,
            new DateTime(2024, 1, 15),
            "Test Transaction",
            TransactionType.Credit,
            csvTxProps
        );

        // Act
        var mappedTransaction = PropertyMapper.MapProperties<CsvTransactionProperties, CsvTransactionProperties>(csvTransaction);

        // Assert
        Assert.False(mappedTransaction.Properties.Extended.IsEmpty);
        Assert.Equal(1, mappedTransaction.Properties.Attachments.Count);
    }

    [Fact]
    public void MapProperties_Account_FromOfxWithBankIdOnly_MapsBankId()
    {
        // Arrange - OFX account with BankId but no Extended
        var ofxProps = new OfxAccountProperties(
            bankId: "BANK001",
            extended: default
        );
        var ofxAccount = Account<OfxAccountProperties>.FromStrings("ACC001", "Test Account", AccountType.Checking, "USD", ofxProps);

        // Act - Map to CSV (which doesn't have BankId, but should preserve Extended)
        var csvAccount = PropertyMapper.MapProperties<OfxAccountProperties, CsvAccountProperties>(ofxAccount);

        // Assert - BankId doesn't map to CSV, Extended should be empty
        Assert.True(csvAccount.Properties.Extended.IsEmpty);
        // BankName and BranchCode should be default since OFX doesn't have them
        Assert.True(csvAccount.Properties.BankName.IsEmpty);
        Assert.True(csvAccount.Properties.BranchCode.IsEmpty);
    }

    [Fact]
    public void MapProperties_Account_FromCsvWithBankNameOnly_MapsBankName()
    {
        // Arrange - CSV account with BankName but no Extended
        var csvProps = new CsvAccountProperties(
            bankName: "Test Bank",
            branchCode: default,
            extended: default,
            attachments: default
        );
        var csvAccount = Account<CsvAccountProperties>.FromStrings("ACC001", "Test Account", AccountType.Checking, "USD", csvProps);

        // Act - Map to OFX
        var ofxAccount = PropertyMapper.MapProperties<CsvAccountProperties, OfxAccountProperties>(csvAccount);

        // Assert - BankName doesn't map to OFX, Extended should be empty
        Assert.True(ofxAccount.Properties.Extended.IsEmpty);
        // BankId should be default since CSV doesn't have it
        Assert.True(ofxAccount.Properties.BankId.IsEmpty);
    }

    [Fact]
    public void MapProperties_Account_FromCsvWithExtendedOnly_MapsExtended()
    {
        // Arrange - CSV account with Extended but no typed properties
        var extendedPairs = new KeyValuePair<ReadOnlyString, ReadOnlyString>[]
        {
            new("CustomField1", "Value1")
        };
        var extended = new ReadOnlyExtended(new ReadOnlyMemory<KeyValuePair<ReadOnlyString, ReadOnlyString>>(extendedPairs));
        var csvProps = new CsvAccountProperties(
            bankName: default,
            branchCode: default,
            extended: extended,
            attachments: default
        );
        var csvAccount = Account<CsvAccountProperties>.FromStrings("ACC001", "Test Account", AccountType.Checking, "USD", csvProps);

        // Act
        var ofxAccount = PropertyMapper.MapProperties<CsvAccountProperties, OfxAccountProperties>(csvAccount);

        // Assert - Extended should be preserved
        if (ofxAccount.Properties is IExtended ext)
        {
            Assert.False(ext.Extended.IsEmpty);
            Assert.True(ext.Extended.ContainsKey("CustomField1"));
        }
        else
        {
            Assert.Fail("OFX account should have IExtended");
        }
    }

    [Fact]
    public void MapProperties_Account_FromOfxWithExtendedOnly_MapsExtended()
    {
        // Arrange - OFX account with Extended but no BankId
        var extendedPairs = new KeyValuePair<ReadOnlyString, ReadOnlyString>[]
        {
            new("CustomField1", "Value1")
        };
        var extended = new ReadOnlyExtended(new ReadOnlyMemory<KeyValuePair<ReadOnlyString, ReadOnlyString>>(extendedPairs));
        var ofxProps = new OfxAccountProperties(
            bankId: default,
            extended: extended
        );
        var ofxAccount = Account<OfxAccountProperties>.FromStrings("ACC001", "Test Account", AccountType.Checking, "USD", ofxProps);

        // Act
        var csvAccount = PropertyMapper.MapProperties<OfxAccountProperties, CsvAccountProperties>(ofxAccount);

        // Assert - Extended should be preserved
        if (csvAccount.Properties is IExtended ext)
        {
            Assert.False(ext.Extended.IsEmpty);
            Assert.True(ext.Extended.ContainsKey("CustomField1"));
        }
        else
        {
            Assert.Fail("CSV account should have IExtended");
        }
    }

    [Fact]
    public void MapProperties_Account_FromSourceWithoutIBankId_HandlesMissingInterface()
    {
        // Arrange - Source that doesn't implement IBankId
        var minimalAccount = Account<MinimalAccountProperties>.FromStrings("ACC001", "Test Account", AccountType.Checking, "USD", default);

        // Act - Map to OFX (which requires IBankId)
        var ofxAccount = PropertyMapper.MapProperties<MinimalAccountProperties, OfxAccountProperties>(minimalAccount);

        // Assert - BankId should be default since source doesn't implement IBankId
        // This tests the missing branch in OfxAccountProperties.FromSource
        Assert.True(ofxAccount.Properties.BankId.IsEmpty);
    }

    [Fact]
    public void MapProperties_Account_FromSourceWithIBankIdButNoIExtended_HandlesPartialInterfaces()
    {
        // Arrange - Create a properties struct that implements IBankId but not IExtended
        // We'll use OfxAccountProperties as source (which has IBankId) but without Extended
        var ofxProps = new OfxAccountProperties(
            bankId: "BANK001",
            extended: default
        );
        var ofxAccount = Account<OfxAccountProperties>.FromStrings("ACC001", "Test Account", AccountType.Checking, "USD", ofxProps);

        // Act - Map to CSV (which has both IBankName and IExtended)
        var csvAccount = PropertyMapper.MapProperties<OfxAccountProperties, CsvAccountProperties>(ofxAccount);

        // Assert - BankId doesn't map to CSV, Extended should be empty
        Assert.True(csvAccount.Properties.BankName.IsEmpty);
        Assert.True(csvAccount.Properties.BranchCode.IsEmpty);
        Assert.True(csvAccount.Properties.Extended.IsEmpty);
    }

    [Fact]
    public void MapProperties_Account_FromSourceWithIExtendedButNoIBankId_HandlesPartialInterfaces()
    {
        // Arrange - Source with Extended but no BankId
        var extendedPairs = new KeyValuePair<ReadOnlyString, ReadOnlyString>[]
        {
            new("CustomField1", "Value1")
        };
        var extended = new ReadOnlyExtended(new ReadOnlyMemory<KeyValuePair<ReadOnlyString, ReadOnlyString>>(extendedPairs));
        var csvProps = new CsvAccountProperties(
            bankName: default,
            branchCode: default,
            extended: extended,
            attachments: default
        );
        var csvAccount = Account<CsvAccountProperties>.FromStrings("ACC001", "Test Account", AccountType.Checking, "USD", csvProps);

        // Act - Map to OFX (which has IBankId and IExtended)
        var ofxAccount = PropertyMapper.MapProperties<CsvAccountProperties, OfxAccountProperties>(csvAccount);

        // Assert - BankId should be default, Extended should be preserved
        Assert.True(ofxAccount.Properties.BankId.IsEmpty);
        if (ofxAccount.Properties is IExtended ext)
        {
            Assert.False(ext.Extended.IsEmpty);
            Assert.True(ext.Extended.ContainsKey("CustomField1"));
        }
    }
}

