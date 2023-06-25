using System.Diagnostics;
using Project_One_Objects.Environment;
using System.Numerics;
using Microsoft.CodeAnalysis;
using Microsoft.Diagnostics.Tracing.Parsers.Clr;

namespace Project_One_Objects.AIComponents;

public class PopulationManager
{
    private Genome[] _population;
    private int _checkpointEvolutionStep = 0;
    private readonly Track _progenitorTrack;
    private Car _progenitorCar;
    private static readonly Random Random = new();
    private int _generation = 0;

    /// <summary> Creates a new population manager. </summary>
    /// <param name="populationSize"> Amount of groups of size 10. </param>
    /// <param name="progenitorTrack"> The progenitorTrack to use. </param>
    public PopulationManager(int populationSize, Track progenitorTrack)
    {
        _progenitorTrack = progenitorTrack;
        _population = new Genome[populationSize * 10];

        _progenitorCar = new Car(new Vector2(0, 0), 0) { Track = _progenitorTrack, IsVisionActive = true };

        Console.WriteLine("Creating starting population...");
        var stopwatch = new Stopwatch();
        stopwatch.Start();

        FillRandomGeneration(_population, new Genome(_progenitorCar, Config.TickRate, _checkpointEvolutionStep));

        stopwatch.Stop();
        Console.WriteLine($"Creating starting population took {stopwatch.ElapsedMilliseconds}ms");
    }

    public Genome[] Population => _population;
    public int Generation => _generation;
    public List<int> BestGenes { get; set; } = new();

    public static void FillWithRandomGenes(Genome genome, bool setOrigin = false)
    {
        for (var i = 0; i < genome.Genes.Length; i++) 
            genome.Genes[i] = Random.Next(0, Config.CarActions.Count);

        if (setOrigin)
            genome.Origin = Config.OriginsKeys["Random"];
    }

    public static void FillRandomGeneration(Genome[] population, Genome progenitorGenome, bool setOrigin = true)
    {
        //Parallel.For(0, population.Length, i =>
        //{
        //    population[i] = new Genome(progenitorGenome);
        //    FillWithRandomGenes(population[i], setOrigin);
        //});
        //for (var i = 0; i < population.Length; i++)
        //{
        //    population[i] = new Genome(progenitorGenome);
        //    FillWithRandomGenes(population[i], setOrigin);
        //}

        //split by Config.MaxDegreeOfParallelism
        Parallel.For(0, Config.MaxDegreeOfParallelism, i =>
        {
            var startIndex = i * (population.Length / Config.MaxDegreeOfParallelism);
            var endIndex = (i + 1) * (population.Length / Config.MaxDegreeOfParallelism);
            if (i == Config.MaxDegreeOfParallelism - 1)
                endIndex = population.Length;
            for (var j = startIndex; j < endIndex; j++)
            {
                population[j] = new Genome(progenitorGenome);
                FillWithRandomGenes(population[j], setOrigin);
            }
        });
    }

    public void RunSimulation(bool print = false)
    {
        var stopwatch = new Stopwatch();
        stopwatch.Start();
        if (print)
            Console.WriteLine("Starting simulation...");
        foreach (var genome in _population)
        {
            genome.Track.CurrentCheckpointIndex = _checkpointEvolutionStep;
            while (genome.IsAlive)
            {
                genome.Update();
            }
        }
        stopwatch.Stop();
        if (print)
            Console.WriteLine($"Simulation took {stopwatch.ElapsedMilliseconds}ms");
    }

    public void RunSimulationParallel(bool optimize = false, bool print = false)
    {
        var stopwatch = new Stopwatch();
        stopwatch.Start();
        if (print)
            Console.WriteLine("Starting parallel simulation...");
        
        if (optimize)
        {
            Parallel.ForEach(_population, Config.OptimizationOptions, genome =>
            {
                genome.Track.CurrentCheckpointIndex = _checkpointEvolutionStep;
                while (genome.IsAlive)
                {
                    genome.Update();
                }
            });
        }
        else
        {
            Parallel.ForEach(_population, genome =>
            {
                genome.Track.CurrentCheckpointIndex = _checkpointEvolutionStep;
                while (genome.IsAlive)
                {
                    genome.Update();
                }
            });
        }

        stopwatch.Stop();
        if (print)
            Console.WriteLine($"Parallel simulation took {stopwatch.ElapsedMilliseconds}ms");
    }

    /// <summary> Mutate a genome by changing a random gene to a random action. </summary>
    public void MutateGenome(Genome genome, float mutationRate, bool setOrigin = false)
    {
        for (var i = 0; i < genome.Genes.Length; i++)
        {
            if (Random.NextDouble() < mutationRate)
            {
                genome.Genes[i] = Random.Next(0, Config.CarActions.Count);
                genome.Values[i] = (short)(Random.NextInt64(1, Math.Max(1, Generation / 2)));
            }
        }

        if (setOrigin)
            genome.Origin = Config.OriginsKeys["Mutate"];
    }

