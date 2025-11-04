using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using System.IO.Pipelines;
using System.Text;
using Sceny.Finance.IO;

namespace Sceny.Finance.IO.Benchmarks;

[MemoryDiagnoser]
[SimpleJob(RuntimeMoniker.Net90)]
public class PipelineBenchmarks
{
    private readonly byte[] _testData;

    public PipelineBenchmarks()
    {
        var csv = "AccountId,AccountName,Type,Currency,Date,Amount,Description\nACC001,Checking,Checking,USD,2024-01-15,100.50,Test Transaction\nACC001,Checking,Checking,USD,2024-01-16,-50.25,Debit\n";
        _testData = Encoding.UTF8.GetBytes(csv);
    }

    [Benchmark]
    public async Task ReadCompleteSequenceAsync()
    {
        var pipe = new Pipe();
        var writer = pipe.Writer;
        var span = writer.GetSpan(_testData.Length);
        _testData.CopyTo(span);
        writer.Advance(_testData.Length);
        writer.Complete();

        var sequence = await Utilities.ReadCompleteSequenceAsync(pipe.Reader);
        _ = sequence.Length;
    }

    [Benchmark]
    public string SequenceToString()
    {
        var pipe = new Pipe();
        var writer = pipe.Writer;
        var span = writer.GetSpan(_testData.Length);
        _testData.CopyTo(span);
        writer.Advance(_testData.Length);
        writer.Complete();

        var sequence = Utilities.ReadCompleteSequenceAsync(pipe.Reader).GetAwaiter().GetResult();
        return Utilities.SequenceToString(sequence, Encoding.UTF8);
    }
}

