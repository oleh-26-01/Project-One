using System.Numerics;
using BenchmarkDotNet.Attributes;

namespace Project_One_Objects;

[MemoryDiagnoser]
public class Benchmark
{

    public Benchmark()
    {
        MathExtensions.IsAngleBetween(0f, 12f, 2f);
    }
}