    /// <summary> Cross over two genomes to create two new genomes. </summary>
    public static void CrossOverGenomes(Genome parent1, Genome parent2, Genome child1, Genome child2, bool setOrigin = false)
    {
        var crossOverPoint = Random.Next(0, parent1.Genes.Length);
        var maxIndex = new[] { parent1.Genes.Length, child1.Genes.Length, parent2.Genes.Length, child2.Genes.Length }.Min();
        for (var i = 0; i < maxIndex; i++)
        {
            if (i < crossOverPoint)
            {
                child1.Genes[i] = parent1.Genes[i];
                child2.Genes[i] = parent2.Genes[i];
                child1.Values[i] = parent1.Values[i];
                child2.Values[i] = parent2.Values[i];
            }
            else
            {
                child1.Genes[i] = parent2.Genes[i];
                child2.Genes[i] = parent1.Genes[i];
                child1.Values[i] = parent2.Values[i];
                child2.Values[i] = parent1.Values[i];
            }
        }

        if (setOrigin)
        {
            child1.Origin = Config.OriginsKeys["Crossover"];
            child2.Origin = Config.OriginsKeys["Crossover"];
        }
    }

    /// <summary> Randomly cross over two genomes to create two new genomes. </summary>
    public static void RandomCrossGenomes(Genome parent1, Genome parent2, Genome child1, Genome child2, bool setOrigin = false)
    {
        var maxIndex = new[] { parent1.Genes.Length, child1.Genes.Length, parent2.Genes.Length, child2.Genes.Length }.Min();
        for (var i = 0; i < maxIndex; i++)
        {
            if (Random.NextDouble() < 0.5)
            {
                child1.Genes[i] = parent1.Genes[i];
                child2.Genes[i] = parent2.Genes[i];
                child1.Values[i] = parent1.Values[i];
                child2.Values[i] = parent2.Values[i];
            }
            else
            {
                child1.Genes[i] = parent2.Genes[i];
                child2.Genes[i] = parent1.Genes[i];
                child1.Values[i] = parent2.Values[i];
                child2.Values[i] = parent1.Values[i];
            }
        }

        if (setOrigin)
        {
            child1.Origin = Config.OriginsKeys["RandomCrossover"];
            child2.Origin = Config.OriginsKeys["RandomCrossover"];
        }
    }

    /// <summary> Cross over genomes by value of their genes. </summary>
    public static void ValueCrossGenomes(Genome parent1, Genome parent2, Genome child, bool setOrigin = false)
    {
        var maxIndex = new[] { parent1.Genes.Length, parent2.Genes.Length, child.Genes.Length }.Min();
        for (var i = 0; i < maxIndex; i++)
        {
            if (parent1.Values[i] > parent2.Values[i])
            {
                child.Genes[i] = parent1.Genes[i];
                child.Values[i] = parent1.Values[i];
            }
            else
            {
                child.Genes[i] = parent2.Genes[i];
                child.Values[i] = parent2.Values[i];
            }
        }

        if (setOrigin)
            child.Origin = Config.OriginsKeys["ValueCrossover"];
    }

    public static void SmoothCrossGenomes(Genome parent1, Genome parent2, Genome child, bool setOrigin = false)
    {
        var maxIndex = new[] { parent1.Genes.Length, parent2.Genes.Length, child.Genes.Length }.Min();
        for (var i = 0; i < maxIndex; i++)
        {
            // rules
            // 1 + 2 => 0
            // 3 + 4 => 0
            // 5 + 6 => 1
            // 7 + 8 => 2
            // 5 + 7 => 3
            // 6 + 8 => 4

            var action = parent1.Genes[i] + parent2.Genes[i];
            child.Genes[i] = action switch
            {
                3 => 0,
                7 => 0,
                11 => 1,
                15 => 2,
                12 => 3,
                14 => 4,
                _ => child.Genes[i]
            };
        }

        if (setOrigin)
            child.Origin = Config.OriginsKeys["SmoothCrossover"];
    }

    public string AnalyzeGeneration(bool print = false)
    {
        var stopwatch = new Stopwatch();
        stopwatch.Start();

        // Sort population by fitness.
        Array.Sort(_population, (x, y) => y.Fitness.CompareTo(x.Fitness));

        var avgReport = new float[5];
        var minReport = new float[5];
        var maxReport = new float[5];
        
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
                     $"Speed: {avgReport[4]} avg. (min: {minReport[4]}, max: {maxReport[4]})\n" +
                     $"Origins: {string.Join(", ", originReport)}\n";

        stopwatch.Stop();
        if (print)
            Console.WriteLine($"Analysis took {stopwatch.ElapsedMilliseconds}ms\n");
        return report;
    }

    private Genome GetProgenitorGenome()
    {
        return new Genome(new Car(_progenitorCar), Config.TickRate, _checkpointEvolutionStep);
    }

