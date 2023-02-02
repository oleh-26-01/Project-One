using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Shapes;

namespace Project_One;

internal class WPFObjects
{
    /// <summary>Creates new WPF Line object and stylize it.</summary>
    /// <returns>Stylized WPF Line.</returns>
    public static Line Line(double x1 = 0, double y1 = 0, double x2 = 0, double y2 = 0)
    {
        return new Line
        {
            X1 = x1,
            Y1 = y1,
            X2 = x2,
            Y2 = y2,
            Stroke = System.Windows.Media.Brushes.Black,
            StrokeThickness = 2
        };
    }

    public static Polyline Polyline() => new()
    {
        Stroke = System.Windows.Media.Brushes.Black,
        StrokeThickness = 2,
    };
}