using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Project_One.Drawing;
using Project_One_Objects.AIComponents;
using Project_One_Objects.Helpers;

namespace Project_One;

public partial class ThirdCanvas : UserControl
{
    private readonly string _trackPath = "C:\\Coding\\C#\\Project One\\Project One\\Curves\\curve001.crv";
    private Genome? _bestGenome;
    private FpsMeter _fpsMeter = new();
    private readonly Stopwatch _lastUpdate = new();
    private int _playingIndex;

    private readonly PopulationManager _populationManager;

    private readonly int _tickRate = 60;

    private IDisposable? _updateSubscription;
    public float CameraZoom = 12;

    public Vector2 MousePosition = new(-1, -1);
    public Vector2 MousePositionWorld = new(-1, -1);

    public ThirdCanvas()
    {
        InitializeComponent();
        Camera = new Camera(new Vector2((float)ActualWidth, (float)ActualHeight), new Vector2(0, 0), CameraZoom);
        Track = new WpfTrack();
        Track.Load(_trackPath);
        //_track.ShowCheckpoints = false;
        Car = new WpfCar(new Vector2(0, 0), 0);
        _populationManager = new PopulationManager(200, Track);
    }

    /// <summary>The camera used to draw all objects on the canvas.</summary>
    public Camera Camera { get; }

    public WpfTrack Track { get; }

    public WpfCar Car { get; private set; }

    /// <summary>The time in milliseconds between each update.</summary>
    public double TargetRefreshTime => 1000d / _tickRate;

    public PopulationManager PopulationManager { get; set; }
    public List<Genome> BestOnGeneration { get; set; }

    public void Init()
    {
        Track.DrawOn(CanvasControl);
        Car.DrawOn(CanvasControl);
        Car.IsVisionActive = true;
        Car.Track = Track;

        var stopwatch = new Stopwatch();
        stopwatch.Start();

        _populationManager.RunSimulationParallel();
        var currentBest = _populationManager.Population[0];

        var iteration = 1;

        while (currentBest.CurrentDistance > 0.1 && iteration++ < 350)
        {
            _populationManager.PrepareNextGeneration();
            _populationManager.RunSimulationParallel(true);
            var report = _populationManager.AnalyzeGeneration();
            currentBest = _populationManager.Population[0];
            if (iteration % 10 == 0)
            {
                Console.WriteLine(report);
                Console.WriteLine($"Fitness: {currentBest.Fitness} " +
                                  $"Distance: {currentBest.CurrentDistance} " +
                                  $"Avg. Speed: {currentBest.AvgSpeed} " +
                                  $"Time: {currentBest.CurrentTime}\n\n");
                //Console.WriteLine($"On checkpoint fitness: " +
                //                  $"{currentBest.OnCheckpointFitness.Select(f => f.ToString()).Aggregate((a, b) => $"{a}, {b}")}");
                //Console.WriteLine($"Best's values: {currentBest.Values.Select(v => v.ToString()).Aggregate((a, b) => $"{a}, {b}")}");
                //iteration++;
            }
        }

        stopwatch.Stop();

        Console.WriteLine($"Time: {stopwatch.ElapsedMilliseconds}ms");
        _bestGenome = currentBest;
    }

    public void StartUpdates()
    {
        _updateSubscription = Observable.Interval(TimeSpan.FromMilliseconds(1000f / Config.TickRate))
            .Subscribe(l =>
            {
                Dispatcher.Invoke(() =>
                {
                    if (Track.LoadStatus)
                    {
                        Track.Update(Camera);
                        Track.OnCheckpoint(Car.Position, Car.Width);
                    }

                    Car.Update(Camera);
                    MoveCamera();

                    if (_bestGenome != null)
                    {
                        Config.CarActions[_bestGenome.Genes[_playingIndex]](Car, 1f / Config.TickRate);
                        Car.Move(1f / Config.TickRate);
                        _playingIndex++;
                        if (_playingIndex >= _bestGenome.CurrentGene)
                        {
                            _playingIndex = 0;
                            Car.RemoveFrom(CanvasControl);
                            Car = new WpfCar(new Vector2(0, 0), 0)
                            {
                                IsVisionActive = true
                            };
                            Car.DrawOn(CanvasControl);
                            Track.Load(_trackPath);
                            Car.Track = Track;
                            Camera.Follow(() => Car.Position);
                        }
                    }

                    Car.UpdateVision();

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
        if (Camera.IsFollowing)
        {
            Camera.FollowUpdate(_lastUpdate.ElapsedMilliseconds / 1000f);
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
                Camera.MoveSync(
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