    /// <summary> Prepare next generation of genomes. </summary>
    /// <param name="specialAction"> 0 - nothing, 1 - save best genes, 2 - reset population </param>
    /// <param name="print"> </param>
    /// <returns> True if next generation is possible, false otherwise. </returns>
    public bool PrepareNextGeneration(int specialAction = 0, bool print = false)
    {
        var stopwatch = new Stopwatch();
        stopwatch.Start();

        switch (specialAction)
        {
            case 0:
                break;
            case 1:
                var bestGenome = _population[0];
                _progenitorCar = bestGenome.MiddleCarState!;
                BestGenes.AddRange(bestGenome.Genes.Take(bestGenome.MiddleGeneIndex));
            
                if (_checkpointEvolutionStep + Config.StepWidth + 1 == _progenitorTrack.Checkpoints.Length)
                {
                    return false;
                }
                
                FillRandomGeneration(_population, GetProgenitorGenome());

                _checkpointEvolutionStep++;
                break;
            case 2:

                FillRandomGeneration(_population, GetProgenitorGenome());
                break;
        }

        // Create new population.
        var newPopulation = new Genome[_population.Length];

        var bestGenomeSize = (int) (_population.Length * Config.BestGenomesRate);
        var shift = 0;
        for (var i = shift; i < shift + bestGenomeSize; i++)
        {
            newPopulation[i] = _population[i];
            newPopulation[i].Origin = Config.OriginsKeys["Best"];
        }
        shift += bestGenomeSize;

        var crossGenomeSize = (int)(_population.Length * Config.CrossoverGenomesRate);
        for (var i = shift; i < shift + crossGenomeSize; i += 2)
        {
            var parent1 = _population[Random.Next(0, bestGenomeSize)];
            var parent2 = _population[Random.Next(0, bestGenomeSize)];
            var child1 = new Genome(new Car(_progenitorCar), Config.TickRate, _checkpointEvolutionStep);
            var child2 = new Genome(new Car(_progenitorCar), Config.TickRate, _checkpointEvolutionStep);
            CrossOverGenomes(parent1, parent2, child1, child2, true);
            newPopulation[i] = child1;
            newPopulation[i + 1] = child2;
        }
        shift += crossGenomeSize;

        var randomCrossGenomeSize = (int)(_population.Length * Config.RandomCrossGenomesRate);
        for (var i = shift; i < shift + randomCrossGenomeSize; i += 2)
        {
            var parent1 = _population[Random.Next(0, bestGenomeSize)];
            var parent2 = _population[Random.Next(0, bestGenomeSize)];
            var child1 = new Genome(new Car(_progenitorCar), Config.TickRate, _checkpointEvolutionStep);
            var child2 = new Genome(new Car(_progenitorCar), Config.TickRate, _checkpointEvolutionStep);
            RandomCrossGenomes(parent1, parent2, child1, child2, true);
            newPopulation[i] = child1;
            newPopulation[i + 1] = child2;
        }
        shift += randomCrossGenomeSize;

        var valueCrossGenomeSize = (int)(_population.Length * Config.ValueCrossGenomesRate);
        for (var i = shift; i < shift + valueCrossGenomeSize; i++)
        {
            var parent1 = _population[Random.Next(0, bestGenomeSize)];
            var parent2 = _population[Random.Next(0, bestGenomeSize)];
            var child = new Genome(new Car(_progenitorCar), Config.TickRate, _checkpointEvolutionStep);
            ValueCrossGenomes(parent1, parent2, child, true);
            newPopulation[i] = child;
        }
        shift += valueCrossGenomeSize;

        var smoothCrossGenomeSize = (int)(_population.Length * Config.SmoothCrossGenomesRate);
        for (var i = shift; i < shift + smoothCrossGenomeSize; i++)
        {
            var parent1 = _population[Random.Next(0, bestGenomeSize)];
            var parent2 = _population[Random.Next(0, bestGenomeSize)];
            var child = new Genome(new Car(_progenitorCar), Config.TickRate, _checkpointEvolutionStep);
            SmoothCrossGenomes(parent1, parent2, child, true);
            newPopulation[i] = child;
        }
        shift += smoothCrossGenomeSize;

        var mutateGenomeSize = (int)(_population.Length * Config.MutatedGenomesRate);
        for (var i = shift; i < shift + mutateGenomeSize; i++)
        {
            var parent = _population[Random.Next(0, bestGenomeSize)];
            var child = new Genome(new Car(_progenitorCar), Config.TickRate, _checkpointEvolutionStep);
            Array.Copy(parent.Genes, child.Genes, Math.Min(parent.Genes.Length, child.Genes.Length));
            MutateGenome(child, Config.MutationRate, true);
            newPopulation[i] = child;
        }
        shift += mutateGenomeSize;

        // Add random genomes to new population.
        for (var i = shift; i < _population.Length; i++)
        {
            newPopulation[i] = new Genome(new Car(_progenitorCar), Config.TickRate, _checkpointEvolutionStep);
            FillWithRandomGenes(newPopulation[i], true);
        }

        // Replace old population with new population.
        _population = newPopulation;
        _generation++;
        
        stopwatch.Stop();
        if (print)
            Console.WriteLine($"Generation {_generation} prepared in {stopwatch.ElapsedMilliseconds}ms");

        return true;
    }
}