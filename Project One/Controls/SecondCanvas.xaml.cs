using System;
using System.Diagnostics;
using System.Numerics;
using System.Reactive.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Project_One.Drawing;
using Project_One.Helpers;
using Project_One_Objects.Helpers;

namespace Project_One;

public partial class SecondCanvas : UserControl
{
    private readonly Camera _camera;
    private readonly WpfTrack _track;
    private readonly WpfCar _car;

    public Vector2 MousePosition = new(-1, -1);
    public Vector2 MousePositionWorld = new(-1, -1);
    public float CameraZoom = 12;

    private IDisposable? _updateSubscription;
    private Thread _carUpdateThread;
    private bool _isCarUpdateThreadRunning = false;
    private int _tickRate = 60;
    private Stopwatch _lastUpdate = new();
    private FpsMeter _fpsMeter = new();
    private Label _carDirectionLabel;
    private Label _collisionLabel;
    private Label _cpsLabel;
    private Stopwatch _labelTimer = new();

    public SecondCanvas()
    {
        InitializeComponent();
        _camera = new Camera(new Vector2((float)ActualWidth, (float)ActualHeight), new Vector2(0, 0), CameraZoom);
        _track = new WpfTrack();
        _car = new WpfCar(new Vector2(0, 0), 0);
    }

    /// <summary>The car drawn on the canvas.</summary>
    public WpfCar Car => _car;

    /// <summary>The track drawn on the canvas.</summary>
    public WpfTrack Track => _track;

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
            _lastUpdate.Restart();
        }
    }

    /// <summary>The time since the last update.</summary>
    public Stopwatch LastUpdate => _lastUpdate;

    public void Init(Label carDirectionLabel, Label collisionLabel, Label cpsLabel)
    {
        _carDirectionLabel = carDirectionLabel;
        _collisionLabel = collisionLabel;
        _cpsLabel = cpsLabel;
        _track.DrawOn(CanvasControl);
        _car.DrawOn(CanvasControl);
        _car.IsVisionActive = true;
        _labelTimer.Start();
    }

    /// <summary>Starts the canvas updater.</summary>
    public void StartUpdates()
    {
        _updateSubscription = Observable.Interval(TimeSpan.FromMilliseconds(TargetRefreshTime))
            .Subscribe((l) =>
            {
                Dispatcher.Invoke(() =>
                {
                    if (_track.LoadStatus)
                    {
                        _track.Update(_camera);
                        if (_track.OnCheckpoint(_car.Position, _car.Width))
                        {
                            //Console.WriteLine("Checkpoint");
                        }
                    }
                    //_car.UpdateVision();
                    _car.Update(_camera);
                    MoveCar();
                    MoveCamera();
                    _lastUpdate.Restart();
                    if (_labelTimer.ElapsedMilliseconds > 100)
                    {
                        _labelTimer.Restart();
                        //_cpsLabel.Content = $"{(int)_fpsMeter.GetAverageFps() / 10}k cps";
                        //_cpsLabel.Content = $"Car. pos. x: {_car.Position.X}, y: {_car.Position.Y}";
                        var checkpoints = _track.GetCheckpoints().ToArray();
                        if (checkpoints.Length == 0)
                        {
                            _cpsLabel.Content = "Distance: 0";
                            return;
                        }
                        var distance = Vector2.Distance(_car.Position, checkpoints[_track.CurrentCheckpointIndex]);
                        for (var i = _track.CurrentCheckpointIndex; i < _track.CheckpointsIndexes.Count - 1; i++)
                        {
                            distance += Vector2.Distance(checkpoints[i], checkpoints[i + 1]);
                        }
                        _cpsLabel.Content = $"Distance: {distance}";

                        var isLookingForward = _car.IsLookingForward();
                        _carDirectionLabel.Content = isLookingForward ? Strings.Forward : Strings.Backward;
                        _carDirectionLabel.Foreground = isLookingForward ? System.Windows.Media.Brushes.Blue : System.Windows.Media.Brushes.Red;

                        var isCollision = _car.IsCollision();
                        _collisionLabel.Content = isCollision ? Strings.Collision : Strings.NoCollision;
                        _collisionLabel.Foreground = isCollision ? System.Windows.Media.Brushes.Red : System.Windows.Media.Brushes.Green;

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
            {
                Dispatcher.Invoke(() =>
                {
                    for (var i = 0; i < 100; i++)
                        _car.UpdateVision();
                    _fpsMeter.Tick();
                });
            }
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
        if (_camera.IsFollowing)
        {
            _camera.FollowUpdate(_lastUpdate.ElapsedMilliseconds / 1000f);
        }
        else
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
    }

    public void MoveCar()
    {
        var dt = _lastUpdate.ElapsedMilliseconds / 1000f;
        _car.Move(dt);
        
        if (Keyboard.IsKeyDown(Key.A))
        {
            _car.TurnLeft(dt);
        }
        else if (Keyboard.IsKeyDown(Key.D))
        {
            _car.TurnRight(dt);
        }
        else
        {
            _car.StopTurning(dt);
        }        
        
        if (Keyboard.IsKeyDown(Key.W))
        {
            _car.SpeedUp(dt);
        }
        else if (Keyboard.IsKeyDown(Key.S))
        {
            _car.SpeedDown(dt);
        }
        else
        {
            _car.Stop(dt);
        }
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