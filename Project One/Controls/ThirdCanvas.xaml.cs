using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using Project_One.Drawing;
using Project_One_Objects.Environment;
using Project_One_Objects.Helpers;
using Project_One_Objects.AIComponents;

namespace Project_One;

public partial class ThirdCanvas : UserControl
{
    
    private readonly Camera _camera;
    private readonly WpfTrack _track;
    private readonly string _trackPath = "C:\\Coding\\C#\\Project One\\Project One\\Curves\\curve001.crv";
    private WpfCar _car;

    public Vector2 MousePosition = new(-1, -1);
    public Vector2 MousePositionWorld = new(-1, -1);
    public float CameraZoom = 12;

    private IDisposable? _updateSubscription;
    
    private int _tickRate = 60;
    private Stopwatch _lastUpdate = new();
    private FpsMeter _fpsMeter = new();

    private PopulationManager _populationManager;
    private Genome? _bestGenome = null;
    private int _playingIndex = 0;

    public ThirdCanvas()
    {
        InitializeComponent();
        _camera = new Camera(new Vector2((float)ActualWidth, (float)ActualHeight), new Vector2(0, 0), CameraZoom);
        _track = new WpfTrack();
        _track.Load(_trackPath);
        //_track.ShowCheckpoints = false;
        _car = new WpfCar(new Vector2(0, 0), 0);
        _populationManager = new PopulationManager(1000, _track);
    }

    /// <summary>The camera used to draw all objects on the canvas.</summary>
    public Camera Camera => _camera;
    public WpfTrack Track => _track;
    public WpfCar Car => _car;

    /// <summary>The time in milliseconds between each update.</summary>
    public double TargetRefreshTime => 1000d / _tickRate;
    public PopulationManager PopulationManager { get; set; }
    public List<Genome> BestOnGeneration { get; set; }

    public void Init()
    {
        _track.DrawOn(CanvasControl);
        _car.DrawOn(CanvasControl);
        _car.IsVisionActive = true;
        _car.Track = _track;

        var stopwatch = new Stopwatch();
        stopwatch.Start();

        _populationManager.RunSimulationParallel();
        var currentBest = _populationManager.Population[0];
        
        var iteration = 1;

        while (currentBest.CurrentDistance > 0.1 && iteration++ < 250)
        {
            _populationManager.PrepareNextGeneration();
            _populationManager.RunSimulationParallel(true);
            var report = _populationManager.AnalyzeGeneration();
            currentBest = _populationManager.Population[0];
            if (iteration % 1 == 0)
            {
                Console.WriteLine(report);
                Console.WriteLine($"Fitness: {currentBest.Fitness} " +
                                  $"Distance: {currentBest.CurrentDistance} " +
                                  $"Avg. Speed: {currentBest.AvgSpeed} " +
                                  $"Time: {currentBest.CurrentTime}\n\n");
                Console.WriteLine($"On checkpoint fitness: " +
                                  $"{currentBest.OnCheckpointFitness.Select(f => f.ToString()).Aggregate((a, b) => $"{a}, {b}")}");
            }
            //iteration++;
        }
        stopwatch.Stop();

        Console.WriteLine($"Time: {stopwatch.ElapsedMilliseconds}ms");
        _bestGenome = currentBest;
    }

    public void StartUpdates()
    {
        _updateSubscription = Observable.Interval(TimeSpan.FromMilliseconds(1000f / Config.TickRate))
            .Subscribe((l) =>
            {
                Dispatcher.Invoke(() =>
                {

                    if (_track.LoadStatus)
                    {
                        _track.Update(_camera);
                        _track.OnCheckpoint(_car.Position, _car.Width);
                    }

                    _car.Update(_camera);
                    MoveCamera();

                    if (_bestGenome != null)
                    {
                        Config.CarActions[_bestGenome.Genes[_playingIndex]](_car, 1f / Config.TickRate);
                        _car.Move(1f / Config.TickRate);
                        _playingIndex++;
                        if (_playingIndex >= _bestGenome.CurrentGene)
                        {
                            _playingIndex = 0;
                            _car.RemoveFrom(CanvasControl);
                            _car = new WpfCar(new Vector2(0, 0), 0)
                            {
                                IsVisionActive = true
                            };
                            _car.DrawOn(CanvasControl);
                            _track.Load(_trackPath);
                            _car.Track = _track;
                            _camera.Follow(() => _car.Position);
                        }
                    }
                    _car.UpdateVision();

                    _lastUpdate.Restart();
                });
            });

    }

    public void StopUpdates()
    {
        _updateSubscription?.Dispose();
        //_isCarUpdateThreadRunning = false;
    }

    public void MoveCamera()
    {
        //Console.WriteLine($"camera center abs position: {_camera.Position + _camera.Center }, car position: {_car.Position}");
        if (_camera.IsFollowing)
        {
            _camera.FollowUpdate(_lastUpdate.ElapsedMilliseconds / 1000f);
            //_camera.FollowUpdate(1000f / Config.TickRate);
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