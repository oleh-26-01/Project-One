using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Reactive.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Project_One_Objects;

namespace Project_One;

public partial class ThirdCanvas : UserControl
{
    
    private readonly Camera _camera;

    public Vector2 MousePosition = new(-1, -1);
    public Vector2 MousePositionWorld = new(-1, -1);
    public float CameraZoom = 12;

    private IDisposable? _updateSubscription;
    
    private int _tickRate = 60;
    private Stopwatch _lastUpdate = new();
    private FpsMeter _fpsMeter = new();

    public ThirdCanvas()
    {
        InitializeComponent();
        _camera = new Camera(new Vector2((float)ActualWidth, (float)ActualHeight), new Vector2(0, 0), CameraZoom);
    }

    /// <summary>The camera used to draw all objects on the canvas.</summary>
    public Camera Camera => _camera;

    /// <summary>The time in milliseconds between each update.</summary>
    public double TargetRefreshTime => 1000d / _tickRate;

    public void Init()
    {

    }

    public void StartUpdates()
    {
        _updateSubscription = Observable.Interval(TimeSpan.FromMilliseconds(TargetRefreshTime))
            .Subscribe((l) =>
            {
                Dispatcher.Invoke(() =>
                {
                });
            });

    }

    public void StopUpdates()
    {
        _updateSubscription?.Dispose();
        //_isCarUpdateThreadRunning = false;
    }


    public void Canvas_OnMouseClick(object sender, MouseButtonEventArgs e)
    {
        Mouse.Capture(e.ButtonState == MouseButtonState.Pressed ? CanvasControl : null);

        if (e is { ChangedButton: MouseButton.Left, LeftButton: MouseButtonState.Released })
        {
        }
    }

    public void Canvas_OnMouseMove(object sender, MouseEventArgs e)
    {
        var position = e.GetPosition(CanvasControl);
        var newMousePosition = new Vector2((float)position.X, (float)position.Y);
        var mousePositionWorld = _camera.ConvertIn(newMousePosition);
        var newCenter = newMousePosition;

        if (e.LeftButton == MouseButtonState.Pressed)
        {
            
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
                    if (!_camera.IsFollowing)
                    {
                        _camera.Move((MousePosition - newMousePosition) / _camera.Zoom);
                    }
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