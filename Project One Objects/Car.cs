using System.Diagnostics;
using System.Numerics;

namespace Project_One_Objects;

public class Car
{
    private Vector2 _position;
    private float _speed;
    private double _bodyAngle;
    private double _frontWheelsAngle;

    public Track? _track;
    private Vector2[] _trackSlopeIntercepts;
    private int _visionCount;
    private Vector2[] _visionPoints;
    private float[] _minVisionLengths;
    private List<Vector2>[] _tempPoints;
    private bool _isVisionActive = false;

    private float _slowDownSpeed = 60;
    private float _speedUpSpeed = 30;
    private float _stopSpeed = 10;
    private float _maxSpeed = 40;

    private float _maxFrontWheelsAngle = (float)30d.ToRad();
    private float _rotateSpeed = 3;
    private float _rotateBackSpeed = 2;
    private float _stopRotate = 3;

    // fields to prevent garbage collection
    private float[] _trackPointsAngles;
    private Vector2[] _sortedTrackPointsAngles;
    private float[] _carVisionAngles;
    private Vector2[] _sortedCarVisionAngles;
    private Vector2[] _vectorSlopeIntercepts;
    private float[] _tempVectorAngles;

    public int nearestPointIndex = 1;

    public Car(Vector2 startPosition, double bodyAngle)
    {
        _position = startPosition;
        _bodyAngle = bodyAngle;
        VisionCount = 8;
        _frontWheelsAngle = _bodyAngle;
    }

    /// <summary> Copy constructor </summary>
    /// <param name="car"> Car to copy </param>
    public Car(Car car)
    {
        _position = car._position;
        _speed = car._speed;
        _bodyAngle = car._bodyAngle;
        _frontWheelsAngle = car._frontWheelsAngle;
        _track = car._track;
        _trackSlopeIntercepts = car._trackSlopeIntercepts;
        _visionCount = car._visionCount;
        _visionPoints = car._visionPoints;
        _minVisionLengths = car._minVisionLengths;
        _tempPoints = car._tempPoints;
        _isVisionActive = car._isVisionActive;
        _slowDownSpeed = car._slowDownSpeed;
        _speedUpSpeed = car._speedUpSpeed;
        _stopSpeed = car._stopSpeed;
        _maxSpeed = car._maxSpeed;
        _maxFrontWheelsAngle = car._maxFrontWheelsAngle;
        _rotateSpeed = car._rotateSpeed;
        _rotateBackSpeed = car._rotateBackSpeed;
        _stopRotate = car._stopRotate;
        _trackPointsAngles = car._trackPointsAngles;
        _sortedTrackPointsAngles = car._sortedTrackPointsAngles;
        _carVisionAngles = car._carVisionAngles;
        _sortedCarVisionAngles = car._sortedCarVisionAngles;
        _vectorSlopeIntercepts = car._vectorSlopeIntercepts;
        _tempVectorAngles = car._tempVectorAngles;
        nearestPointIndex = car.nearestPointIndex;
    }

    public Vector2 Position => _position;
    public double BodyAngle => _bodyAngle;
    public float Speed => _speed;
    public double FrontWheelsAngle => _frontWheelsAngle;

    public int Width => 2;
    public int Height => 4;

    /* need to trigger graphic wrapper 
    public int VisionCount
    {
        get => _visionCount;
        set
        {
            if (value < 1)
                throw new ArgumentOutOfRangeException(nameof(value), "Vision count must be greater than 0.");
            _visionCount = (int)value;
            _visionPoints = new Vector2[_visionCount];
        }
    }
    */

    // genome necessary value
    public float MaxSpeed => _maxSpeed;

    public int VisionCount 
    {
        get => _visionCount;
        set
        {
            if (value < 1)
                throw new ArgumentOutOfRangeException(nameof(value), "Vision count must be greater than 0.");
            _visionCount = (int)value;
            _visionPoints = new Vector2[_visionCount];
            _minVisionLengths = GetMinVisionLengths();
            _tempPoints = new List<Vector2>[_visionCount];
            for (var i = 0; i < _visionCount; i++)
                _tempPoints[i] = new List<Vector2>();

            _carVisionAngles = new float[_visionCount];
            _sortedCarVisionAngles = new Vector2[_visionCount];
            _vectorSlopeIntercepts = new Vector2[_visionCount];
            _tempVectorAngles = new float[_visionCount];
        }
    }

    public Vector2[] VisionPoints => _visionPoints;

