using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using System.IO.Pipelines;
using System.Text;
using Sceny.Finance.IO;
using Sceny.Finance.IO.Plugin.File.Csv;

namespace Sceny.Finance.IO.Benchmarks;

[MemoryDiagnoser]
[SimpleJob(RuntimeMoniker.Net90)]
public class CsvParsingBenchmarks
{
    private readonly byte[] _csvData;
    private readonly Account _testAccount;

    public CsvParsingBenchmarks()
    {
        var csv = "AccountId,Date,Amount,Description\nACC001,2024-01-15,100.50,Transaction 1\nACC001,2024-01-16,-50.25,Transaction 2\nACC001,2024-01-17,200.00,Transaction 3\n";
        _csvData = Encoding.UTF8.GetBytes(csv);
        _testAccount = Account.FromStrings("ACC001", "Test Account", AccountType.Checking, "USD");
    }

    [Benchmark]
    public async Task ParseAccountsFromCsv()
    {
        var reader = CreatePipeReader(_csvData);
        var csvReader = new CsvSourceReader();
        
        await foreach (var account in csvReader.GetAccountsAsync(reader))
        {
            _ = account.Id;
        }
    }

    [Benchmark]
    public async Task ParseTransactionsFromCsv()
    {
        var reader = CreatePipeReader(_csvData);
        var csvReader = new CsvSourceReader();
        
        await foreach (var transaction in csvReader.GetTransactionsAsync(_testAccount, reader))
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

