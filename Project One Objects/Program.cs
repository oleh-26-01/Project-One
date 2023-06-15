using System.Diagnostics;
using System.Numerics;
using BenchmarkDotNet.Running;
using Project_One_Objects.AIComponents;
using Project_One_Objects.Environment;
using Project_One_Objects.Helpers;

namespace Project_One_Objects;

internal class Program
{
    private static void RunBenchmark()
    {
        var summary = BenchmarkRunner.Run<Benchmark>();
    }
    
    private static void Main()
    {
        RunBenchmark();
    }
}