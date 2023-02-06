using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using Project_One_Objects;

namespace Project_One;

public class WpfCurve : Project_One_Objects.Curve
{
    private readonly Polyline _polyLine;

    public WpfCurve(string filePath = "")
    {
        _polyLine = WpfObjects.Polyline();
        if (filePath != "")
        {
            Load(filePath);
        }
    }

    public void DrawOn(Canvas canvas)
    {
        if (!canvas.Children.Contains(_polyLine))
        {
            canvas.Children.Add(_polyLine);
        }
    }

    public void RemoveFrom(Canvas canvas)
    {
        canvas.Children.Remove(_polyLine);
    }

    public void SetVisibility(bool visible)
    {
        _polyLine.Visibility = visible ? System.Windows.Visibility.Visible : System.Windows.Visibility.Hidden;
    }

    public void Update(Camera camera)
    {
        var points = camera.ConvertOut(Points);

        var pointsCollection = _polyLine.Points;
        pointsCollection.Clear();
        foreach (var point in points)
        {
            pointsCollection.Add(new Point(point.X, point.Y));
        }
    }
}