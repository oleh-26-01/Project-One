using System.Numerics;
using BenchmarkDotNet.Attributes;

namespace Project_One_Objects;

[MemoryDiagnoser]
public class Benchmark
{
    private readonly Car _exampleCar;
    private Car _exampleCarCopy;

    public Benchmark()
    {
        _exampleCar = new Car(Vector2.Zero, Math.PI / 2);
    }

    [Benchmark]
    public void CreateFrom()
    {
        _exampleCarCopy = new Car(_exampleCar);
    }
}