    public Track Track
    {
        get => _track!;
        set
        {
            _trackPointsAngles = new float[value.Points.Length];
            _sortedTrackPointsAngles = new Vector2[value.Points.Length];
            _trackSlopeIntercepts = new Vector2[value.Points.Length];

            _track = value;
            for (var i = 1; i < _track.Points.Length; i++)
            {
                _trackSlopeIntercepts[i] = MathExtensions.SlopeIntercept(_track.Points[i - 1], _track.Points[i]);
            }
            _trackSlopeIntercepts[0] = MathExtensions.SlopeIntercept(_track.Points[^1], _track.Points[0]);

            _position = (_track.Points[1] + _track.Points[^2]) / 2;
            _bodyAngle = Math.Atan2(_track.Points[2].Y - _track.Points[1].Y, _track.Points[2].X - _track.Points[1].X);
            if (_isVisionActive) UpdateVision();
        }
    }

    public bool IsVisionActive
    {
        get => _isVisionActive;
        set
        {
            if (value == _isVisionActive) return;
            _isVisionActive = value;
            if (_isVisionActive) UpdateVision();
        }
    }

    public bool IsLookingForward()
    {
        if (_track is null) return false;
        var nextPointDistance =
            Vector2.Distance(_position, _track.CurvePoints[(nearestPointIndex + 1).Mod(_track.CurvePoints.Length)]);
        var prevPointDistance = 
            Vector2.Distance(_position, _track.CurvePoints[(nearestPointIndex - 1).Mod(_track.CurvePoints.Length)]);
        var currentPointDistance =
            Vector2.Distance(_position, _track.CurvePoints[nearestPointIndex]);
        if (nextPointDistance < prevPointDistance && nextPointDistance < currentPointDistance)
        {
            nearestPointIndex++;
        }
        else if (prevPointDistance < nextPointDistance && prevPointDistance < currentPointDistance)
        {
            nearestPointIndex--;
        }

        nearestPointIndex = Math.Clamp(nearestPointIndex, 1, _track.CurvePoints.Length - 1);

        var vector = _track.CurvePoints[nearestPointIndex] - 
                     _track.CurvePoints[nearestPointIndex - 1];
        var angle = (Math.Atan2(vector.Y, vector.X)).Mod(MathExtensions.TwoPi);
        return MathExtensions.IsAngleBetween(_bodyAngle.Mod(MathExtensions.TwoPi), 
            (angle - Math.PI / 2.01).Mod(MathExtensions.TwoPi), 
            (angle + Math.PI / 2.01).Mod(MathExtensions.TwoPi));
    }

    public bool IsCollision()
    {
        // check if there is intersection which is smaller than min vision length
        for (var i = 0; i < _visionCount; i++)
        {
            var distance = Vector2.Distance(_position, _visionPoints[i]);
            if (distance > 0 && distance < _minVisionLengths[i])
                return true;
        }

        return false;
    }

    public float[] GetMinVisionLengths()
    {
        var carVertices = new Vector2[4];
        carVertices[0] = new Vector2(Width / 2f, -Height / 2f);
        carVertices[1] = new Vector2(Width / 2f, Height / 2f);
        carVertices[2] = new Vector2(-Width / 2f, Height / 2f);
        carVertices[3] = new Vector2(-Width / 2f, -Height / 2f);

        var nanVector = new Vector2(float.NaN, float.NaN);
        var result = new float[_visionCount];

        var verticesAngles = new float[carVertices.Length];
        var sortedVerticesAngles = new Vector2[carVertices.Length];
        MathExtensions.CalcRelativeAngles(carVertices, _position, _bodyAngle, verticesAngles);
        for (var i = 0; i < sortedVerticesAngles.Length; i++)
        {
            sortedVerticesAngles[i].X = i;
            sortedVerticesAngles[i].Y = verticesAngles[i];
        }
        Array.Sort(sortedVerticesAngles, (a, b) => a.Y.CompareTo(b.Y));

        var carVisionAngles = new float[_visionCount];
        var sortedCarVisionAngles = new Vector2[_visionCount];
        MathExtensions.CalcVectorAngles(_visionCount, 90d.ToRad(), carVisionAngles); // !!! 90d.ToRad()
        for (var i = 0; i < sortedCarVisionAngles.Length; i++)
        {
            sortedCarVisionAngles[i].X = i;
            sortedCarVisionAngles[i].Y = carVisionAngles[i];
        }
        Array.Sort(sortedCarVisionAngles, (a, b) => a.Y.CompareTo(b.Y));

        var c = 0; // car vision index
        var f = 0; // vectors iterated
        for (var v = 0; v < carVertices.Length && f < _visionCount; v++)
        {
            var v1Index = v.Mod(carVertices.Length);
            var v2Index = (v + 1).Mod(carVertices.Length);
            while (!MathExtensions.IsAngleBetween(sortedCarVisionAngles[c].Y, sortedVerticesAngles[v1Index].Y,
                    sortedVerticesAngles[v2Index].Y))
            {
                c = (c + 1).Mod(_visionCount);
                f++;
            }

            while (MathExtensions.IsAngleBetween(sortedCarVisionAngles[c].Y, sortedVerticesAngles[v1Index].Y,
                       sortedVerticesAngles[v2Index].Y))
            {
                var visionPoint = new Vector2(1, 0).Rotate(sortedCarVisionAngles[c].Y);
                var point = MathExtensions.LineIntersection(Vector2.Zero, visionPoint,
                    carVertices[(int)sortedVerticesAngles[v1Index].X],
                    carVertices[(int)sortedVerticesAngles[v2Index].X]);
                if (point != nanVector)
                {
                    result[(int)sortedCarVisionAngles[c].X] = point.Length();
                }

                c = (c + 1).Mod(_visionCount);
            }
        }

        return result;
    }

