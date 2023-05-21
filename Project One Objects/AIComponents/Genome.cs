using System.Numerics;
using Project_One_Objects.Environment;

namespace Project_One_Objects.AIComponents;

public class Genome
{
    private Car _car;
    private int _visionOptimization = 0;
    private readonly Track _track;
    private readonly Vector2[] _checkPoints;
    private bool _firstCheckpoint = true;
    public int[] Genes { get; set; }
    public short[] Values { get; set; }
    public int Origin { get; set; }
    public double Fitness;
    private readonly float _tickRate;

    private int _currentGene = 0;
    private bool _isAlive = true;
    public double[] OnCheckpointFitness { get; set; }

    private float _fullDistance;
    public Genome(Car car, float tickRate)
    {
        _car = car;
        _track = car.Track;
        _tickRate = tickRate;
        _checkPoints = _track.GetCheckpoints().ToArray();

        OnCheckpointFitness = new double[_checkPoints.Length];

        _fullDistance = Vector2.Distance(_car.Position, _checkPoints[0]);
        for (var i = 0; i < _checkPoints.Length - 1; i++)
        {
            _fullDistance += Vector2.Distance(_checkPoints[i], _checkPoints[i + 1]);
        }
        var size = (int)(_fullDistance / (car.MaxSpeed * TickTime / 1.5));
        Genes = new int[size];
        Values = new short[size];
        for (var i = 0; i < Values.Length; i++)
        {
            Values[i] = 1;
        }
    }

    public Car Car
    {
        get => _car;
        set => _car = value;
    }
    public Track Track => _track;
    public float TickRate => _tickRate;
    public float TickTime => 1 / _tickRate;
    public bool IsAlive => _isAlive;
    public float FullDistance => _fullDistance;
    public Vector2[] CheckPoints => _checkPoints;
    public int CurrentGene => _currentGene;
    public float CurrentDistance { get; set; }
    public float CurrentTime => _currentGene * TickTime;
    public float AvgSpeed => (_fullDistance - CurrentDistance) / CurrentTime;


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
        Config.CarActions[Genes[_currentGene]](_car, TickTime);
        _currentGene++;
        _car.Move(TickTime);

        if (_visionOptimization - (int)(TickTime * 1000) <= 0)
        {
            _car.UpdateVision();
            _visionOptimization = _car.VisionOptimization();
        }
        else
        {
            _visionOptimization -= (int)(TickTime * 1000);
        }

        //_car.UpdateVision();

        if (_car.IsCollision() || _currentGene >= Genes.Length)
        {
            Fitness = GetFitness();
            _track.DropCheckpoint();
            _isAlive = false;
        }
        else if (_track.OnCheckpoint(_car.Position, _car.Width))
        {
            if (_track.CurrentCheckpointIndex == 0 && !_firstCheckpoint)
            {
                Fitness = GetFitness();
                _track.DropCheckpoint();
                _isAlive = false;
                return; // to prevent IndexOutOfRangeException on line 89
            }
            
            OnCheckpointFitness[_track.CurrentCheckpointIndex - 1] = GetFitness();
            _firstCheckpoint = false;
        }
    }

    /// <summary> Calculate fitness of genome based on distance to second checkpoint and time spent. </summary>
    /// <returns> Fitness from 0 to 100 points. </returns>
    private double GetFitness()
    {
        CurrentDistance = Vector2.Distance(_car.Position, _checkPoints[_track.CurrentCheckpointIndex]);
        for (var i = _track.CurrentCheckpointIndex; i < _checkPoints.Length - 1; i++)
        {
            CurrentDistance += Vector2.Distance(_checkPoints[i], _checkPoints[i + 1]);
        }

        if (CurrentDistance > _fullDistance)
            CurrentDistance = _fullDistance;
        var distancePoints = 1000 * Math.Sqrt(1 - CurrentDistance / _fullDistance);

        var timePoints = 666 * Math.Sqrt(1 - (float)_currentGene / Genes.Length);
        //if (CurrentDistance < 10)
        //    timePoints *= 1.5f;

        var checkpointPoints = 100 * _track.CurrentCheckpointIndex / _checkPoints.Length;
        
        return distancePoints + timePoints + checkpointPoints;
    }

    /// <summary> Get information about genome. </summary>
    /// <returns> Array of floats with information about genome. </returns>
    /// <remarks>
    /// - fitness <br/>
    /// - distance to last checkpoint <br/>
    /// - time spent <br/>
    /// - current gene <br/>
    /// - total genes <br/>
    /// - origin <br/>
    /// </remarks>
    public float[] GetInfo()
    {
        return new []
        {
            (float)Fitness,
            CurrentDistance,
            CurrentTime,
            _currentGene,
            Genes.Length,
            AvgSpeed
        };
    }
}