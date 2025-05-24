using System.Diagnostics;
using System.Numerics;
using Project_One_Objects.Environment;
using Project_One_Objects.Helpers;

namespace Project_One_Objects.AIComponents;

public class PopulationManager
{
    private static readonly Random Random = new();
    private readonly Car _progenitorCar;
    private readonly Track _progenitorTrack;

    /// <summary> Creates a new population manager. </summary>
    /// <param name="populationSize"> Amount of groups of size 10. </param>
    /// <param name="progenitorTrack"> The progenitorTrack to use. </param>
    public PopulationManager(int populationSize, Track progenitorTrack)
    {
        _progenitorTrack = progenitorTrack;
        Population = new Genome[populationSize * 10];

        _progenitorCar = new Car(new Vector2(0, 0), 0) { Track = _progenitorTrack, IsVisionActive = true };

        Console.WriteLine("Creating starting population...");
        Stopwatch stopwatch = new();
        stopwatch.Start();

        var progenitorGenome = GetProgenitorGenome();
        for (var i = 0; i < Population.Length; i++)
        {
            Population[i] = new Genome(progenitorGenome);
            FillWithRandomGenes(Population[i], true);
        }
        //FillRandomGeneration(Population, GetProgenitorGenome());

        stopwatch.Stop();
        Console.WriteLine($"Creating starting population took {stopwatch.ElapsedMilliseconds}ms");
    }

    public int EvolutionStep { get; private set; }
    public int StepsCount => _progenitorTrack.Checkpoints.Length;

    public Genome[] Population { get; private set; }
    public int Generation { get; private set; }
    public List<int> BestGenes { get; set; } = new();

    public static void FillWithRandomGenes(Genome genome, bool setOrigin = false)
    {
        var start = Random.Next(0, MathExtensions.PseudoRandomCarActions.Length - genome.Genes.Length);
        Array.Copy(MathExtensions.PseudoRandomCarActions, start, genome.Genes, 0, genome.Genes.Length);

        if (setOrigin) genome.Origin = Config.Origin.Random;
    }

    public static void FillRandomGeneration(Genome[] population, Car progenitorCar, int evolutionStep, bool setOrigin = true)
    {
        foreach (var genome in population)
        {
            progenitorCar.CopyStateTo(genome.Car);
            genome.EvolutionStep = evolutionStep;
            FillWithRandomGenes(genome, setOrigin);
        }
    }

    public void RunSimulationParallel(bool print = false)
    {
        Stopwatch stopwatch = new();
        stopwatch.Start();
        if (print) Console.WriteLine("Starting parallel simulation...");

        Parallel.ForEach(Population, Config.OptimizationOptions, genome =>
        {
            genome.Track.CurrentCheckpointIndex = EvolutionStep;
            while (genome.IsAlive) genome.Update();
        });

        stopwatch.Stop();
        if (print) Console.WriteLine($"Parallel simulation took {stopwatch.ElapsedMilliseconds}ms");
    }

    /// <summary> Mutate a genome by changing a random gene to a random action. </summary>
    public void MutateGenome(Genome genome, float mutationRate, bool setOrigin = false)
    {
        var count = (int)(genome.Genes.Length * mutationRate);
        var start = Random.Next(0, MathExtensions.PseudoRandomCarActions.Length - count);

        for (var i = 0; i < count; i++)
        {
            var index = Random.Next(0, genome.Genes.Length);

            genome.Genes[index] = MathExtensions.PseudoRandomCarActions[start + i];
            genome.Values[index] = (short)Random.NextInt64(1, Math.Max(1, Generation / 2));
        }

        if (setOrigin) genome.Origin = Config.Origin.Mutate;
    }

    /// <summary> Cross over two genomes to create two new genomes. </summary>
    public static void CrossOverGenomes(Genome parent1, Genome parent2, Genome child1, Genome child2,
        bool setOrigin = false)
    {
        var maxIndex = new[] { parent1.Genes.Length, child1.Genes.Length, parent2.Genes.Length, child2.Genes.Length }
            .Min();
        var crossOverPoint = Random.Next(0, maxIndex);
        for (var i = 0; i < crossOverPoint; i++)
        {
            child1.Genes[i] = parent1.Genes[i];
            child2.Genes[i] = parent2.Genes[i];
            child1.Values[i] = parent1.Values[i];
            child2.Values[i] = parent2.Values[i];
        }

        for (var i = crossOverPoint; i < maxIndex; i++)
        {
            child1.Genes[i] = parent2.Genes[i];
            child2.Genes[i] = parent1.Genes[i];
            child1.Values[i] = parent2.Values[i];
            child2.Values[i] = parent1.Values[i];
        }

        if (setOrigin)
        {
            child1.Origin = Config.Origin.Crossover;
            child2.Origin = Config.Origin.Crossover;
        }
    }

