using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Windows.Controls;
using System.Windows.Shapes;
using Project_One_Objects.Helpers;

namespace Project_One.Drawing.WpfOnly;

public class WpfCurveEraser : WpfAbstract
{
    private readonly Ellipse _ellipse;
    private int _radius;

    public WpfCurveEraser(Vector2 position = new(), int radius = 10)
    {
        _ellipse = WpfObjects.EraserEllipse();
        Objects.Add(_ellipse);
        Radius = radius;
        Position = position;
    }

    public int Radius
    {
        get => _radius;
        set
        {
            _radius = value;
            _ellipse.Width = _radius * 2;
            _ellipse.Height = _radius * 2;
        }
    }

    public Vector2 Position { get; set; }

    public void MoveToNearestPoint(List<Vector2> points, Vector2 mousePosition)
    {
        if (points.Count == 0) return;

        Position = points.MinBy(p => Vector2.Distance(p, mousePosition));
    }

    public static void EraseNearestPoint(List<Vector2> points, Vector2 mousePosition)
    {
        var distances = points.Select(p => Vector2.Distance(p, mousePosition)).ToList();
        if (distances.Count == 0) return;

        var index = distances.IndexOf(distances.Min());
        points.RemoveAt(index);
    }

    public void ErasePoints(List<Vector2> points, Vector2 mousePosition, float zoom)
    {
        for (var i = points.Count - 1; i >= 0; i--)
        {
            var distance = Vector2.Distance(points[i], mousePosition);
            if (distance < _radius / zoom) points.RemoveAt(i);
        }
    }

    public new void Update(Camera camera)
    {
        var position = camera.ConvertOut(Position);
        Canvas.SetLeft(_ellipse, position.X - _radius);
        Canvas.SetTop(_ellipse, position.Y - _radius);
    }
}