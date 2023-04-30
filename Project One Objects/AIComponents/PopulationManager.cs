using System.Diagnostics;
using Project_One_Objects.Environment;
using System.Numerics;
using Microsoft.Diagnostics.Tracing.Parsers.Clr;

namespace Project_One_Objects.AIComponents;

public class PopulationManager
{
    private Genome[] _population;
    private readonly Track _progenitorTrack;
    private readonly Car _progenitorCar;
    private static readonly Random Random = new();
    private int _generation = 1;
    private bool _survivorIssue = true;
    private int _survivorIssueGeneration = 0;
    private int _maxSurvivorIssueGenerations = 10;

    /// <summary> Creates a new population manager. </summary>
    /// <param name="populationSize"> Amount of groups of size 10. </param>
    /// <param name="progenitorTrack"> The progenitorTrack to use. </param>
    public PopulationManager(int populationSize, Track progenitorTrack)
    {
        _progenitorTrack = progenitorTrack;
        _population = new Genome[populationSize * 10];

        _progenitorCar = new Car(new Vector2(0, 0), 0)
        {
            Track = _progenitorTrack,
            IsVisionActive = true
        };
        
        Console.WriteLine("Creating starting population...");
        var stopwatch = new Stopwatch();
        stopwatch.Start();
        
        // Create starting population.
        //for (var i = 0; i < _population.Length; i++)
        //{
        //    _population[i] = new Genome(new Car(_progenitorCar), Config.TickRate)
        //    {
        //        Origin = Config.OriginsKeys["Random"]
        //    };
        //    FillWithRandomGenes(_population[i].Genes);
        //}

        Parallel.For(0, _population.Length, i =>
        {
            _population[i] = new Genome(new Car(_progenitorCar), Config.TickRate)
            {
                Origin = Config.OriginsKeys["Random"]
            };
            FillWithRandomGenes(_population[i].Genes);
        });

        stopwatch.Stop();
        Console.WriteLine($"Creating starting population took {stopwatch.ElapsedMilliseconds}ms");
    }

    public Genome[] Population => _population;
    public int Generation => _generation;

    public static void FillWithRandomGenes(int[] genes)
    {
        for (var i = 0; i < genes.Length; i++) 
        {
            genes[i] = Random.Next(0, Config.CarActions.Count);
        }
    }

    public void RunSimulation()
    {
        var stopwatch = new Stopwatch();
        stopwatch.Start();
        Console.WriteLine("Starting simulation...");
        foreach (var genome in _population)
        {
            genome.Track.DropCheckpoint();
            while (genome.IsAlive)
            {
                genome.Update();
            }
        }
        stopwatch.Stop();
        Console.WriteLine($"Simulation took {stopwatch.ElapsedMilliseconds}ms");
    }

    public void RunSimulationParallel(bool optimize = false, bool print = false)
    {
        var stopwatch = new Stopwatch();
        stopwatch.Start();
        if (print)
            Console.WriteLine("Starting parallel simulation...");
        ParallelOptions parallelOptions = new();
        if (optimize)
        {
            parallelOptions.MaxDegreeOfParallelism = Config.MaxDegreeOfParallelism;
        }
        Parallel.ForEach(_population, parallelOptions, genome =>
        {
            genome.Track.DropCheckpoint();
            while (genome.IsAlive)
            {
                genome.Update();
            }
        });
        stopwatch.Stop();
        if (print)
            Console.WriteLine($"Parallel simulation took {stopwatch.ElapsedMilliseconds}ms");
    }

    /// <summary> Mutate a genome by changing a random gene to a random action. </summary>
    /// <param name="genome"> The genome to mutate. </param>
    /// <param name="mutationRate"> The chance of a gene being mutated (0-1). </param>
    public static void MutateGenome(Genome genome, float mutationRate)
    {
        for (var i = 0; i < genome.Genes.Length; i++)
        {
            if (Random.NextDouble() < mutationRate)
            {
                genome.Genes[i] = Random.Next(0, Config.CarActions.Count);
            }
        }
    }

