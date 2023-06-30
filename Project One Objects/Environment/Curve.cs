using System.Numerics;
using Newtonsoft.Json;
using Project_One_Objects.Helpers;

namespace Project_One_Objects.Environment;

public class Curve
{
    public static readonly double DefaultOptAngle = Math.PI / 180 * 5;
    private readonly List<Vector2> _tempPoints;
    private bool _isTempPointsSaved = true;
    private double _optAngle;
    private List<Vector2> _points;

    /// <param name="optimizationAngle">1 rad</param>
    public Curve(double optimizationAngle = 0)
    {
        _points = new List<Vector2>();
        _tempPoints = new List<Vector2>();
        OptAngle = optimizationAngle == 0 ? DefaultOptAngle : optimizationAngle;
    }

    /// <summary>
    ///     The minimum difference between the angles
    ///     of consecutive vectors formed by three consecutive points.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    /// <remarks>Does NOT update the curve. Value in radians.</remarks>
    public double OptAngle
    {
        get => _optAngle;
        set
        {
            if (value < 0)
                throw new ArgumentOutOfRangeException(nameof(value), "Value must be equal or greater than 0");

            _optAngle = value;
        }
    }

    /// <summary>Actual points of the curve.</summary>
    public List<Vector2> Points
    {
        get => _isTempPointsSaved ? _points : _tempPoints;
        set => _points = value;
    }

    /// <summary>Adds a point to the curve.</summary>
    public void AddPoint(Vector2 point, float minLength = 0)
    {
        if (_points.Count > 1)
        {
            var angleA = Math.Atan2(_points[^1].Y - _points[^2].Y, _points[^1].X - _points[^2].X);
            var angleB = Math.Atan2(point.Y - _points[^1].Y, point.X - _points[^1].X);
            var length = Vector2.Distance(_points[^1], point);
            if (Math.Abs(angleB - angleA) > OptAngle && length > minLength)
            {
                point.X = _points[^1].X - point.X == 0 ? point.X + MathExtensions.CloseToZero : point.X;
                point.Y = _points[^1].Y - point.Y == 0 ? point.Y + MathExtensions.CloseToZero : point.Y;
                _points.Add(point);
            }
        }
        else
        {
            _points.Add(point);
        }
    }

    /// <summary>Clear lists of temporary and actual points.</summary>
    public void Clear()
    {
        _points.Clear();
        _tempPoints.Clear();
        _isTempPointsSaved = true;
    }

    /// <summary>Save copy of temporary points as actual points.</summary>
    public void ApplyChanges()
    {
        _points = new List<Vector2>(_tempPoints);
        _isTempPointsSaved = true;
    }

    /// <summary>Load points from file.</summary>
    /// <param name="path">Path to file.</param>
    public Curve Load(string path)
    {
        var json = File.ReadAllText(path);
        var data = JsonConvert.DeserializeObject<dynamic>(json);
        _ = data.Date;
        OptAngle = data.Accuracy;
        _points.Clear();
        foreach (var point in data.Points) _points.Add(new Vector2((float)point.X, (float)point.Y));

        return this;
    }

    /// <summary>Save actual points to file.</summary>
    /// <param name="path">Path to file.</param>
    public void Save(string path)
    {
        var json = new
        {
            Date = DateTime.Now,
            Accuracy = OptAngle,
            Points = _points
        };
        File.WriteAllText(path, JsonConvert.SerializeObject(json));
    }

    /// <summary>
    ///     Updates the temporary points list with OptAngle value.
    /// </summary>
    /// <param name="optAngle">Optimization angle in radians.</param>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public void ApplyAngleToCurve(double optAngle)
    {
        OptAngle = optAngle;
        if (_points.Count < 3) return;

        _isTempPointsSaved = false;

        double commonAngle = 0;
        _tempPoints.Clear();
        _tempPoints.Add(_points[0]);

        var angleA = Math.Atan2(_points[1].Y - _points[0].Y, _points[1].X - _points[0].X);
        for (var i = 1; i < _points.Count - 1; i++)
        {
            var angleB = Math.Atan2(_points[i + 1].Y - _points[i].Y, _points[i + 1].X - _points[i].X);
            commonAngle += angleB - angleA;
            if (Math.Abs(commonAngle) > _optAngle)
            {
                _tempPoints.Add(_points[i]);
                commonAngle = 0;
            }

            angleA = angleB;
        }

        _tempPoints.Add(_points[^1]);
    }
}