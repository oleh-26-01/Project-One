using System.Numerics;
using Project_One_Objects.AIComponents;
using Project_One_Objects.Helpers;

namespace Project_One_Objects.Environment;

public class Track
{
    private float _width;
    private Curve? _curve;
    private Vector2[] _curvePoints;
    private Vector2[] _shiftedCurvePoints;
    private Vector2[] _points;
    private float _checkpointDistance;
    private int _currentCheckpointIndex = 0;
    private Tuple<Vector2, Vector2>[] _checkpoints = Array.Empty<Tuple<Vector2, Vector2>>();
    private Vector2[] _checkpointCenters = Array.Empty<Vector2>();
    public bool LoadStatus = false;

    public Track(float width = 10, float minCheckpointDistance = 10)
    {
        Width = width;
        MinCheckpointDistance = minCheckpointDistance;
        _curvePoints = Array.Empty<Vector2>();
        _points = Array.Empty<Vector2>();
    }

    public Track(Track track)
    {
        _width = track._width;
        _curvePoints = new Vector2[track._curvePoints.Length];
        track._curvePoints.CopyTo(_curvePoints, 0);
        _shiftedCurvePoints = new Vector2[track._shiftedCurvePoints.Length];
        track._shiftedCurvePoints.CopyTo(_shiftedCurvePoints, 0);
        _points = new Vector2[track._points.Length];
        track._points.CopyTo(_points, 0);
        _checkpoints = new Tuple<Vector2, Vector2>[track._checkpoints.Length];
        track._checkpoints.CopyTo(_checkpoints, 0);
        _checkpointCenters = new Vector2[track._checkpointCenters.Length];
        track._checkpointCenters.CopyTo(_checkpointCenters, 0);
        _checkpointDistance = track._checkpointDistance;
        _currentCheckpointIndex = track._currentCheckpointIndex;
        LoadStatus = track.LoadStatus;
    }

    public float Width
    {
        get => _width;
        set
        {
            if (value < 0) 
                throw new ArgumentOutOfRangeException(nameof(value), "Width cannot be negative.");
            _width = value;
            if (LoadStatus) _points = GetPoints();
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
            if (value < 0 || value >= _checkpoints.Length) 
                throw new ArgumentOutOfRangeException(nameof(value), "Index is out of range.");
            _currentCheckpointIndex = value;
        }
    }

    public Tuple<Vector2, Vector2>[] Checkpoints => _checkpoints;
    public Vector2[] CheckpointCenters => _checkpointCenters;

    public Vector2[] Points => _points;
    public Vector2[] CurvePoints => _curvePoints;

    public Track Load(string path)
    {
        _curve = new Curve().Load(path);
        _curvePoints = _curve.Points.ToArray();
        _shiftedCurvePoints = new Vector2[_curvePoints.Length];
        for (var i = 0; i < _curvePoints.Length; i++)
            _shiftedCurvePoints[i] = _curvePoints[i] - _curvePoints[1];
        _points = GetPoints();
        UpdateCheckpoints();
        CurrentCheckpointIndex = 0;
        LoadStatus = true;
        return this;
    }

    private Vector2[] GetPoints()
    {
        var points = _shiftedCurvePoints;
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

        _curvePoints = newCurvePoints;
        return result;
    }

    public void UpdateCheckpoints()
    {
        var trackLength = 0f;
        var curvePoints = _curvePoints;
        for (var i = 0; i < curvePoints.Length - 1; i++)
        {
            trackLength += Vector2.Distance(curvePoints[i], curvePoints[i + 1]);
        }
        var checkpointCount = (int)(trackLength / _checkpointDistance);
        var checkpoints = new List<Tuple<Vector2, Vector2>>(checkpointCount);
        var checkpointCenters = new List<Vector2>(checkpointCount);

        var distanceRemainder = 0f;

        for (var i = 1; i < curvePoints.Length - 1; i++)
        {
            var segments = Vector2.Distance(curvePoints[i], curvePoints[i + 1]) / _checkpointDistance;
            var leftSegmentLength = Vector2.Distance(_points[i], _points[i + 1]) / segments;
            var rightSegmentLength = Vector2.Distance(_points[^(i + 1)], _points[^(i + 2)]) / segments;
            var leftSegment = Vector2.Normalize(_points[i + 1] - _points[i]) * leftSegmentLength;
            var rightSegment = Vector2.Normalize(_points[^(i + 2)] - _points[^(i + 1)]) * rightSegmentLength;
            for (var j = distanceRemainder; j < segments; j++)
            {
                var left = _points[i] + leftSegment * j;
                var right = _points[^(i + 1)] + rightSegment * j;
                checkpoints.Add(new Tuple<Vector2, Vector2>(left, right));
                checkpointCenters.Add((left + right) / 2);
            }

            if (distanceRemainder < segments)
                distanceRemainder = 1 - (segments - distanceRemainder) % 1;
            else
                distanceRemainder -= segments;
        }

        var lastLeftPoint = _points[_points.Length / 2 - 1];
        var lastRightPoint = _points[^(_points.Length / 2)];
        for (var i = 0; i < Config.StepWidth - 2; i++)
        {
            checkpoints.Add(new Tuple<Vector2, Vector2>(lastLeftPoint, lastRightPoint));
            checkpointCenters.Add((lastLeftPoint + lastRightPoint) / 2);
        }
        _checkpoints = checkpoints.ToArray();
        _checkpointCenters = checkpointCenters.ToArray();
    }

    public bool OnCheckpoint(Vector2 carPosition, float checkDistance = 10)
    {
        var checkpoint = _checkpoints[_currentCheckpointIndex];

        var distanceToCenter = Vector2.Distance(carPosition, _checkpointCenters[_currentCheckpointIndex]);
        if (distanceToCenter > _width) return false;

        var distance = MathExtensions.DistanceToSegment(carPosition, checkpoint.Item1, checkpoint.Item2);
        if (distance < checkDistance)
        {
            _currentCheckpointIndex++;
            if (_currentCheckpointIndex >= _checkpoints.Length) _currentCheckpointIndex = 0;
            return true;
        }

        return false;
    }
}