    /// <summary> Randomly cross over two genomes to create two new genomes. </summary>
    public static void RandomCrossGenomes(Genome parent1, Genome parent2, Genome child1, Genome child2,
        bool setOrigin = false)
    {
        var maxIndex = new[] { parent1.Genes.Length, child1.Genes.Length, parent2.Genes.Length, child2.Genes.Length }
            .Min();
        var start = Random.Next(0, MathExtensions.PseudoRandomDouble.Length - maxIndex);
        for (var i = 0; i < maxIndex; i++)
            if (MathExtensions.PseudoRandomDouble[start + i] < 0.5f)
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

        if (setOrigin)
        {
            child1.Origin = Config.Origin.RandomCrossover;
            child2.Origin = Config.Origin.RandomCrossover;
        }
    }

    /// <summary> Cross over genomes by value of their genes. </summary>
    public static void ValueCrossGenomes(Genome parent1, Genome parent2, Genome child, bool setOrigin = false)
    {
        var maxIndex = new[] { parent1.Genes.Length, parent2.Genes.Length, child.Genes.Length }.Min();
        for (var i = 0; i < maxIndex; i++)
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

        if (setOrigin) child.Origin = Config.Origin.ValueCrossover;
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

            var sum = parent1.Genes[i] + parent2.Genes[i];
            child.Genes[i] = Config.SumToAction[sum];
        }

