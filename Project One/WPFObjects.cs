﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Shapes;

namespace Project_One;

public class WpfObjects
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

    /// <summary>Creates new WPF Line object and stylize it as a crosshair.</summary>
    /// <returns>Stylized WPF Line.</returns>
    public static Line CrosshairLine(double x1 = 0, double y1 = 0, double x2 = 0, double y2 = 0)
    {
        var line = Line(x1, y1, x2, y2);
        line.Stroke = System.Windows.Media.Brushes.Red;
        return line;
    }

    /// <summary>Creates new WPF Polyline object and stylize it.</summary>
    /// <returns>Stylized WPF Polyline.</returns>
    public static Polyline Polyline() => new()
    {
        Stroke = System.Windows.Media.Brushes.Black,
        StrokeThickness = 2,
    };

    /// <summary>Create new WPF Ellipse object and stylize it. Hidden by default.</summary>
    /// <returns>Stylized WPF Ellipse.</returns>
    public static Ellipse EraserEllipse() => new()
    {
        Stroke = System.Windows.Media.Brushes.Yellow,
        StrokeThickness = 2,
        Fill = System.Windows.Media.Brushes.Transparent,
        Visibility = System.Windows.Visibility.Hidden
    };
}