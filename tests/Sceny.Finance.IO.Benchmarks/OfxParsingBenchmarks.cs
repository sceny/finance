using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using System.IO.Pipelines;
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
        var reader = CreatePipeReader(_ofxData);
        var ofxReader = new OfxSourceReader();
        
        await foreach (var account in ofxReader.GetAccountsAsync(reader))
        {
            _ = account.Id;
        }
    }

    [Benchmark]
    public async Task ParseTransactionsFromOfx()
    {
        var reader = CreatePipeReader(_ofxData);
        var ofxReader = new OfxSourceReader();
        
        await foreach (var transaction in ofxReader.GetTransactionsAsync(_testAccount, reader))
        {
            _ = transaction.Amount;
        }
    }

    private static PipeReader CreatePipeReader(byte[] data)
    {
        var pipe = new Pipe();
        var writer = pipe.Writer;
        var span = writer.GetSpan(data.Length);
        data.CopyTo(span);
        writer.Advance(data.Length);
        writer.Complete();
        return pipe.Reader;
    }
}

