using System.Diagnostics;
using System.Numerics;
using Project_One_Objects.Environment;

namespace Project_One_Objects.AIComponents;

public class Genome
{
    public Car Car { get; }
    private readonly Track _track;
    private readonly Vector2[] _checkpoints;
    private bool _firstCheckpoint = true;
    private readonly int _checkpointEvolutionStep = 0;
    public int MiddleGeneIndex = -1;
    public bool OnNextCheckpoint = false;
    public Car? MiddleCarState = null;
    public int[] Genes { get; set; }
    public short[] Values { get; set; }
    public int Origin { get; set; }
    public double Fitness;
    private readonly float _tickRate;

    private int _currentGene = 0;
    private bool _isAlive = true;

    private readonly float _fullDistance = 0;
    public Genome(Car car, float tickRate, int checkpointEvolutionStep)
    {
        Car = car;
        _track = car.Track;
        _tickRate = tickRate;
        _checkpointEvolutionStep = checkpointEvolutionStep;
        _checkpoints = _track.CheckpointCenters;

        _fullDistance = Vector2.Distance(Car.Position, _checkpoints[_checkpointEvolutionStep]);
        for (var i = _checkpointEvolutionStep; i < checkpointEvolutionStep + Config.StepWidth; i++)
            _fullDistance += Vector2.Distance(_checkpoints[i], _checkpoints[i + 1]);

        var size = (int)(_fullDistance / (car.MaxSpeed * TickTime / 4));
        Genes = new int[size];
        Values = new short[size];
        for (var i = 0; i < Values.Length; i++)
            Values[i] = 1;
    }

    public Genome(Genome genome, bool copyArrays = false)
    {
        Car = new Car(genome.Car);
        _track = genome._track;
        _tickRate = genome._tickRate;
        _checkpointEvolutionStep = genome._checkpointEvolutionStep;
        _checkpoints = genome._checkpoints;
        _fullDistance = genome._fullDistance;
        Genes = new int[genome.Genes.Length];
        Values = new short[genome.Values.Length];
        if (!copyArrays) return;
        genome.Genes.CopyTo(Genes, 0);
        genome.Values.CopyTo(Values, 0);
    }

    public Track Track => _track;
    public float TickRate => _tickRate;
    public float TickTime => 1 / _tickRate;
    public bool IsAlive => _isAlive;
    public float CurrentDistance { get; set; }
    public float CurrentTime => _currentGene * TickTime;
    public List<float> Speeds { get; set; } = new();
    public float AvgSpeed => Speeds.Average();

    /// <summary>
    /// Perform one tick of simulation.
    /// </summary>
    /// <remarks>
    /// - call action based on current gene and move car. <br/>
    /// - update vision and check for collision. <br/>
    /// - if collision or second checkpoint reached, <br/>
    /// calculate fitness and prevent further updates. <br/>
    /// </remarks>
    public void Update()
    {
        if (!_isAlive) return;
        Config.CarActions[Genes[_currentGene]](Car, TickTime);
        _currentGene++;
        Car.Move(TickTime);

        Speeds.Add(Car.Speed);

        Car.UpdateVisionOpt(TickTime);

        if (Car.IsCollision() || _currentGene + 1 == Genes.Length)
        {
            Fitness = GetFitness();
            _track.CurrentCheckpointIndex = _checkpointEvolutionStep;
            _isAlive = false;
        }
        else if (_track.OnCheckpoint(Car.Position, Car.Width))
        {
            if (_track.CurrentCheckpointIndex == _checkpointEvolutionStep + 1)
            {
                MiddleGeneIndex = _currentGene;
                MiddleCarState = new Car(Car);
            }

            if (_track.CurrentCheckpointIndex == _checkpointEvolutionStep + Config.StepWidth && !_firstCheckpoint
                && MiddleCarState is not null)
            {
                OnNextCheckpoint = true;
                Fitness = GetFitness();
                _track.CurrentCheckpointIndex = _checkpointEvolutionStep;
                _isAlive = false;
            }
            
            _firstCheckpoint = false;
        }
    }

    /// <summary> Calculate fitness of genome based on distance to second checkpoint and time spent. </summary>
    /// <returns> Fitness from 0 to 100 points. </returns>
    private double GetFitness()
    {
        CurrentDistance = Vector2.Distance(Car.Position, _checkpoints[_track.CurrentCheckpointIndex]);
        for (var i = _track.CurrentCheckpointIndex; i < _checkpointEvolutionStep + Config.StepWidth; i++)
        {
            CurrentDistance += Vector2.Distance(_checkpoints[i], _checkpoints[i + 1]);
        }

        if (CurrentDistance > _fullDistance)
            CurrentDistance = _fullDistance;

        var distancePoints = 1 - CurrentDistance / _fullDistance;
        var timePoints =  (double)_currentGene / Genes.Length;
        var avgSpeedPoints = (double)AvgSpeed / Car.MaxSpeed;

        return distancePoints + avgSpeedPoints * (OnNextCheckpoint ? 1 : 0) - timePoints;
    }

    /// <summary> Get information about genome. </summary>
    /// <returns> Array of floats with information about genome. </returns>
    /// <remarks>
    /// - fitness <br/>
    /// - distance to last checkpoint <br/>
    /// - time spent <br/>
    /// - current gene <br/>
    /// - average speed <br/>
    /// </remarks>
    public float[] GetInfo()
    {
        return new []
        {
            (float)Fitness,
            CurrentDistance,
            CurrentTime,
            _currentGene,
            AvgSpeed
        };
    }
}