using System;
using System.Diagnostics;
using System.Numerics;
using System.Reactive.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Project_One.Helpers;
using Project_One_Objects;

namespace Project_One;

public partial class FirstCanvas : UserControl
{
    private readonly Camera _camera;
    private readonly WpfCurve _curve;
    private readonly WpfCurveEraser _curveEraser;
    private readonly WpfCrosshair _crosshair;

    public Vector2 MousePosition = new(-1, -1);
    public Vector2 MousePositionWorld = new(-1, -1);

    private IDisposable? _updateSubscription;
    private IDisposable? _keyEventSubscription;
    private int _tickRate = 60;
    private Stopwatch _lastUpdate = new();

    private FirstTopPanel _firstTopPanel;

    public FirstCanvas()
    {
        InitializeComponent();
        _camera = new Camera(new Vector2((float)ActualWidth, (float)ActualHeight), new Vector2(0, 0));
        _curve = new WpfCurve();
        _curveEraser = new WpfCurveEraser();
        _crosshair = new WpfCrosshair(new Vector2(0, 0));
    }

    /// <summary>The curve drawn on the canvas.</summary>
    public WpfCurve Curve => _curve;

    /// <summary>The curve eraser drawn on the canvas.</summary>
    public WpfCurveEraser CurveEraser => _curveEraser;

    /// <summary>The camera used to draw all objects on the canvas.</summary>
    public Camera Camera => _camera;
    
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
    public Stopwatch LastUpdate => _lastUpdate;

    public void Init(FirstTopPanel firstTopPanel)
    {
        _firstTopPanel = firstTopPanel;

        _curve.DrawOn(CanvasControl);
        _curveEraser.DrawOn(CanvasControl);
        _crosshair.DrawOn(CanvasControl);
    }

    /// <summary>Starts the canvas updater.</summary>
    public void StartUpdates()
    {
        _updateSubscription = Observable.Interval(TimeSpan.FromMilliseconds(TargetRefreshTime))
            .Subscribe((l) =>
            {
                Dispatcher.Invoke(() =>
                {
                    _curve.Update(_camera);
                    _crosshair.Update(_camera);
                    _curveEraser.Update(_camera);
                    UpdateCamera();
                    _lastUpdate.Restart();
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
            _camera.MoveSync(
                Vector2.Normalize(cameraMoveDirection), 
                _lastUpdate.ElapsedMilliseconds, 
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
            _curveEraser.EraseNearestPoint(_curve.Points, MousePositionWorld);
            _curveEraser.MoveToNearestPoint(_curve.Points, MousePositionWorld);
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
        var mousePositionWorld = _camera.ConvertIn(newMousePosition);
        
        _curveEraser.MoveToNearestPoint(_curve.Points, mousePositionWorld);

        if (e.LeftButton == MouseButtonState.Pressed)
        {
            switch (_firstTopPanel.OnCurveAction)
            {
                case Strings.DrawAction:
                    _curve.AddPoint(mousePositionWorld);
                    break;
                case Strings.EraseAction:
                    _curveEraser.ErasePoints(_curve.Points, mousePositionWorld, _camera.Zoom);
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
                    mousePositionWorld = _camera.ConvertIn(newMousePosition);
                }
                else
                {
                    _camera.Move((MousePosition - newMousePosition) / _camera.Zoom);
                }
            }
        }

        MousePosition = newMousePosition;
        MousePositionWorld = mousePositionWorld;
    }

    public void Canvas_OnMouseWheel(object sender, MouseWheelEventArgs e)
    {
        var mousePosition = e.GetPosition(CanvasControl);
        if (mousePosition.X < 0 || mousePosition.Y < 0 || mousePosition.X > ActualWidth || mousePosition.Y > ActualHeight) return;

        if (e.Delta > 0)
        {
            _camera.ZoomIn(e.Delta / 1000f);
        }
        else
        {
            _camera.ZoomOut(-e.Delta / 1000f);
        }
    }

    private void Canvas_OnSizeChanged(object sender, SizeChangedEventArgs e)
    {
        var canvasSize = new Vector2((float)e.NewSize.Width, (float)e.NewSize.Height);
        var newCenter = canvasSize / 2;

        _camera.Move(_camera.Center - newCenter);
        _camera.Center = newCenter;
    }
}