    public void UpdateVision()
    {
        if (!_isVisionActive || _track is null) return;

        var nanVector = new Vector2(float.NaN, float.NaN);

        var bodyAngle = _bodyAngle.Mod(MathExtensions.TwoPi);

        MathExtensions.CalcRelativeAngles(_track.Points, _position, bodyAngle, _trackPointsAngles);
        for (var i = 0; i < _sortedTrackPointsAngles.Length; i++)
        {
            _sortedTrackPointsAngles[i].X = i;
            _sortedTrackPointsAngles[i].Y = _trackPointsAngles[i];
        }
        Array.Sort(_sortedTrackPointsAngles, (a, b) => a.Y.CompareTo(b.Y));

        MathExtensions.CalcVectorAngles(_visionCount, bodyAngle, _carVisionAngles);
        for (var i = 0; i < _sortedCarVisionAngles.Length; i++)
        {
            _sortedCarVisionAngles[i].X = i;
            _sortedCarVisionAngles[i].Y = _carVisionAngles[i];
        }
        Array.Sort(_sortedCarVisionAngles, (a, b) => a.Y.CompareTo(b.Y));

        for (var i = 0; i < _visionCount; i++)
        {
            _tempVectorAngles[i] = _sortedCarVisionAngles[i].Y;
        }

        MathExtensions.AngleToSlopeIntercept(_tempVectorAngles, _position, _vectorSlopeIntercepts);

        int c = 0; // car vision index
        int t = 0; // track points index
        int f = 0; // vectors iterated

        while (t < _track.Points.Length && f < _visionCount + 1)
        {
            var sortedCvaIndex = (c - 1).Mod(_visionCount);
            if (!MathExtensions.IsAngleBetween(
                    _sortedTrackPointsAngles[t].Y, 
                    _sortedCarVisionAngles[sortedCvaIndex].Y, 
                    _sortedCarVisionAngles[c].Y))
            {
                c = (c + 1) % _visionCount;
                f++;
            }
            else
            {
                var secondPointIndex = (int)(_sortedTrackPointsAngles[t].X + 1) % _track.Points.Length;
                var tempC = c;
                while (MathExtensions.IsAngleBetween(
                           _sortedCarVisionAngles[c].Y, 
                           _sortedTrackPointsAngles[t].Y,
                           _trackPointsAngles[secondPointIndex]))
                {
                    if (_vectorSlopeIntercepts[c].X - _trackSlopeIntercepts[secondPointIndex].X != 0)
                    {
                        var x = (_trackSlopeIntercepts[secondPointIndex].Y - _vectorSlopeIntercepts[c].Y) /
                                (_vectorSlopeIntercepts[c].X - _trackSlopeIntercepts[secondPointIndex].X);
                        var y = _trackSlopeIntercepts[secondPointIndex].X * x + _trackSlopeIntercepts[secondPointIndex].Y;
                        var point = new Vector2(x, y);
                        _tempPoints[(int)_sortedCarVisionAngles[c].X].Add(point);
                    }

                    c = (c + 1) % _visionCount;
                }

                c = tempC;
                t++;
            }
        }

        for (var i = 0; i < _visionCount; i++)
        {
            switch (_tempPoints[i].Count)
            {
                case 1:
                    _visionPoints[i] = _tempPoints[i][0];
                    break;
                case > 1:
                {
                    var min = float.MaxValue;
                    var minIndex = 0;
                    for (var j = 0; j < _tempPoints[i].Count; j++)
                    {
                        var dist = Vector2.Distance(_position, _tempPoints[i][j]);
                        if (!(dist < min)) continue;
                        min = dist;
                        minIndex = j;
                    }

                    _visionPoints[i] = _tempPoints[i][minIndex];
                    break;
                }
                default:
                    _visionPoints[i] = Vector2.Zero;
                    break;
            }
            _tempPoints[i].Clear();
        }
    }

