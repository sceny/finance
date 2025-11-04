using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
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
        await foreach (var account in FinanceReader.FromBytes(_csvData).AsCsv().GetAccountsAsync())
        {
            _ = account.Id;
        }
    }

    [Benchmark]
    public async Task ParseTransactionsFromCsv()
    {
        await foreach (var transaction in FinanceReader.FromBytes(_csvData).AsCsv().GetTransactionsAsync(_testAccount))
        {
            _ = transaction.Amount;
        }
    }
}