        if (setOrigin) child.Origin = Config.Origin.SmoothCrossover;
    }

    public string AnalyzeGeneration(bool makeReport = false, bool print = false)
    {
        Stopwatch stopwatch = new();
        stopwatch.Start();

        // Sort population by fitness.
        Array.Sort(Population, (x, y) => y.Fitness.CompareTo(x.Fitness));

        if (!makeReport) return "";

        var avgReport = new float[5];
        var minReport = new float[5];
        var maxReport = new float[5];

        for (var i = 0; i < minReport.Length; i++)
        {
            avgReport[i] = 0;
            minReport[i] = float.MaxValue;
            maxReport[i] = float.MinValue;
        }

        // var originReport = new float[Config.OriginsKeys.Count];
        var originReport = new Dictionary<Config.Origin, int>();
        foreach (var origin in Enum.GetValues<Config.Origin>()) originReport[origin] = 0;

        for (var i = 0; i < Population.Length * Config.BestGenomesRate; i++) originReport[Population[i].Origin] += 1;

        foreach (var genome in Population)
        {
            var genomeInfo = genome.GetInfo();
            for (var i = 0; i < genomeInfo.Length; i++)
            {
                avgReport[i] += genomeInfo[i];
                if (genomeInfo[i] < minReport[i]) minReport[i] = genomeInfo[i];

                if (genomeInfo[i] > maxReport[i]) maxReport[i] = genomeInfo[i];
            }
        }

        for (var i = 0; i < avgReport.Length; i++) avgReport[i] /= Population.Length;

        var report = $"Generation {Generation} report:\n" +
                     $"Fitness: {avgReport[0]} avg. (min: {minReport[0]}, max: {maxReport[0]})\n" +
                     $"Distance: {avgReport[1]} avg. (min: {minReport[1]}, max: {maxReport[1]})\n" +
                     $"Time: {avgReport[2]} avg. (min: {minReport[2]}, max: {maxReport[2]})\n" +
                     $"Current gene: {avgReport[3]} avg. (min: {minReport[3]}, max: {maxReport[3]})\n" +
                     $"Speed: {avgReport[4]} avg. (min: {minReport[4]}, max: {maxReport[4]})\n" +
                     $"Origins: {string.Join(", ", originReport)}\n";

        stopwatch.Stop();
        if (print) Console.WriteLine($"Analysis took {stopwatch.ElapsedMilliseconds}ms\n");

        return report;
    }

    private Genome GetProgenitorGenome()
    {
        return new Genome(new Car(_progenitorCar), Config.TickRate, EvolutionStep);
    }

    /// <summary> Prepare next generation of genomes. </summary>
    /// <param name="specialAction"> 0 - nothing, 1 - save best genes, 2 - reset population </param>
    /// <param name="print"> </param>
    /// <returns> True if next generation is possible, false otherwise. </returns>
    public bool PrepareNextGeneration(int specialAction = 0, bool print = false)
    {
        Stopwatch stopwatch = new();
        stopwatch.Start();

        switch (specialAction)
        {
            case 0:
                break;
            case 1:
                var bestGenome = Population[0];
                _progenitorCar.SetState(bestGenome.MiddleCarState);
                for (var i = 0; i < bestGenome.MiddleGeneIndex; i++)
                    BestGenes.Add(bestGenome.Genes[i]);

                if (EvolutionStep + Config.StepWidth + 1 == StepsCount)
                    return false;

                FillRandomGeneration(Population, _progenitorCar, EvolutionStep);
                EvolutionStep++;
                break;
            case 2:
                FillRandomGeneration(Population, _progenitorCar, EvolutionStep);
                break;
        }

        var bestSize = (int)(Population.Length * Config.BestGenomesRate);
        var shift = 0;

        // Set origin for best genomes
        for (var i = shift; i < shift + bestSize; i++)
        {
            for (var j = 0; j < Population[i].Values.Length; j++) Population[i].Values[j] += 1;
            Population[i].Origin = Config.Origin.Best;
        }

        shift += bestSize;

        // Generate genomes through crossover, random crossover, value crossover, and smooth crossover
        var crossoverSizes = new[] {
            (int)(Population.Length * Config.CrossoverGenomesRate),
            (int)(Population.Length * Config.RandomCrossGenomesRate),
            (int)(Population.Length * Config.ValueCrossGenomesRate),
            (int)(Population.Length * Config.SmoothCrossGenomesRate)
        };

        for (var c = 0; c < crossoverSizes.Length; c++)
        {
            var size = crossoverSizes[c];

            for (var i = shift; i < shift + size; i += (c < 2) ? 2 : 1)
            {
                var parent1 = Population[Random.Next(0, bestSize)];
                var parent2 = Population[Random.Next(0, bestSize)];
                var child1 = Population[i];
                var child2 = (c < 2) ? Population[i + 1] : null;

                _progenitorCar.CopyStateTo(child1.Car);
                child1.EvolutionStep = EvolutionStep;

                if (child2 != null)
                {
                    _progenitorCar.CopyStateTo(child2.Car);
                    child2.EvolutionStep = EvolutionStep;
                }

                switch (c)
                {
                    case 0: CrossOverGenomes(parent1, parent2, child1, child2!, true); break;
                    case 1: RandomCrossGenomes(parent1, parent2, child1, child2!, true); break;
                    case 2: ValueCrossGenomes(parent1, parent2, child1, true); break;
                    case 3: SmoothCrossGenomes(parent1, parent2, child1, true); break;
                }
            }

            shift += size;
        }

        // Generate mutated genomes and fill remaining with random genomes
        var mutatedSize = (int)(Population.Length * Config.MutatedGenomesRate);

        for (var i = shift; i < shift + mutatedSize; i++)
        {
            var parent = Population[Random.Next(0, bestSize)];
            var child = Population[i];

            _progenitorCar.CopyStateTo(child.Car);
            child.EvolutionStep = EvolutionStep;
            Array.Copy(parent.Genes, child.Genes, Math.Min(parent.Genes.Length, child.Genes.Length));
            MutateGenome(child, Config.MutationRate, true);
        }

        shift += mutatedSize;

        for (var i = shift; i < Population.Length; i++)
        {
            _progenitorCar.CopyStateTo(Population[i].Car);
            Population[i].EvolutionStep = EvolutionStep;
            FillWithRandomGenes(Population[i], true);
        }

        Generation++;

        stopwatch.Stop();
        if (print) Console.WriteLine($"Generation {Generation} prepared in {stopwatch.ElapsedMilliseconds}ms");

        return true;
    }
}