    /// <summary> Move Car according to its speed, body angle, and front wheels angle.</summary>
    /// <param name="dt"> The time in seconds since the last update.</param>
    public void Move(float dt)
    {
        if (_speed == 0) return;

        if (_speed > 0)
        {
            _bodyAngle += _frontWheelsAngle * dt * _speed / 6;
        }
        else
        {
            _bodyAngle += _frontWheelsAngle * dt * _speed / 6;
        }
        _position += new Vector2(1, 0).Rotate((float)_bodyAngle) * _speed * dt;
    }

    /// <summary> Accelerates the car forward. </summary>
    /// <param name="dt"> The time in seconds since the last update.</param>
    public void SpeedUp(float dt)
    {
        if (_speed >= 0)
        {
            _speed += _speedUpSpeed * dt;
        }
        else
        {
            _speed += _slowDownSpeed * dt;
        }

        //_speed = Math.Clamp(_speed, -_maxSpeed, _maxSpeed);
        _speed += ((float)Math.Min(Math.Min(_speed, 
                           _maxSpeed / (Math.Pow(Math.Abs(_frontWheelsAngle).ToDeg(), 0.35) + 0.1)), 
                       _maxSpeed) - _speed) * dt * 2;
    }

    /// <summary> Accelerates the car backwards. </summary>
    /// <param name="dt"> The time in seconds since the last update.</param>
    public void SpeedDown(float dt)
    {
        if (_speed <= 0)
        {
            _speed -= _speedUpSpeed * dt;
        }
        else
        {
            _speed -= _slowDownSpeed * dt;
        }

        _speed += ((float)Math.Max(Math.Max(_speed, 
                           -_maxSpeed / (Math.Pow(Math.Abs(_frontWheelsAngle).ToDeg(), 0.35) + 0.1)),
                       -_maxSpeed) - _speed) * dt * 2;
    }

    /// <summary> Stops the car. </summary>
    /// <param name="dt"> The time in seconds since the last update.</param>
    public void Stop(float dt)
    {
        if (_speed > 0)
        {
            _speed -= _stopSpeed * dt;
            if (_speed < 0) _speed = 0;
        }
        else if (_speed < 0)
        {
            _speed += _stopSpeed * dt;
            if (_speed > 0) _speed = 0;
        }
    }

    /// <summary> Turns the car left. </summary>
    /// <param name="dt"> The time in seconds since the last update.</param>
    public void TurnLeft(float dt)
    {
        _frontWheelsAngle -= (_rotateSpeed - Math.Abs(_speed) / _maxSpeed) * dt;
        _frontWheelsAngle = Math.Max(_frontWheelsAngle, -_maxFrontWheelsAngle);
    }

    /// <summary> Turns the car right. </summary>
    /// <param name="dt"> The time in seconds since the last update.</param>
    public void TurnRight(float dt)
    {
        _frontWheelsAngle += (_rotateSpeed - Math.Abs(_speed) / _maxSpeed) * dt;
        _frontWheelsAngle = Math.Min(_frontWheelsAngle, _maxFrontWheelsAngle);
    }

    /// <summary> Stops the car from turning. </summary>
    /// <param name="dt"> The time in seconds since the last update.</param>
    public void StopTurning(float dt)   
    {
        switch (_frontWheelsAngle)
        {
            case > 0:
                _frontWheelsAngle -= _stopRotate * dt;
                _frontWheelsAngle = Math.Max(_frontWheelsAngle, 0);
                break;
            case < 0:
                _frontWheelsAngle += _stopRotate * dt;
                _frontWheelsAngle = Math.Min(_frontWheelsAngle, 0);
                break;
        }
    }
}