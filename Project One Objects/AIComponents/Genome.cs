using System.Numerics;
using Project_One_Objects.Environment;

namespace Project_One_Objects.AIComponents;

public class Genome
{
    private readonly int _checkpointEvolutionStep;

    private readonly Vector2[] _checkpoints;
    private readonly float _fullDistance;
    private int _currentGene;
    private bool _firstCheckpoint = true;
    public double Fitness;
    public Car.State MiddleCarState;
    public int MiddleGeneIndex = -1;
    public bool OnNextCheckpoint;

    public Genome(Car car, float tickRate, int checkpointEvolutionStep)
    {
        Car = car;
        Track = car.Track;
        TickRate = tickRate;
        _checkpointEvolutionStep = checkpointEvolutionStep;
        _checkpoints = Track.CheckpointCenters;

        _fullDistance = Vector2.Distance(Car.Position, _checkpoints[_checkpointEvolutionStep]);
        for (var i = _checkpointEvolutionStep; i < checkpointEvolutionStep + Config.StepWidth; i++)
            _fullDistance += Vector2.Distance(_checkpoints[i], _checkpoints[i + 1]);

        var size = (int)(_fullDistance / (car.MaxSpeed * TickTime / 4));
        Genes = new int[size];
        Values = new short[size];
        for (var i = 0; i < Values.Length; i++) Values[i] = 1;
    }

    public Genome(Genome genome, bool copyArrays = false)
    {
        Car = new Car(genome.Car);
        Track = genome.Track;
        TickRate = genome.TickRate;
        _checkpointEvolutionStep = genome._checkpointEvolutionStep;
        _checkpoints = genome._checkpoints;
        _fullDistance = genome._fullDistance;
        Genes = new int[genome.Genes.Length];
        Values = new short[genome.Values.Length];
        if (!copyArrays) return;

        genome.Genes.CopyTo(Genes, 0);
        genome.Values.CopyTo(Values, 0);
    }

    public Car Car { get; }
    public int[] Genes { get; set; }
    public short[] Values { get; set; }
    public int Origin { get; set; }

    public Track Track { get; }
    public float TickRate { get; }
    public float TickTime => 1 / TickRate;
    public bool IsAlive { get; private set; } = true;
    public float CurrentDistance { get; set; }
    public float CurrentTime => _currentGene * TickTime;
    public List<float> Speeds { get; set; } = new();
    public float AvgSpeed => Speeds.Average();

    /// <summary>
    ///     Perform one tick of simulation.
    /// </summary>
    /// <remarks>
    ///     - call action based on current gene and move car. <br />
    ///     - update vision and check for collision. <br />
    ///     - if collision or second checkpoint reached, <br />
    ///     calculate fitness and prevent further updates. <br />
    /// </remarks>
    public void Update()
    {
        if (!IsAlive) return;

        Config.CarActions[Genes[_currentGene]](Car, TickTime);
        _currentGene++;
        Car.Move(TickTime);

        Speeds.Add(Car.Speed);

        Car.UpdateVisionOpt(TickTime);

        if (Car.IsCollision() || _currentGene + 1 == Genes.Length)
        {
            Fitness = GetFitness();
            Track.CurrentCheckpointIndex = _checkpointEvolutionStep;
            IsAlive = false;
        }
        else if (Track.OnCheckpoint(Car.Position, Car.Width))
        {
            if (Track.CurrentCheckpointIndex == _checkpointEvolutionStep + 1)
            {
                MiddleGeneIndex = _currentGene;
                MiddleCarState = Car.GetState();
            }

            if (Track.CurrentCheckpointIndex == _checkpointEvolutionStep + Config.StepWidth && !_firstCheckpoint
                && MiddleGeneIndex != -1)
            {
                OnNextCheckpoint = true;
                Fitness = GetFitness();
                Track.CurrentCheckpointIndex = _checkpointEvolutionStep;
                IsAlive = false;
            }

            _firstCheckpoint = false;
        }
    }

    /// <summary> Calculate fitness of genome based on distance to second checkpoint and time spent. </summary>
    /// <returns> Fitness from 0 to 100 points. </returns>
    private double GetFitness()
    {
        CurrentDistance = Vector2.Distance(Car.Position, _checkpoints[Track.CurrentCheckpointIndex]);
        for (var i = Track.CurrentCheckpointIndex; i < _checkpointEvolutionStep + Config.StepWidth; i++)
            CurrentDistance += Vector2.Distance(_checkpoints[i], _checkpoints[i + 1]);

        if (CurrentDistance > _fullDistance) CurrentDistance = _fullDistance;

        var distancePoints = 1 - CurrentDistance / _fullDistance;
        var timePoints = (double)_currentGene / Genes.Length;
        var avgSpeedPoints = (double)AvgSpeed / Car.MaxSpeed;

        return distancePoints + avgSpeedPoints * (OnNextCheckpoint ? 1 : 0) - timePoints;
    }

    /// <summary> Get information about genome. </summary>
    /// <returns> Array of floats with information about genome. </returns>
    /// <remarks>
    ///     - fitness <br />
    ///     - distance to last checkpoint <br />
    ///     - time spent <br />
    ///     - current gene <br />
    ///     - average speed <br />
    /// </remarks>
    public float[] GetInfo()
    {
        return new[]
        {
            (float)Fitness,
            CurrentDistance,
            CurrentTime,
            _currentGene,
            AvgSpeed
        };
    }
}