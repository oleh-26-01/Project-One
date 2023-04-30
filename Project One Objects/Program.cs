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
        //RunBenchmark();
        var track = new Track().Load("C:\\Coding\\C#\\Project One\\Project One\\Curves\\curve004.crv");
        var populationManager = new PopulationManager(100, track);
        for (var i = 0; i < 200; i++)
        {
            populationManager.RunSimulationParallel();
            var report = populationManager.AnalyzeGeneration();
            Console.WriteLine(report);
            populationManager.PrepareNextGeneration();  
        }

        populationManager.RunSimulationParallel();
        var report2 = populationManager.AnalyzeGeneration();
        var bestGenome = populationManager.Population[0];
        Console.WriteLine(report2);
        Console.WriteLine(bestGenome.FullDistance);
        // save in file bestGenome.Genes
        var stringGenes = string.Join(" ", bestGenome.Genes);

        Console.WriteLine(populationManager.Population[0].CheckPoints[^1]);
        Console.WriteLine(populationManager.Population[0].Car.Position);
        System.IO.File.WriteAllText("C:\\Coding\\C#\\Project One\\Project One\\Curves\\curve001.genes", stringGenes);
    }
}