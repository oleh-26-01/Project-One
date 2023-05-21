using System;
using System.Diagnostics;
using System.Numerics;
using System.Reactive.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Project_One.Drawing;
using Project_One.Helpers;
using Project_One_Objects.Helpers;

namespace Project_One;

public partial class SecondCanvas : UserControl
{
    private Label _carDirectionLabel;
    private Thread _carUpdateThread;
    private Label _collisionLabel;
    private Label _cpsLabel;
    private readonly FpsMeter _fpsMeter = new();
    private bool _isCarUpdateThreadRunning;
    private readonly Stopwatch _labelTimer = new();
    private int _tickRate = 60;

    private IDisposable? _updateSubscription;
    public float CameraZoom = 12;

    public Vector2 MousePosition = new(-1, -1);
    public Vector2 MousePositionWorld = new(-1, -1);

    public SecondCanvas()
    {
        InitializeComponent();
        Camera = new Camera(new Vector2((float)ActualWidth, (float)ActualHeight), new Vector2(0, 0), CameraZoom);
        Track = new WpfTrack();
        Car = new WpfCar(new Vector2(0, 0), 0);
    }

    /// <summary>The car drawn on the canvas.</summary>
    public WpfCar Car { get; }

    /// <summary>The track drawn on the canvas.</summary>
    public WpfTrack Track { get; }

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
            LastUpdate.Restart();
        }
    }

    /// <summary>The time since the last update.</summary>
    public Stopwatch LastUpdate { get; } = new();

    public void Init(Label carDirectionLabel, Label collisionLabel, Label cpsLabel)
    {
        _carDirectionLabel = carDirectionLabel;
        _collisionLabel = collisionLabel;
        _cpsLabel = cpsLabel;
        Track.DrawOn(CanvasControl);
        Car.DrawOn(CanvasControl);
        Car.IsVisionActive = true;
        _labelTimer.Start();
    }

    /// <summary>Starts the canvas updater.</summary>
    public void StartUpdates()
    {
        _updateSubscription = Observable.Interval(TimeSpan.FromMilliseconds(TargetRefreshTime))
            .Subscribe(l =>
            {
                Dispatcher.Invoke(() =>
                {
                    if (Track.LoadStatus)
                    {
                        Track.Update(Camera);
                        if (Track.OnCheckpoint(Car.Position, Car.Width))
                        {
                            //Console.WriteLine("Checkpoint");
                        }
                    }

                    //_car.UpdateVision();
                    Car.Update(Camera);
                    MoveCar();
                    MoveCamera();
                    LastUpdate.Restart();
                    if (_labelTimer.ElapsedMilliseconds > 100)
                    {
                        _labelTimer.Restart();
                        _cpsLabel.Content = $"{(int)_fpsMeter.GetAverageFps() / 10}k cps";
                        //_cpsLabel.Content = $"Car. pos. x: {_car.Position.X}, y: {_car.Position.Y}";
                        //var checkpoints = _track.GetCheckpoints().ToArray();
                        //if (checkpoints.Length == 0)
                        //{
                        //    _cpsLabel.Content = "Distance: 0";
                        //    return;
                        //}
                        //var distance = Vector2.Distance(_car.Position, checkpoints[_track.CurrentCheckpointIndex]);
                        //for (var i = _track.CurrentCheckpointIndex; i < _track.CheckpointsIndexes.Count - 1; i++)
                        //{
                        //    distance += Vector2.Distance(checkpoints[i], checkpoints[i + 1]);
                        //}
                        //_cpsLabel.Content = $"Distance: {distance}";

                        var isLookingForward = Car.IsLookingForward();
                        _carDirectionLabel.Content = isLookingForward ? Strings.Forward : Strings.Backward;
                        _carDirectionLabel.Foreground = isLookingForward ? Brushes.Blue : Brushes.Red;

                        var isCollision = Car.IsCollision();
                        _collisionLabel.Content = isCollision ? Strings.Collision : Strings.NoCollision;
                        _collisionLabel.Foreground = isCollision ? Brushes.Red : Brushes.Green;

                        if (isCollision)
                        {
                            //Console.WriteLine("Collision");
                        }
                    }
                });
            });

        // do update as fast as possible
        _isCarUpdateThreadRunning = true;
        _carUpdateThread = new Thread(() =>
        {
            while (_isCarUpdateThreadRunning)
                Dispatcher.Invoke(() =>
                {
                    for (var i = 0; i < 100; i++)
                        Car.UpdateVision();
                    _fpsMeter.Tick();
                });
        });
        _carUpdateThread.Start();
    }

    /// <summary>Stops the canvas updater.</summary>
    public void StopUpdates()
    {
        _updateSubscription?.Dispose();
        _isCarUpdateThreadRunning = false;
    }

    public void MoveCamera()
    {
        //Console.WriteLine($"camera center abs position: {_camera.Position + _camera.Center }, car position: {_car.Position}");
        if (Camera.IsFollowing)
        {
            Camera.FollowUpdate(LastUpdate.ElapsedMilliseconds / 1000f);
        }
        else
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
    }

    public void MoveCar()
    {
        var dt = LastUpdate.ElapsedMilliseconds / 1000f;
        Car.Move(dt);

        if (Keyboard.IsKeyDown(Key.A))
            Car.TurnLeft(dt);
        else if (Keyboard.IsKeyDown(Key.D))
            Car.TurnRight(dt);
        else
            Car.StopTurning(dt);

        if (Keyboard.IsKeyDown(Key.W))
            Car.SpeedUp(dt);
        else if (Keyboard.IsKeyDown(Key.S))
            Car.SpeedDown(dt);
        else
            Car.Stop(dt);
    }

    public void Canvas_OnMouseClick(object sender, MouseButtonEventArgs e)
    {
        Mouse.Capture(e.ButtonState == MouseButtonState.Pressed ? CanvasControl : null);

        if (e is { ChangedButton: MouseButton.Left, LeftButton: MouseButtonState.Released })
        {
        }
    }

    [DllImport("user32.dll")]
    private static extern bool SetCursorPos(int a, int b);

    public void Canvas_OnMouseMove(object sender, MouseEventArgs e)
    {
        var position = e.GetPosition(CanvasControl);
        var newMousePosition = new Vector2((float)position.X, (float)position.Y);
        var mousePositionWorld = Camera.ConvertIn(newMousePosition);
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
                    mousePositionWorld = Camera.ConvertIn(newMousePosition);
                }
                else
                {
                    if (!Camera.IsFollowing) Camera.Move((MousePosition - newMousePosition) / Camera.Zoom);
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