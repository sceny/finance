using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using System.Text;
using Sceny.Finance.IO;
using Sceny.Finance.IO.Plugin.File.Ofx;

namespace Sceny.Finance.IO.Benchmarks;

[MemoryDiagnoser]
[SimpleJob(RuntimeMoniker.Net90)]
public class OfxParsingBenchmarks
{
    private readonly byte[] _ofxData;
    private readonly Account _testAccount;

    public OfxParsingBenchmarks()
    {
        var ofx = @"<OFX>
<BANKMSGSRSV1>
<STMTTRNRS>
<STMTRS>
<BANKACCTFROM>
<ACCTID>ACC001</ACCTID>
<ACCTTYPE>CHECKING</ACCTTYPE>
</BANKACCTFROM>
<BANKTRANLIST>
<STMTTRN>
<DTPOSTED>20240115</DTPOSTED>
<TRNAMT>100.50</TRNAMT>
<FITID>FIT001</FITID>
<MEMO>Transaction 1</MEMO>
<TRNTYPE>CREDIT</TRNTYPE>
</STMTTRN>
<STMTTRN>
<DTPOSTED>20240116</DTPOSTED>
<TRNAMT>-50.25</TRNAMT>
<FITID>FIT002</FITID>
<MEMO>Transaction 2</MEMO>
<TRNTYPE>DEBIT</TRNTYPE>
</STMTTRN>
</BANKTRANLIST>
</STMTRS>
</STMTTRNRS>
</BANKMSGSRSV1>
</OFX>";
        _ofxData = Encoding.UTF8.GetBytes(ofx);
        _testAccount = Account.FromStrings("ACC001", "Test Account", AccountType.Checking, "USD");
    }

    [Benchmark]
    public async Task ParseAccountsFromOfx()
    {
        await foreach (var account in FinanceReader.FromBytes(_ofxData).AsOfx().GetAccountsAsync())
        {
            _ = account.Id;
        }
    }

    [Benchmark]
    public async Task ParseTransactionsFromOfx()
    {
        await foreach (var transaction in FinanceReader.FromBytes(_ofxData).AsOfx().GetTransactionsAsync(_testAccount))
        {
            _ = transaction.Amount;
        }
    }
}

