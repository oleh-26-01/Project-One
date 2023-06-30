using System.Numerics;
using Project_One_Objects.AIComponents;
using Project_One_Objects.Helpers;

namespace Project_One_Objects.Environment;

public class Track
{
    private readonly Curve _curve = new();
    private float _checkpointDistance;
    private int _currentCheckpointIndex;
    private Vector2[]? _shiftedCurvePoints;
    private float _width;
    public bool LoadStatus;

    public Track(float width = 10, float minCheckpointDistance = 10)
    {
        Width = width;
        MinCheckpointDistance = minCheckpointDistance;
        CurvePoints = Array.Empty<Vector2>();
        Points = Array.Empty<Vector2>();
    }

    public Track(Track track)
    {
        _width = track._width;
        CurvePoints = new Vector2[track.CurvePoints.Length];
        track.CurvePoints.CopyTo(CurvePoints, 0);
        _shiftedCurvePoints = new Vector2[track._shiftedCurvePoints!.Length];
        track._shiftedCurvePoints.CopyTo(_shiftedCurvePoints, 0);
        Points = new Vector2[track.Points.Length];
        track.Points.CopyTo(Points, 0);
        Checkpoints = new Tuple<Vector2, Vector2>[track.Checkpoints.Length];
        track.Checkpoints.CopyTo(Checkpoints, 0);
        CheckpointCenters = new Vector2[track.CheckpointCenters.Length];
        track.CheckpointCenters.CopyTo(CheckpointCenters, 0);
        _checkpointDistance = track._checkpointDistance;
        _currentCheckpointIndex = track._currentCheckpointIndex;
        LoadStatus = track.LoadStatus;
    }

    public float Width
    {
        get => _width;
        set
        {
            if (value < 0) throw new ArgumentOutOfRangeException(nameof(value), "Width cannot be negative.");

            _width = value;
            if (LoadStatus) Points = GetPoints();
        }
    }

    public float MinCheckpointDistance
    {
        get => _checkpointDistance;
        set
        {
            if (value < 0)
                throw new ArgumentOutOfRangeException(nameof(value), "MinCheckpointDistance cannot be negative.");

            _checkpointDistance = value;
        }
    }

    public int CurrentCheckpointIndex
    {
        get => _currentCheckpointIndex;
        set
        {
            if (value < 0 || value >= Checkpoints.Length)
                throw new ArgumentOutOfRangeException(nameof(value), "Index is out of range.");

            _currentCheckpointIndex = value;
        }
    }

    public Tuple<Vector2, Vector2>[] Checkpoints { get; private set; } = Array.Empty<Tuple<Vector2, Vector2>>();
    public Vector2[] CheckpointCenters { get; private set; } = Array.Empty<Vector2>();

    public Vector2[] Points { get; private set; }
    public Vector2[] CurvePoints { get; private set; }

    public void Load(string path)
    {
        _curve.Load(path);
        CurvePoints = _curve.Points.ToArray();
        _shiftedCurvePoints = new Vector2[CurvePoints.Length];
        for (var i = 0; i < CurvePoints.Length; i++) _shiftedCurvePoints[i] = CurvePoints[i] - CurvePoints[1];

        Points = GetPoints();
        UpdateCheckpoints();
        CurrentCheckpointIndex = 0;
        LoadStatus = true;
    }

    private Vector2[] GetPoints()
    {
        var points = _shiftedCurvePoints!;
        var newCurvePoints = new Vector2[points.Length - 1];
        var result = new Vector2[(points.Length - 1) * 2];

        var rotateLeft = Matrix3x2.CreateRotation((float)-90d.ToRad());
        var rotateRight = Matrix3x2.CreateRotation((float)90d.ToRad());
        for (var i = 0; i < points.Length - 1; i++)
        {
            var vector = (points[i + 1] - points[i]) / 2;
            var shift = Vector2.Normalize(vector) * _width;
            result[i] = points[i] + vector + shift.Rotate(rotateLeft);
            result[^(i + 1)] = points[i] + vector + shift.Rotate(rotateRight);
            newCurvePoints[i] = points[i] + vector;
        }

        CurvePoints = newCurvePoints;
        return result;
    }

    public void UpdateCheckpoints()
    {
        var trackLength = 0f;
        var curvePoints = CurvePoints;
        for (var i = 0; i < curvePoints.Length - 1; i++)
            trackLength += Vector2.Distance(curvePoints[i], curvePoints[i + 1]);
        var checkpointCount = (int)(trackLength / _checkpointDistance);
        List<Tuple<Vector2, Vector2>> checkpoints = new(checkpointCount);
        List<Vector2> checkpointCenters = new(checkpointCount);

        var distanceRemainder = 0f;

        for (var i = 1; i < curvePoints.Length - 1; i++)
        {
            var segments = Vector2.Distance(curvePoints[i], curvePoints[i + 1]) / _checkpointDistance;
            var leftSegmentLength = Vector2.Distance(Points[i], Points[i + 1]) / segments;
            var rightSegmentLength = Vector2.Distance(Points[^(i + 1)], Points[^(i + 2)]) / segments;
            var leftSegment = Vector2.Normalize(Points[i + 1] - Points[i]) * leftSegmentLength;
            var rightSegment = Vector2.Normalize(Points[^(i + 2)] - Points[^(i + 1)]) * rightSegmentLength;
            for (var j = distanceRemainder; j < segments; j++)
            {
                var left = Points[i] + leftSegment * j;
                var right = Points[^(i + 1)] + rightSegment * j;
                checkpoints.Add(new Tuple<Vector2, Vector2>(left, right));
                checkpointCenters.Add((left + right) / 2);
            }

            if (distanceRemainder < segments)
                distanceRemainder = 1 - (segments - distanceRemainder) % 1;
            else
                distanceRemainder -= segments;
        }

        var lastLeftPoint = Points[Points.Length / 2 - 1];
        var lastRightPoint = Points[^(Points.Length / 2)];
        for (var i = 0; i < Config.StepWidth - 2; i++)
        {
            checkpoints.Add(new Tuple<Vector2, Vector2>(lastLeftPoint, lastRightPoint));
            checkpointCenters.Add((lastLeftPoint + lastRightPoint) / 2);
        }

        Checkpoints = checkpoints.ToArray();
        CheckpointCenters = checkpointCenters.ToArray();
    }

    public bool OnCheckpoint(Vector2 carPosition, float checkDistance = 10)
    {
        var checkpoint = Checkpoints[_currentCheckpointIndex];

        var distanceToCenter = Vector2.Distance(carPosition, CheckpointCenters[_currentCheckpointIndex]);
        if (distanceToCenter > _width) return false;

        var distance = MathExtensions.DistanceToSegment(carPosition, checkpoint.Item1, checkpoint.Item2);
        if (distance > checkDistance) return false;
        _currentCheckpointIndex++;
        if (_currentCheckpointIndex >= Checkpoints.Length) _currentCheckpointIndex = 0;

        return true;
    }
}