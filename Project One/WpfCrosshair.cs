using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Shapes;
using Project_One_Objects;

namespace Project_One;

public class WpfCrosshair
{
    private readonly Line _line1;
    private readonly Line _line2;
    private int _size;
    private Vector2 _position;
    private Vector2[] _relativePositions;

    public WpfCrosshair(Vector2 position, int size = 10)
    {
        _line1 = WpfObjects.CrosshairLine();
        _line2 = WpfObjects.CrosshairLine();
        _relativePositions = new Vector2[4];
        Size = size;
        _position = position;
    }

    public void DrawOn(Canvas canvas)
    {
        if (!canvas.Children.Contains(_line1))
        {
            canvas.Children.Add(_line1);
        }
        if (!canvas.Children.Contains(_line2))
        {
            canvas.Children.Add(_line2);
        }
    }

    public void RemoveFrom(Canvas canvas)
    {
        canvas.Children.Remove(_line1);
        canvas.Children.Remove(_line2);
    }

    public void SetVisibility(bool visible)
    {
        _line1.Visibility = visible ? System.Windows.Visibility.Visible : System.Windows.Visibility.Hidden;
        _line2.Visibility = visible ? System.Windows.Visibility.Visible : System.Windows.Visibility.Hidden;
    }

    public void Update(Camera camera)
    {
        var position = camera.ConvertOut(_position);
        var points = _relativePositions.Select(p => p + position).ToArray();
        _line1.X1 = points[0].X;
        _line1.Y1 = points[0].Y;
        _line1.X2 = points[1].X;
        _line1.Y2 = points[1].Y;
        _line2.X1 = points[2].X;
        _line2.Y1 = points[2].Y;
        _line2.X2 = points[3].X;
        _line2.Y2 = points[3].Y;
    }

    public int Size
    {
        get => _size;
        set
        {
            if (value < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(value), "Size must be greater than 0.");
            }
            _size = value;
            _relativePositions = new Vector2[]
            {
                new Vector2(-value, 0),
                new Vector2(value, 0),
                new Vector2(0, -value),
                new Vector2(0, value)
            };
        }
    }
}