    /// <summary> Cross over two genomes to create two new genomes. </summary>
    public static void CrossOverGenomes(Genome parent1, Genome parent2, Genome child1, Genome child2)
    {
        var crossOverPoint = Random.Next(0, parent1.Genes.Length);
        for (var i = 0; i < parent1.Genes.Length; i++)
        {
            if (i < crossOverPoint)
            {
                child1.Genes[i] = parent1.Genes[i];
                child2.Genes[i] = parent2.Genes[i];
            }
            else
            {
                child1.Genes[i] = parent2.Genes[i];
                child2.Genes[i] = parent1.Genes[i];
            }
        }
    }

    /// <summary> Randomly cross over two genomes to create two new genomes. </summary>
    public static void RandomCrossGenomes(Genome parent1, Genome parent2, Genome child1, Genome child2)
    {
        for (var i = 0; i < parent1.Genes.Length; i++)
        {
            if (Random.NextDouble() < 0.5)
            {
                child1.Genes[i] = parent1.Genes[i];
                child2.Genes[i] = parent2.Genes[i];
            }
            else
            {
                child1.Genes[i] = parent2.Genes[i];
                child2.Genes[i] = parent1.Genes[i];
            }
        }
    }

    public string AnalyzeGeneration(bool print = false)
    {
        var stopwatch = new Stopwatch();
        stopwatch.Start();

        // Sort population by fitness.
        Array.Sort(_population, (x, y) => y.Fitness.CompareTo(x.Fitness));

        var avgReport = new float[6];
        var minReport = new float[6];
        var maxReport = new float[6];
        
        for (var i = 0; i < minReport.Length; i++)
        {
            avgReport[i] = 0;
            minReport[i] = float.MaxValue;
            maxReport[i] = float.MinValue;
        }
        
        var originReport = new float[Config.OriginsKeys.Count];

        for (var i = 0; i < _population.Length * Config.BestGenomesRate; i++)
        {
            originReport[_population[i].Origin] += 1;
        }

        foreach (var genome in _population)
        {
            var genomeInfo = genome.GetInfo();
            for (var i = 0; i < genomeInfo.Length; i++)
            {
                avgReport[i] += genomeInfo[i];
                if (genomeInfo[i] < minReport[i]) minReport[i] = genomeInfo[i];
                if (genomeInfo[i] > maxReport[i]) maxReport[i] = genomeInfo[i];
            }
        }

        for (var i = 0; i < avgReport.Length; i++)
        {
            avgReport[i] /= _population.Length;
        }

        var report = $"Generation {_generation} report:\n" +
                     $"Fitness: {avgReport[0]} avg. (min: {minReport[0]}, max: {maxReport[0]})\n" +
                     $"Distance: {avgReport[1]} avg. (min: {minReport[1]}, max: {maxReport[1]})\n" +
                     $"Time: {avgReport[2]} avg. (min: {minReport[2]}, max: {maxReport[2]})\n" +
                     $"Current gene: {avgReport[3]} avg. (min: {minReport[3]}, max: {maxReport[3]})\n" +
                     $"Genes: {avgReport[4]} avg. (min: {minReport[4]}, max: {maxReport[4]})\n" +
                     $"Speed: {avgReport[5]} avg. (min: {minReport[5]}, max: {maxReport[5]})\n" +
                     $"Origins: {string.Join(", ", originReport)}\n";

        stopwatch.Stop();
        if (print)
            Console.WriteLine($"Analysis took {stopwatch.ElapsedMilliseconds}ms\n");
        return report;
    }

