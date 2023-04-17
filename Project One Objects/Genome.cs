using System.Numerics;

namespace Project_One_Objects;

public class Genome
{
    private readonly Car _car;
    private readonly Track _track;
    private readonly Vector2[] _checkPoints;
    private int _targetCheckPointIndex;
    public int[] Genes { get; set; }
    public float Fitness;
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

    private float GetFitness()
    {
        var distance = Vector2.Distance(_car.Position, _checkPoints[_targetCheckPointIndex]);
        var distancePoints = (1 - distance / _fullDistance) * 100;
        var timePoints = (1 - (float) _currentGene / Genes.Length) * 100;
        return distancePoints + timePoints;
    }
}