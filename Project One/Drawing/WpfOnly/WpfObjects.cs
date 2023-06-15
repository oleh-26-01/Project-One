using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Project_One.Drawing.WpfOnly;

/// <summary> Creates stylized WPF objects. </summary>
public class WpfObjects
{
    public static Line Line(double x1 = 0, double y1 = 0, double x2 = 0, double y2 = 0)
    {
        return new Line
        {
            X1 = x1,
            Y1 = y1,
            X2 = x2,
            Y2 = y2,
            Stroke = Brushes.Black,
            StrokeThickness = 2
        };
    }

    public static Line CrosshairLine(double x1 = 0, double y1 = 0, double x2 = 0, double y2 = 0)
    {
        var line = Line(x1, y1, x2, y2);
        line.Stroke = Brushes.Red;
        return line;
    }

    public static Line CarVector(double x1 = 0, double y1 = 0, double x2 = 0, double y2 = 0)
    {
        var line = Line(x1, y1, x2, y2);
        line.Stroke = Brushes.Red;
        return line;
    }

    public static Polyline Polyline()
    {
        return new Polyline
        {
            Stroke = Brushes.Black,
            StrokeThickness = 2
        };
    }

    public static Polygon Polygon(Brush colorBrush, bool isFilled = true, bool isStroked = true)
    {
        return new Polygon
        {
            Fill = isFilled ? colorBrush : Brushes.Transparent,
            Stroke = isStroked ? Brushes.Black : Brushes.Transparent,
            StrokeThickness = 1
        };
    }

    public static Ellipse EraserEllipse()
    {
        return new Ellipse
        {
            Stroke = Brushes.Yellow,
            StrokeThickness = 2,
            Fill = Brushes.Transparent,
            Visibility = Visibility.Hidden
        };
    }

    public static Line CheckpointLine(double x1 = 0, double y1 = 0, double x2 = 0, double y2 = 0)
    {
        var line = Line(x1, y1, x2, y2);
        line.Stroke = Brushes.Yellow;
        return line;
    }

    public static TextBlock TextBlock(string text = "", double x = 0, double y = 0, float fontSize = 12)
    {
        return new TextBlock
        {
            Text = text,
            Foreground = Brushes.Black,
            FontSize = fontSize,
            Margin = new Thickness(x, y, 0, 0)
        };
    }
}