    public void PrepareNextGeneration(bool print = false)
    {
        var stopwatch = new Stopwatch();
        stopwatch.Start();
        
        // Create new population.
        var newPopulation = new Genome[_population.Length];
        
        var bestGenomeSize = (int) (_population.Length * Config.BestGenomesRate);
        var upIndex = bestGenomeSize;
        // Add best genomes to new population.
        if (_survivorIssueGeneration != 0 && _survivorIssue)
        {
            var half = bestGenomeSize / 2;
            for (var i = 0; i < half; i++)
            {
                newPopulation[i] = _population[Random.Next(0, _population.Length)];
                newPopulation[i].Origin = Config.OriginsKeys["SurvivorIssue"];

                newPopulation[i + half] = _population[i];
                newPopulation[i + half].Origin = Config.OriginsKeys["Best"];
            }
        }
        else
        {
            for (var i = 0; i < upIndex; i++)
            {
                newPopulation[i] = _population[i];
                newPopulation[i].Origin = Config.OriginsKeys["Best"];
            }
        }

        _survivorIssueGeneration++;
        if (_survivorIssueGeneration == _maxSurvivorIssueGenerations)
        {
            _survivorIssueGeneration = 0;
        }

        //_survivorIssue = !_survivorIssue;

        var crossGenomeSize = (int)(_population.Length * Config.CrossoverGenomesRate);
        upIndex += crossGenomeSize;
        // Add cross over genomes to new population.
        for (var i = upIndex - crossGenomeSize; i < upIndex; i += 2)
        {
            var parent1 = _population[Random.Next(0, bestGenomeSize)];
            var parent2 = _population[Random.Next(0, bestGenomeSize)];
            var child1 = new Genome(new Car(_progenitorCar), Config.TickRate)
            {
                Origin = Config.OriginsKeys["Crossover"]
            };
            var child2 = new Genome(new Car(_progenitorCar), Config.TickRate)
            {
                Origin = Config.OriginsKeys["Crossover"]
            };
            CrossOverGenomes(parent1, parent2, child1, child2);
            newPopulation[i] = child1;
            newPopulation[i + 1] = child2;
        }

        var randomCrossGenomeSize = (int)(_population.Length * Config.RandomCrossGenomesRate);
        upIndex += randomCrossGenomeSize;
        // Add random cross over genomes to new population.
        // Add cross over genomes to new population.
        for (var i = upIndex - randomCrossGenomeSize; i < upIndex; i += 2)
        {
            var parent1 = _population[Random.Next(0, bestGenomeSize)];
            var parent2 = _population[Random.Next(0, bestGenomeSize)];
            var child1 = new Genome(new Car(_progenitorCar), Config.TickRate)
            {
                Origin = Config.OriginsKeys["RandomCross"]
            };
            var child2 = new Genome(new Car(_progenitorCar), Config.TickRate)
            {
                Origin = Config.OriginsKeys["RandomCross"]
            };
            RandomCrossGenomes(parent1, parent2, child1, child2);
            newPopulation[i] = child1;
            newPopulation[i + 1] = child2;
        }
        var mutateGenomeSize = (int)(_population.Length * Config.MutatedGenomesRate);
        upIndex += mutateGenomeSize;
        // Add mutated genomes to new population.
        for (var i = upIndex - mutateGenomeSize; i < upIndex; i++)
        {
            var parent = _population[Random.Next(0, bestGenomeSize)];
            var child = new Genome(new Car(_progenitorCar), Config.TickRate)
            {
                Origin = Config.OriginsKeys["Mutate"]
            };
            Array.Copy(parent.Genes, child.Genes, parent.Genes.Length);
            MutateGenome(child, Config.MutationRate);
            newPopulation[i] = child;
        }
        
        // Add random genomes to new population.
        for (var i = upIndex; i < _population.Length; i++)
        {
            newPopulation[i] = new Genome(new Car(_progenitorCar), Config.TickRate)
            {
                Origin = Config.OriginsKeys["Random"]
            };
            FillWithRandomGenes(newPopulation[i].Genes);
        }
        
        // Replace old population with new population.
        _population = newPopulation;
        _generation++;
        
        stopwatch.Stop();
        if (print)
            Console.WriteLine($"Generation {_generation} prepared in {stopwatch.ElapsedMilliseconds}ms");
    }
}