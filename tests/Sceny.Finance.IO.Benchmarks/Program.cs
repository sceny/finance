using BenchmarkDotNet.Running;
using Sceny.Finance.IO.Benchmarks;

var summary = BenchmarkRunner.Run(typeof(Program).Assembly);

