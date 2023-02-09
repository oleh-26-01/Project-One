using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Shapes;
using Project_One_Objects;

namespace Project_One;

public class WpfCurveEraser
{
    private readonly Ellipse _ellipse;
    private int _radius;

    public WpfCurveEraser(Vector2 position = new(), int radius = 10)
    {
        _ellipse = WpfObjects.EraserEllipse();
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

    public void EraseNearestPoint(List<Vector2> points, Vector2 mousePosition)
    {
        var distances = points.Select(p => Vector2.Distance(p, mousePosition)).ToList();
        var index = distances.IndexOf(distances.Min());
        if (index == -1) return;
        points.RemoveAt(index);
    }

    public void ErasePoints(List<Vector2> points, Vector2 mousePosition, float Zoom)
    {
        for (var i = points.Count - 1; i >= 0; i--)
        {
            var distance = Vector2.Distance(points[i], mousePosition);
            if (distance < _radius / Zoom) points.RemoveAt(i);
        }
    }

    public void DrawOn(Canvas canvas)
    {
        if (!canvas.Children.Contains(_ellipse)) canvas.Children.Add(_ellipse);
    }

    public void RemoveFrom(Canvas canvas)
    {
        canvas.Children.Remove(_ellipse);
    }

    public void SetVisibility(bool visible)
    {
        _ellipse.Visibility = visible ? Visibility.Visible : Visibility.Hidden;
    }

    public void Update(Camera camera)
    {
        var position = camera.ConvertOut(Position);
        Canvas.SetLeft(_ellipse, position.X - _radius);
        Canvas.SetTop(_ellipse, position.Y - _radius);
    }
}