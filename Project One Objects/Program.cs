using BenchmarkDotNet.Running;
using Project_One_Objects.Helpers;

namespace Project_One_Objects;

internal class Program
{
    private static void RunBenchmark()
    {
        _ = BenchmarkRunner.Run<Benchmark>();
    }

    private static void Main()
    {
        RunBenchmark();
    }
}