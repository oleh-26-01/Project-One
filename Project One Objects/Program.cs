using BenchmarkDotNet.Running;

namespace Project_One_Objects;

internal class Program
{
    private static void Main()
    {
        BenchmarkRunner.Run<Benchmark>();
    }
}