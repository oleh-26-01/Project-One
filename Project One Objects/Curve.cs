using System.Numerics;
using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace Project_One_Objects;

public class Curve
{
    private List<Vector2> _points;
    private List<Vector2> _tempPoints;
    private double _optAngle;
    
    /// <param name="optimizationAngle">1 rad</param>
    public Curve(double optimizationAngle = Math.PI / 180 * 5)
    {
        _points = new List<Vector2>();
        _tempPoints = new List<Vector2>();
        OptAngle = optimizationAngle;
    }

    /// <summary>Adds a point to the curve.</summary>
    public void AddPoint(Vector2 point) => _points.Add(point);

    /// <summary>Adds points to the curve.</summary>
    public void AddPoints(List<Vector2> points) => _points.AddRange(points);

    /// <summary>Save copy of temporary points as actual points.</summary>
    public void ApplyChanges() => _points = new List<Vector2>(_tempPoints);

    /// <summary>Load points from file.</summary>
    /// <param name="path">Path to file.</param>
    public void Load(string path)
    {
        string json = File.ReadAllText(path);
        var data = JsonConvert.DeserializeObject<dynamic>(json);
        
        DateTime date = data.Date;
        int pointsCount = data.PointsCount;
        OptAngle = data.Accuracy;
        _points.Clear();
        foreach (var point in data.Points)
        {
            _points.Add(new Vector2((float)point.X, (float)point.Y));
        }
    }

    /// <summary>Save actual points to file.</summary>
    /// <param name="path">Path to file.</param>
    public void Save(string path)
    {
        var json = new
        {
            Date = DateTime.Now,
            PointsCount = _points.Count,
            Accuracy = OptAngle,
            Points = _points
        };
        File.WriteAllText(path, JsonConvert.SerializeObject(json));
    }

    /// <summary>
    /// Updates the temporary points list with OptAngle value.
    /// </summary>
    /// <param name="optAngle">Optimization angle in radians.</param>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public void ApplyAngleToCurve(double optAngle)
    {
        OptAngle = optAngle;

        if (_points.Count < 3) return;

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

    /// <summary>
    /// The minimum difference between the angles
    /// of consecutive vectors formed by three consecutive points.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    /// <remarks>Does NOT update the curve. Value in radians.</remarks>
    public double OptAngle
    {
        get => _optAngle;
        set
        {
            if (value < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(value), "Value must be equal or greater than 0");
            }
            _optAngle = value;
        }
    }

    /// <summary>Actual points of the curve.</summary>
    public List<Vector2> Points
    {
        get => _points;
        set => _points = value;
    }

    /// <summary>Temporary points of the curve.</summary>
    public List<Vector2> TempPoints
    {
        get => _tempPoints;
        set => _tempPoints = value;
    }
}