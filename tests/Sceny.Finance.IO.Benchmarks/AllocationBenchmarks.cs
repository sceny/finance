using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using Sceny.Finance.IO;

namespace Sceny.Finance.IO.Benchmarks;

[MemoryDiagnoser]
public class AllocationBenchmarks
{
    [Benchmark]
    public Account CreateAccount_FromStrings()
    {
        return Account.FromStrings("ACC001", "Checking Account", AccountType.Checking, "USD");
    }

    [Benchmark]
    public Account CreateAccount_ImplicitConversion()
    {
        return new Account("ACC001", "Checking Account", AccountType.Checking, "USD");
    }

    [Benchmark]
    public Transaction CreateTransaction_FromStrings()
    {
        return Transaction.FromStrings(
            "ACC001",
            100.50m,
            new DateTime(2024, 1, 15),
            "Test Transaction",
            TransactionType.Credit,
            "REF123"
        );
    }

    [Benchmark]
    public Transaction CreateTransaction_ImplicitConversion()
    {
        return new Transaction(
            "ACC001",
            100.50m,
            new DateTime(2024, 1, 15),
            "Test Transaction",
            TransactionType.Credit,
            "REF123"
        );
    }

    [Benchmark]
    public string Account_ToString()
    {
        var account = Account.FromStrings("ACC001", "Checking", AccountType.Checking, "USD");
        return account.Id; // Implicit conversion to string
    }

    [Benchmark]
    public string Transaction_ToString()
    {
        var transaction = Transaction.FromStrings("ACC001", 100m, DateTime.Now, "Test", TransactionType.Credit);
        return transaction.AccountId; // Implicit conversion to string
    }

    [Benchmark]
    public ReadOnlySpan<char> Account_ToSpan()
    {
        var account = Account.FromStrings("ACC001", "Checking", AccountType.Checking, "USD");
        return account.Id; // Implicit conversion to ReadOnlySpan<char>
    }
}
