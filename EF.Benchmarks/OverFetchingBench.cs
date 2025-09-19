using BenchmarkDotNet.Attributes;

namespace EF.Benchmarks;

[MemoryDiagnoser]
public class OverFetchingBench
{
    //Over-fetching is fetching too much data, meaning there is data in the response you don't use.
}