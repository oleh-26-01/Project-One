using System.Numerics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using Project_One.Drawing.WpfOnly;
using Project_One_Objects.Environment;
using Project_One_Objects.Helpers;

namespace Project_One.Drawing.Wrappers;

public class MaterialQuadWPF : MaterialQuad
{
    private readonly Polygon _polygon;

    public MaterialQuadWPF(float width, float height, Vector2 position, double angle, Brush colorBrush) : base(width,
        height, position, angle)
    {
        _polygon = WpfObjects.Polygon(colorBrush, true, false);
    }

    public void DrawOn(Canvas canvas)
    {
        if (!canvas.Children.Contains(_polygon)) canvas.Children.Add(_polygon);
    }

    public void RemoveFrom(Canvas canvas)
    {
        canvas.Children.Remove(_polygon);
    }

    public void SetVisibility(bool visible)
    {
        _polygon.Visibility = visible ? Visibility.Visible : Visibility.Hidden;
    }

    public void Update(Camera camera, Vector2 position, double angle = 0)
    {
        var points = camera.ConvertOut(GetAbsoluteVertices(position, angle));

        var pointsCollection = _polygon.Points;
        pointsCollection.Clear();
        foreach (var point in points) pointsCollection.Add(new Point(point.X, point.Y));
    }
}