using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using System.Buffers;
using System.Text;
using Sceny.Finance.IO;
using Sceny.Finance.IO.Plugin.File.Csv;
using Sceny.Finance.IO.Plugin.File.Ofx;

namespace Sceny.Finance.IO.Benchmarks;

[MemoryDiagnoser]
public class GenericPropertiesBenchmarks
{
    private readonly Account<CsvAccountProperties> _csvAccount;
    private readonly Account<CsvAccountProperties> _csvAccountWithExtended;
    private readonly Account<CsvAccountProperties> _csvAccountWithAttachments;
    private readonly Account<OfxAccountProperties> _ofxAccount;
    private readonly Transaction<CsvTransactionProperties> _csvTransaction;
    private readonly Transaction<OfxTransactionProperties> _ofxTransaction;
    private readonly byte[] _csvData;
    private readonly byte[] _ofxData;

    public GenericPropertiesBenchmarks()
    {
        // Basic CSV account
        var csvProps = new CsvAccountProperties(
            bankName: "Test Bank",
            branchCode: "001",
            extended: default,
            attachments: default
        );
        _csvAccount = Account<CsvAccountProperties>.FromStrings("ACC001", "Test Account", AccountType.Checking, "USD", csvProps);

        // CSV account with Extended properties
        var extendedPairs = new KeyValuePair<ReadOnlyString, ReadOnlyString>[]
        {
            new("CustomField1", "Value1"),
            new("CustomField2", "Value2"),
            new("CustomField3", "Value3")
        };
        var extended = new ReadOnlyExtended(new ReadOnlyMemory<KeyValuePair<ReadOnlyString, ReadOnlyString>>(extendedPairs));
        var csvPropsWithExtended = new CsvAccountProperties(
            bankName: "Test Bank",
            branchCode: "001",
            extended: extended,
            attachments: default
        );
        _csvAccountWithExtended = Account<CsvAccountProperties>.FromStrings("ACC001", "Test Account", AccountType.Checking, "USD", csvPropsWithExtended);

        // CSV account with Attachments
        var attachmentData = new byte[] { 1, 2, 3, 4, 5 };
        var attachment = new Attachment("test.pdf", "application/pdf", new ReadOnlyMemory<byte>(attachmentData), "ref://test");
        var attachments = new ReadOnlyAttachments(new ReadOnlyMemory<Attachment>(new[] { attachment }));
        var csvPropsWithAttachments = new CsvAccountProperties(
            bankName: "Test Bank",
            branchCode: "001",
            extended: default,
            attachments: attachments
        );
        _csvAccountWithAttachments = Account<CsvAccountProperties>.FromStrings("ACC001", "Test Account", AccountType.Checking, "USD", csvPropsWithAttachments);

        // OFX account
        var ofxProps = new OfxAccountProperties(
            bankId: "BANK001",
            extended: default
        );
        _ofxAccount = Account<OfxAccountProperties>.FromStrings("ACC001", "Test Account", AccountType.Checking, "USD", ofxProps);

        // CSV transaction
        var csvTxProps = new CsvTransactionProperties(
            extended: default,
            attachments: default
        );
        _csvTransaction = Transaction<CsvTransactionProperties>.FromStrings("ACC001", 100.50m, new DateTime(2024, 1, 15), "Test Transaction", TransactionType.Credit, csvTxProps);

        // OFX transaction
        var ofxTxProps = new OfxTransactionProperties(extended: default);
        _ofxTransaction = Transaction<OfxTransactionProperties>.FromStrings("ACC001", 100.50m, new DateTime(2024, 1, 15), "Test Transaction", TransactionType.Credit, ofxTxProps);

        var csv = "AccountId,AccountName,Type,Currency,BankName,BranchCode\nACC001,Test Account,Checking,USD,Bank1,001\n";
        _csvData = Encoding.UTF8.GetBytes(csv);

        var ofx = @"<OFX>
<BANKMSGSRSV1>
<STMTTRNRS>
<STMTRS>
<BANKACCTFROM>
<BANKID>BANK001</BANKID>
<ACCTID>ACC001</ACCTID>
<ACCTTYPE>CHECKING</ACCTTYPE>
</BANKACCTFROM>
<DESC>Test Account</DESC>
<CURDEF>USD</CURDEF>
<BANKTRANLIST>
<STMTTRN>
<DTPOSTED>20240115</DTPOSTED>
<TRNAMT>100.50</TRNAMT>
<TRNTYPE>CREDIT</TRNTYPE>
<MEMO>Test Transaction</MEMO>
<FITID>FIT001</FITID>
</STMTTRN>
</BANKTRANLIST>
</STMTRS>
</STMTTRNRS>
</BANKMSGSRSV1>
</OFX>";
        _ofxData = Encoding.UTF8.GetBytes(ofx);
    }

