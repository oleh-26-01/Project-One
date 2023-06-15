using Project_One.Drawing.WpfOnly;
using Project_One.Drawing.Wrappers;
using Project_One.Helpers;
using Project_One_Objects.AIComponents;
using Project_One_Objects.Helpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Project_One.Controls;

public partial class ThirdCanvas : UserControl
{
    private readonly string _trackPath = "C:\\Coding\\C#\\Project One\\Project One\\Curves\\curve009.crv";
    private FpsMeter _fpsMeter = new();
    private readonly Stopwatch _lastUpdate = new();
    private int _playingIndex;
    private bool Evolution = true;

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
        CameraZoomLabel = new WpfText(Strings.CameraZoom(Camera), new Vector2(0, 0), 16);
        Track = new TrackWPF();
        Track.Load(_trackPath);
        Track.Width = 5f;
        //Track.ShowCheckpoints = false;
        Car = new CarWPF(new Vector2(0, 0), 0);
        _populationManager = new PopulationManager(20, Track);
    }

    /// <summary>The camera used to draw all objects on the canvas.</summary>
    public Camera Camera { get; }

    public WpfText CameraZoomLabel { get; }

    public TrackWPF Track { get; }

    public CarWPF Car { get; private set; }

    /// <summary>The time in milliseconds between each update.</summary>
    public double TargetRefreshTime => 1000d / _tickRate;

    public PopulationManager PopulationManager { get; set; }
    public List<Genome> BestOnGeneration { get; set; }

    public void Init()
    {
        _populationManager.RunSimulationParallel();

        var isNextCheckpoint = false;
        var checkpoint = 0;

        var bestFitness = _populationManager.Population[0].Fitness;
        var iteration = 0;
        const int minIterations = 50;
        var iterationsWithoutImprovement = 0;
        const int minIterationsWithoutImprovement = 10;
        var iterationsWithoutSolution = 0;
        const int maxIterationsWithoutSolution = (minIterations + minIterationsWithoutImprovement) * 2;

        var stopwatch = new Stopwatch();
        stopwatch.Start();

        var checkpointsCount = Track.GetCheckpoints().Count;

        while (_populationManager.PrepareNextGeneration(isNextCheckpoint ? 1 : 0))
        {
            isNextCheckpoint = false;

            _populationManager.RunSimulationParallel(true);
            var report = _populationManager.AnalyzeGeneration();

            if (bestFitness < _populationManager.Population[0].Fitness && iteration >= minIterations)
            {
                bestFitness = _populationManager.Population[0].Fitness;
                iterationsWithoutImprovement = 0;
            }
            else if (iteration >= minIterations)
            {
                iterationsWithoutImprovement++;
            }

            if (!_populationManager.Population[0].GetSecondCheckpoint)
            {
                iterationsWithoutSolution++;
                if (iterationsWithoutSolution >= maxIterationsWithoutSolution)
                {
                    Console.WriteLine("No solution found, restarting...");
                    _populationManager.PrepareNextGeneration(2);
                    _populationManager.RunSimulationParallel(true);
                    report = _populationManager.AnalyzeGeneration();
                    iteration = 0;
                    iterationsWithoutImprovement = 0;
                    iterationsWithoutSolution = 0;
                }
            }
            else
            {
                iterationsWithoutSolution = 0;
            }

            iteration++;

            if (iteration < minIterations || iterationsWithoutImprovement < minIterationsWithoutImprovement || !_populationManager.Population[0].GetSecondCheckpoint) continue;
            iteration = 0;
            iterationsWithoutImprovement = 0;
            isNextCheckpoint = true;
            checkpoint++;
            Console.WriteLine($"Checkpoint {checkpoint}/{checkpointsCount - 2}");
            Console.WriteLine(report);
            var bestInfo = _populationManager.Population[0].GetInfo();
            Console.WriteLine($"Best's info: " +
                              $"Fitness: {bestInfo[0]} " +
                              $"Cur. gene: {bestInfo[3]} " +
                              $"Avg. speed: {bestInfo[4]}\n");
        }

        stopwatch.Stop();
        Evolution = false;
        Track.Width = 6;

        Console.WriteLine($"Genome Time: {_populationManager.BestGenes.Count * 1000f / Config.TickRate}ms");
        Console.WriteLine($"Time: {stopwatch.ElapsedMilliseconds}ms");

        CameraZoomLabel.DrawOn(CanvasControl);
        Camera.SetZoomUpdater(() => CameraZoomLabel.SetText(Strings.CameraZoom(Camera)));
        Track.DrawOn(CanvasControl);
        Car.DrawOn(CanvasControl);
        Car.IsVisionActive = true;
        Car.Track = Track;
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

                    if (Evolution == false)
                    {
                        Config.CarActions[_populationManager.BestGenes[_playingIndex]](Car, 1f / Config.TickRate);
                        Car.Move(1f / Config.TickRate);
                        //Console.WriteLine($"Speed: {Car.Speed}");
                        _playingIndex++;
                        if (_playingIndex == _populationManager.BestGenes.Count)
                        {
                            _playingIndex = 0;
                            Car.RemoveFrom(CanvasControl);
                            Car = new CarWPF(new Vector2(0, 0), 0)
                            {
                                IsVisionActive = Car.IsVisionActive
                            };
                            Car.DrawOn(CanvasControl);
                            Track.CurrentCheckpointIndex = 0;
                            Car.Track = Track;
                            Camera.Follow(() => Car.Position);
                        }
                    }

                    //Car.UpdateVisionOpt(1f / Config.TickRate);
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
            //Camera.FollowUpdate(_lastUpdate.ElapsedMilliseconds / 1000f);
            Camera.FollowUpdate(1f / Config.TickRate);
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