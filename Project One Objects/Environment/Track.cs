﻿using System.Numerics;
using Project_One_Objects.AIComponents;
using Project_One_Objects.Helpers;

namespace Project_One_Objects.Environment;

public class Track
{
    private float _width;
    private Curve? _curve;
    private Vector2[] _curvePoints;
    private Vector2[] _points;
    private float _minCheckpointDistance;
    private List<int> _checkpointIndexes;
    private int _currentCheckpointIndex = 0;
    public bool LoadStatus = false;

    public Track(float width = 10, float minCheckpointDistance = 10)
    {
        Width = width;
        MinCheckpointDistance = minCheckpointDistance;
        _curvePoints = Array.Empty<Vector2>();
        _points = Array.Empty<Vector2>();
        _checkpointIndexes = new List<int>();
    }

    public Track(Track track)
    {
        _width = track._width;
        _curvePoints = new Vector2[track._curvePoints.Length];
        track._curvePoints.CopyTo(_curvePoints, 0);
        _points = new Vector2[track._points.Length];
        track._points.CopyTo(_points, 0);
        _minCheckpointDistance = track._minCheckpointDistance;
        _checkpointIndexes = track._checkpointIndexes;
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
            if (LoadStatus) _points = GetPoints();
        }
    }

    public float MinCheckpointDistance
    {
        get => _minCheckpointDistance;
        set
        {
            if (value < 0) throw new ArgumentOutOfRangeException(nameof(value), "MinCheckpointDistance cannot be negative.");
            _minCheckpointDistance = value;
        }
    }

    public List<int> CheckpointsIndexes => _checkpointIndexes;

    public int CurrentCheckpointIndex
    {
        get => _currentCheckpointIndex;
        set
        {
            if (value < 0 || value >= _checkpointIndexes.Count) throw new ArgumentOutOfRangeException(nameof(value), "Index is out of range.");
            _currentCheckpointIndex = value;
        }
    }

    public Vector2 CurrentCheckpointCenter { get; set; } = Vector2.Zero;

    public Vector2[] Points => _points;
    public Vector2[] CurvePoints => _curvePoints;

    public Track Load(string path)
    {
        _curve = new Curve().Load(path);
        _curvePoints = _curve.Points.ToArray();
        _points = GetPoints();
        _checkpointIndexes = GetCheckpointIndexes();
        CurrentCheckpointIndex = 0;
        LoadStatus = true;
        return this;
    }

    private Vector2[] GetPoints()
    {
        var points = _curve!.Points.ToArray().Select(p => p - _curve.Points[1]).ToArray();
        var newCurvePoints = new Vector2[points.Length - 1];
        //var points = _curvePoints;
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

    private List<int> GetCheckpointIndexes()
    {
        var points = _points;
        var checkpoints = new List<int>();
        var lastPointIndex = _curvePoints.Length - 1;
        var distance = 0f;
        for (var i = 0; i < lastPointIndex - 1; i++)
        {
            var point1 = (points[i] + points[^(i + 1)]) / 2;
            var point2 = (points[i + 1] + points[^(i + 2)]) / 2;
            distance += Vector2.Distance(point1, point2);
            if (distance < _minCheckpointDistance) continue;
            checkpoints.Add(i + 1);
            distance = 0;
        }

        for (var i = 0; i < Config.StepWidth; i++)
        {
            checkpoints.Add(checkpoints[^1]);
        }

        return checkpoints;
    }

    public List<Vector2> GetCheckpoints()
    {
        var checkpoints = new List<Vector2>();
        foreach (var index in _checkpointIndexes)
        {
            var firstPoint = _points[index];
            var secondPoint = _points[^(index + 1)];
            checkpoints.Add((firstPoint + secondPoint) / 2);
        }

        return checkpoints;
    }

    public bool OnCheckpoint(Vector2 carPosition, float checkDistance = 10)
    {
        var checkpointIndex = _checkpointIndexes[_currentCheckpointIndex];
        var firstPoint = _points[checkpointIndex];
        var secondPoint = _points[^(checkpointIndex + 1)];

        var center = (firstPoint + secondPoint) / 2;
        var distanceToCenter = Vector2.Distance(carPosition, center);
        if (distanceToCenter > _width) return false;

        var distance = MathExtensions.DistanceToSegment(carPosition, firstPoint, secondPoint);
        if (distance < checkDistance)
        {
            _currentCheckpointIndex++;
            if (_currentCheckpointIndex >= _checkpointIndexes.Count) _currentCheckpointIndex = 0;
            CurrentCheckpointCenter = center;
            return true;
        }

        return false;
    }
}