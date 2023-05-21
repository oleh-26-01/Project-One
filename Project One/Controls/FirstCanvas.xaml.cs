using System;
using System.Diagnostics;
using System.Numerics;
using System.Reactive.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Project_One.Helpers;
using Project_One_Objects.Helpers;

namespace Project_One;

public partial class FirstCanvas : UserControl
{
    private readonly WpfCrosshair _crosshair;

    private FirstTopPanel _firstTopPanel;
    private IDisposable? _keyEventSubscription;
    private int _tickRate = 60;

    private IDisposable? _updateSubscription;

    public Vector2 MousePosition = new(-1, -1);
    public Vector2 MousePositionWorld = new(-1, -1);

    public FirstCanvas()
    {
        InitializeComponent();
        Camera = new Camera(new Vector2((float)ActualWidth, (float)ActualHeight), new Vector2(0, 0));
        Curve = new WpfCurve();
        CurveEraser = new WpfCurveEraser();
        _crosshair = new WpfCrosshair(new Vector2(0, 0));
    }

    /// <summary>The curve drawn on the canvas.</summary>
    public WpfCurve Curve { get; }

    /// <summary>The curve eraser drawn on the canvas.</summary>
    public WpfCurveEraser CurveEraser { get; }

    /// <summary>The camera used to draw all objects on the canvas.</summary>
    public Camera Camera { get; }

    /// <summary>The time in milliseconds between each update.</summary>
    public double TargetRefreshTime => 1000d / _tickRate;

    /// <summary>The number of times per second the canvas will be updated.</summary>
    /// <exception cref="ArgumentOutOfRangeException">TickRate must be greater than 0.</exception>
    /// <remarks>TickRate is not guaranteed to be accurate.</remarks>
    public int TickRate
    {
        get => _tickRate;
        set
        {
            if (value <= 0) throw new ArgumentOutOfRangeException(nameof(value), "Tick rate must be greater than 0.");
            _tickRate = value;
        }
    }

    /// <summary>The time since the last update.</summary>
    public Stopwatch LastUpdate { get; } = new();

    public void Init(FirstTopPanel firstTopPanel)
    {
        _firstTopPanel = firstTopPanel;

        Curve.DrawOn(CanvasControl);
        CurveEraser.DrawOn(CanvasControl);
        _crosshair.DrawOn(CanvasControl);
    }

    /// <summary>Starts the canvas updater.</summary>
    public void StartUpdates()
    {
        _updateSubscription = Observable.Interval(TimeSpan.FromMilliseconds(TargetRefreshTime))
            .Subscribe(l =>
            {
                Dispatcher.Invoke(() =>
                {
                    Curve.Update(Camera);
                    _crosshair.Update(Camera);
                    CurveEraser.Update(Camera);
                    UpdateCamera();
                    LastUpdate.Restart();
                });
            });
    }

    /// <summary>Stops the canvas updater.</summary>
    public void StopUpdates()
    {
        _updateSubscription?.Dispose();
    }

    private void UpdateCamera()
    {
        var cameraMoveDirection = new Vector2(0, 0);
        cameraMoveDirection.X += Keyboard.IsKeyDown(Key.Right) ? 1 : 0;
        cameraMoveDirection.X -= Keyboard.IsKeyDown(Key.Left) ? 1 : 0;
        cameraMoveDirection.Y += Keyboard.IsKeyDown(Key.Down) ? 1 : 0;
        cameraMoveDirection.Y -= Keyboard.IsKeyDown(Key.Up) ? 1 : 0;
        if (cameraMoveDirection.Length() > 0)
            Camera.MoveSync(
                Vector2.Normalize(cameraMoveDirection),
                LastUpdate.ElapsedMilliseconds,
                300);
    }

    public void Canvas_OnMouseClick(object sender, MouseButtonEventArgs e)
    {
        if (_firstTopPanel.OnCurveAction != Strings.EraseAction)
        {
            Mouse.Capture(e.ButtonState == MouseButtonState.Pressed ? CanvasControl : null);
            return;
        }

        if (e is { ChangedButton: MouseButton.Left, LeftButton: MouseButtonState.Released })
        {
            CurveEraser.EraseNearestPoint(Curve.Points, MousePositionWorld);
            CurveEraser.MoveToNearestPoint(Curve.Points, MousePositionWorld);
            _firstTopPanel.Update();
        }
    }

    [DllImport("user32.dll")]
    private static extern bool SetCursorPos(int a, int b);

    public void Canvas_OnMouseMove(object sender, MouseEventArgs e)
    {
        var position = e.GetPosition(CanvasControl);
        //Console.WriteLine(position);

        var newMousePosition = new Vector2((float)position.X, (float)position.Y);
        var mousePositionWorld = Camera.ConvertIn(newMousePosition);

        CurveEraser.MoveToNearestPoint(Curve.Points, mousePositionWorld);

        if (e.LeftButton == MouseButtonState.Pressed)
        {
            switch (_firstTopPanel.OnCurveAction)
            {
                case Strings.DrawAction:
                    Curve.AddPoint(mousePositionWorld);
                    break;
                case Strings.EraseAction:
                    CurveEraser.ErasePoints(Curve.Points, mousePositionWorld, Camera.Zoom);
                    break;
            }

            _firstTopPanel.Update();
        }
        else
        {
            if (e.RightButton == MouseButtonState.Pressed)
            {
                var wrappedMousePosition = MouseExtensions.WrapMouseMove(CanvasControl);
                if (wrappedMousePosition != newMousePosition && wrappedMousePosition != Vector2.Zero)
                {
                    newMousePosition = wrappedMousePosition;
                    mousePositionWorld = Camera.ConvertIn(newMousePosition);
                }
                else
                {
                    Camera.Move((MousePosition - newMousePosition) / Camera.Zoom);
                }
            }
        }

        MousePosition = newMousePosition;
        MousePositionWorld = mousePositionWorld;
    }

    public void Canvas_OnMouseWheel(object sender, MouseWheelEventArgs e)
    {
        var mousePosition = e.GetPosition(CanvasControl);
        if (mousePosition.X < 0 || mousePosition.Y < 0 || mousePosition.X > ActualWidth ||
            mousePosition.Y > ActualHeight) return;

        if (e.Delta > 0)
            Camera.ZoomIn(e.Delta / 1000f);
        else
            Camera.ZoomOut(-e.Delta / 1000f);
    }

    private void Canvas_OnSizeChanged(object sender, SizeChangedEventArgs e)
    {
        var canvasSize = new Vector2((float)e.NewSize.Width, (float)e.NewSize.Height);
        var newCenter = canvasSize / 2;

        Camera.Move(Camera.Center - newCenter);
        Camera.Center = newCenter;
    }
}