    [Benchmark]
    public ReadOnlyString Properties_AccessTypedProperty()
    {
        if (_csvAccount.Properties is IBankName bankNameProp)
        {
            return bankNameProp.BankName;
        }
        return default;
    }

    [Benchmark]
    public ReadOnlyString Properties_AccessMultipleTypedProperties()
    {
        if (_csvAccount.Properties is IBankName bankNameProp && _csvAccount.Properties is IBranchCode branchCodeProp)
        {
            return bankNameProp.BankName; // Return one, but both were checked
        }
        return default;
    }

    [Benchmark]
    public bool Properties_AtomicInterfacePatternMatching()
    {
        // Test with CSV account (has IBankName and IBranchCode)
        return _csvAccount.Properties is IBankName && _csvAccount.Properties is IBranchCode;
    }

    [Benchmark]
    public bool Properties_AtomicInterfacePatternMatching_WithOfx()
    {
        // Test with OFX account (doesn't have IBankName or IBranchCode, but has IBankId)
        return _ofxAccount.Properties is IBankName && _ofxAccount.Properties is IBranchCode;
    }

    [Benchmark]
    public bool Properties_AtomicInterfacePatternMatching_Multiple()
    {
        // Test multiple interface checks on CSV account
        return _csvAccount.Properties is IBankName 
            && _csvAccount.Properties is IBranchCode 
            && _csvAccount.Properties is IExtended 
            && _csvAccount.Properties is IAttachments;
    }

    [Benchmark]
    public ReadOnlyString Properties_AccessExtendedProperty()
    {
        if (_csvAccountWithExtended.Properties is IExtended ext)
        {
            if (ext.Extended.TryGetValue("CustomField1", out var value))
            {
                return value;
            }
        }
        return default;
    }

    [Benchmark]
    public int Properties_EnumerateExtendedProperties()
    {
        int count = 0;
        if (_csvAccountWithExtended.Properties is IExtended ext)
        {
            foreach (var pair in ext.Extended)
            {
                count++;
            }
        }
        return count;
    }

    [Benchmark]
    public int Properties_AccessAttachments()
    {
        if (_csvAccountWithAttachments.Properties is IAttachments att)
        {
            return att.Attachments.Count;
        }
        return 0;
    }

    [Benchmark]
    public int Properties_EnumerateAttachments()
    {
        int count = 0;
        if (_csvAccountWithAttachments.Properties is IAttachments att)
        {
            foreach (var attachment in att.Attachments)
            {
                count += attachment.Size;
            }
        }
        return count;
    }

    [Benchmark]
    public Account<OfxAccountProperties> Properties_GenericMapping()
    {
        return PropertyMapper.MapProperties<CsvAccountProperties, OfxAccountProperties>(_csvAccount);
    }

    [Benchmark]
    public Account<OfxAccountProperties> Properties_GenericMapping_WithExtended()
    {
        return PropertyMapper.MapProperties<CsvAccountProperties, OfxAccountProperties>(_csvAccountWithExtended);
    }

    [Benchmark]
    public Transaction<OfxTransactionProperties> Properties_GenericMappingTransaction()
    {
        return PropertyMapper.MapProperties<CsvTransactionProperties, OfxTransactionProperties>(_csvTransaction);
    }

    [Benchmark]
    public Account<CsvAccountProperties> Properties_GenericMapping_Reverse()
    {
        return PropertyMapper.MapProperties<OfxAccountProperties, CsvAccountProperties>(_ofxAccount);
    }

    [Benchmark]
    public async Task Properties_FullPipelineReadWrite()
    {
        var accounts = FinanceReader.FromBytes(_csvData).AsCsv().GetAccountsAsync();
        var sb = new StringBuilder();
        var writer = FinanceWriter.ToString(sb).AsCsv();

        await foreach (var account in accounts)
        {
            await writer.BeginWriteAsync();
            await writer.WriteAccountAsync(account);
            await writer.EndWriteAsync();
        }
        await Task.Delay(100); // Wait for async writes
    }

    [Benchmark]
    public async Task Properties_FormatConversion()
    {
        var accounts = FinanceReader.FromBytes(_csvData).AsCsv().GetAccountsAsync();
        var sb = new StringBuilder();
        var writer = FinanceWriter.ToString(sb).AsOfx();

        await writer.BeginWriteAsync();
        await foreach (var csvAccount in accounts)
        {
            var ofxAccount = PropertyMapper.MapProperties<CsvAccountProperties, OfxAccountProperties>(csvAccount);
            await writer.WriteAccountAsync(ofxAccount);
        }
        await writer.EndWriteAsync();
        await Task.Delay(100); // Wait for async writes
    }
}

