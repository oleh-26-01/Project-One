using System.Numerics;
using BenchmarkDotNet.Attributes;

namespace Project_One_Objects;

[MemoryDiagnoser]
public class Benchmark
{
    private readonly Camera _camera;
    private readonly Vector2[] _convertArray;

    public Benchmark()
    {
        var convertArraySize = 1000000;
        _convertArray = new Vector2[convertArraySize];
        for (var i = 0; i < convertArraySize; i++) _convertArray[i] = new Vector2(i, i);

        _camera = new Camera(new Vector2(1280, 720), new Vector2(100, 100));
    }
}