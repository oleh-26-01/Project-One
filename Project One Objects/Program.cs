using System.Numerics;
using BenchmarkDotNet.Running;
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
        return;

        var a = new Vector2(1, 1);
        var b = new Vector2(3, 3);
        var c = new Vector2(1, 2);
        var d = new Vector2(2, 1);
        var deltaAB = b - a;
        var deltaCD = d - c;
        var deltaAC = c - a;
        var intersection = MathExtensions.LineIntersection(a, b, c, d);
        var intersectionOpt = MathExtensions.LineIntersectionOpt(a, deltaAB, deltaCD, deltaAC);

        Console.WriteLine(intersection);
        Console.WriteLine(intersectionOpt);
    }
}