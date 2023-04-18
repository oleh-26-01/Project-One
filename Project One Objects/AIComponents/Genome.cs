using System.Numerics;
using Project_One_Objects.Environment;

namespace Project_One_Objects.AIComponents;

public class Genome
{
    private readonly Car _car;
    private readonly Track _track;
    private readonly Vector2[] _checkPoints;
    private int _targetCheckPointIndex;
    public int[] Genes { get; set; }
    public double Fitness;
    private readonly float _tickRate;

    private static readonly Dictionary<int, Action<Car, float>> CarActions = new()
    {
        { 0, (_, _) => { } },
        { 1, (car, dt) => car.SpeedUp(dt) },
        { 2, (car, dt) => car.SpeedDown(dt) },
        { 3, (car, dt) => car.TurnLeft(dt) },
        { 4, (car, dt) => car.TurnRight(dt) },
        {
            5, (car, dt) =>
            {
                car.TurnLeft(dt);
                car.SpeedUp(dt);

            }
        },
        {
            6, (car, dt) =>
            {
                car.TurnRight(dt);
                car.SpeedUp(dt);
            }
        },
        {
            7, (car, dt) =>
            {
                car.TurnLeft(dt);
                car.SpeedDown(dt);
            }
        },
        {
            8, (car, dt) =>
            {
                car.TurnRight(dt);
                car.SpeedDown(dt);
            }
        }
    };
    private int _currentGene = 0;

    private float _fullDistance;

    public Genome(Car car, Track track, float tickRate)
    {
        _car = car;
        _track = track;
        _tickRate = tickRate;
        _checkPoints = _track.GetCheckpoints().ToArray();

        _fullDistance = Vector2.Distance(_car.Position, _checkPoints[0]) +
                       Vector2.Distance(_checkPoints[0], _checkPoints[1]);
        var size = (int)(_fullDistance / (car.MaxSpeed * TickTime / 2));
        Genes = new int[size];
    }

    public Car Car => _car;
    public Track Track => _track;
    public float TickRate => _tickRate;
    public float TickTime => 1 / _tickRate;

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
        if (_currentGene == -1) return;
        CarActions[Genes[_currentGene]](_car, TickTime);
        _currentGene++;
        _car.Move(TickTime);
        _car.UpdateVision();
        if (_car.IsCollision())
        {
            Fitness = GetFitness();
            _currentGene = -1;
        }
        else if (_track.OnCheckpoint(_car.Position, _car.Width))
        {
            if (_targetCheckPointIndex == 1)
            {
                Fitness = GetFitness();
                _currentGene = -1;
            }
            _targetCheckPointIndex++;

        }
    }

    /// <summary> Calculate fitness of genome based on distance to second checkpoint and time spent. </summary>
    /// <returns> Fitness from 0 to 100 points. </returns>
    private double GetFitness()
    {
        var distance = Vector2.Distance(_car.Position, _checkPoints[_targetCheckPointIndex]);
        var distancePoints = 50 * Math.Sqrt(1 - distance / _fullDistance);
        var timePoints = 50 * (-4 * Math.Pow((float)_currentGene / Genes.Length - 0.5f, 2) + 1);
        return distancePoints + timePoints;
    }
}