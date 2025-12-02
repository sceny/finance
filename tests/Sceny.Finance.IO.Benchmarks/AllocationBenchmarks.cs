using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using Sceny.Finance.IO;
using Sceny.Finance.IO.Plugin.File.Csv;

namespace Sceny.Finance.IO.Benchmarks;

[MemoryDiagnoser]
public class AllocationBenchmarks
{
    [Benchmark]
    public Account<CsvAccountProperties> CreateAccount_FromStrings()
    {
        return Account<CsvAccountProperties>.FromStrings("ACC001", "Checking Account", AccountType.Checking, "USD", default(CsvAccountProperties));
    }

    [Benchmark]
    public Account<CsvAccountProperties> CreateAccount_ImplicitConversion()
    {
        return new Account<CsvAccountProperties>("ACC001", "Checking Account", AccountType.Checking, "USD", default(CsvAccountProperties));
    }

    [Benchmark]
    public Transaction<CsvTransactionProperties> CreateTransaction_FromStrings()
    {
        return Transaction<CsvTransactionProperties>.FromStrings(
            "ACC001",
            100.50m,
            new DateTime(2024, 1, 15),
            "Test Transaction",
            TransactionType.Credit,
            default(CsvTransactionProperties),
            "REF123"
        );
    }

    [Benchmark]
    public Transaction<CsvTransactionProperties> CreateTransaction_ImplicitConversion()
    {
        return new Transaction<CsvTransactionProperties>(
            "ACC001",
            100.50m,
            new DateTime(2024, 1, 15),
            "Test Transaction",
            TransactionType.Credit,
            default(CsvTransactionProperties),
            "REF123"
        );
    }

    [Benchmark]
    public string Account_ToString()
    {
        var account = Account<CsvAccountProperties>.FromStrings("ACC001", "Checking", AccountType.Checking, "USD", default(CsvAccountProperties));
        return account.Id; // Implicit conversion to string
    }

    [Benchmark]
    public string Transaction_ToString()
    {
        var transaction = Transaction<CsvTransactionProperties>.FromStrings("ACC001", 100m, DateTime.Now, "Test", TransactionType.Credit, default(CsvTransactionProperties));
        return transaction.AccountId; // Implicit conversion to string
    }

    [Benchmark]
    public ReadOnlySpan<char> Account_ToSpan()
    {
        var account = Account<CsvAccountProperties>.FromStrings("ACC001", "Checking", AccountType.Checking, "USD", default(CsvAccountProperties));
        return account.Id; // Implicit conversion to ReadOnlySpan<char>
    }
}
