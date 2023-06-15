using System.Numerics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using Project_One.Drawing.WpfOnly;
using Project_One_Objects.Environment;
using Project_One_Objects.Helpers;

namespace Project_One.Drawing.Wrappers;

public class CarWPF : Car
{
    private readonly MaterialQuadWPF _body;
    private readonly Line[] _carVectors;
    private readonly MaterialQuadWPF _wheelBackLeft;
    private readonly MaterialQuadWPF _wheelBackRight;
    private readonly MaterialQuadWPF _wheelFrontLeft;
    private readonly MaterialQuadWPF _wheelFrontRight;
    private bool _vectorsVisibility = true;

    public CarWPF(Vector2 startPosition, double bodyAngle) : base(startPosition, bodyAngle)
    {
        _body = new MaterialQuadWPF(
            Width, Height,
            startPosition, bodyAngle,
            Brushes.LimeGreen);

        _wheelFrontLeft = new MaterialQuadWPF(
            Width * 0.1f, Height * 0.15f,
            startPosition + new Vector2(1.5f, -0.9f), bodyAngle,
            Brushes.Red);

        _wheelFrontRight = new MaterialQuadWPF(
            Width * 0.1f, Height * 0.15f,
            startPosition + new Vector2(1.5f, 0.9f), bodyAngle,
            Brushes.Red);

        _wheelBackLeft = new MaterialQuadWPF(
            Width * 0.1f, Height * 0.15f,
            startPosition + new Vector2(-1.5f, -0.9f), bodyAngle,
            Brushes.Blue);

        _wheelBackRight = new MaterialQuadWPF(
            Width * 0.1f, Height * 0.15f,
            startPosition + new Vector2(-1.5f, 0.9f), bodyAngle,
            Brushes.Blue);

        _carVectors = new Line[VisionCount];
        for (var i = 0; i < VisionCount; i++)
            _carVectors[i] = WpfObjects.CarVector();
    }

    public void DrawOn(Canvas canvas)
    {
        for (var i = 0; i < VisionCount; i++)
            canvas.Children.Add(_carVectors[i]);

        _body.DrawOn(canvas);
        _wheelFrontLeft.DrawOn(canvas);
        _wheelFrontRight.DrawOn(canvas);
        _wheelBackLeft.DrawOn(canvas);
        _wheelBackRight.DrawOn(canvas);
    }

    public void RemoveFrom(Canvas canvas)
    {
        _body.RemoveFrom(canvas);
        _wheelFrontLeft.RemoveFrom(canvas);
        _wheelFrontRight.RemoveFrom(canvas);
        _wheelBackLeft.RemoveFrom(canvas);
        _wheelBackRight.RemoveFrom(canvas);

        for (var i = 0; i < VisionCount; i++)
            canvas.Children.Remove(_carVectors[i]);
    }

    public void SetVisibility(bool visible)
    {
        _body.SetVisibility(visible);
        _wheelFrontLeft.SetVisibility(visible);
        _wheelFrontRight.SetVisibility(visible);
        _wheelBackLeft.SetVisibility(visible);
        _wheelBackRight.SetVisibility(visible);

        for (var i = 0; i < VisionCount; i++)
            _carVectors[i].Visibility = visible ? Visibility.Visible : Visibility.Hidden;
    }

    public void SetAngle(double angle)
    {
        _body.Angle = angle;
        _wheelFrontLeft.Angle = angle;
        _wheelFrontRight.Angle = angle;
        _wheelBackLeft.Angle = angle;
        _wheelBackRight.Angle = angle;
    }

    public void Update(Camera camera)
    {
        if (_vectorsVisibility != IsVisionActive)
        {
            _vectorsVisibility = IsVisionActive;
            for (var i = 0; i < VisionCount; i++)
                _carVectors[i].Visibility = IsVisionActive ? Visibility.Visible : Visibility.Hidden;
        }

        _body.Angle = BodyAngle;
        _body.Update(camera, Position);

        var absoluteFrontWheelsAngle = (BodyAngle + FrontWheelsAngle).Mod(MathExtensions.TwoPi);

        _wheelFrontLeft.Angle = absoluteFrontWheelsAngle;
        _wheelFrontLeft.Update(camera, Position, BodyAngle);

        _wheelFrontRight.Angle = absoluteFrontWheelsAngle;
        _wheelFrontRight.Update(camera, Position, BodyAngle);

        _wheelBackLeft.Angle = BodyAngle;
        _wheelBackLeft.Update(camera, Position, BodyAngle);

        _wheelBackRight.Angle = BodyAngle;
        _wheelBackRight.Update(camera, Position, BodyAngle);

        for (var i = 0; i < VisionCount && IsVisionActive; i++)
            if (VisionPoints[i] == Vector2.Zero)
            {
                _carVectors[i].Visibility = Visibility.Hidden;
            }
            else
            {
                _carVectors[i].Visibility = Visibility.Visible;
                var start = camera.ConvertOut(Position);
                var end = camera.ConvertOut(VisionPoints[i]);
                _carVectors[i].X1 = start.X;
                _carVectors[i].Y1 = start.Y;
                _carVectors[i].X2 = end.X;
                _carVectors[i].Y2 = end.Y;
            }
    }
}