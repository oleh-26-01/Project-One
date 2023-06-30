using System;
using System.Diagnostics;
using System.Numerics;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Input;
using Project_One.Drawing.WpfOnly;
using Project_One.Drawing.Wrappers;
using Project_One.Helpers;
using Project_One_Objects.AIComponents;
using Project_One_Objects.Helpers;

namespace Project_One.Controls;

public partial class ThirdCanvas
{
    private const string TrackPath = "C:\\Coding\\C#\\Project One\\Project One\\Curves\\curve001.crv";
    private readonly Stopwatch _lastUpdate = new();
    private readonly PopulationManager _populationManager;
    private bool _evolution = true;
    private int _playingIndex;

    private IDisposable? _updateSubscription;
    public float CameraZoom = 12;

    public Vector2 MousePosition = new(-1, -1);
    public Vector2 MousePositionWorld = new(-1, -1);

    public ThirdCanvas()
    {
        InitializeComponent();
        Camera = new Camera(new Vector2((float)ActualWidth, (float)ActualHeight), new Vector2(0, 0), CameraZoom);
        CameraZoomLabel = new WpfText(Strings.CameraZoom(Camera), new Vector2(0, 0), 16);
        Track = new TrackWPF
        {
            Width = 5f
        };
        Track.Load(TrackPath);
        //Track.ShowCheckpoints = false;
        Car = new CarWPF(new Vector2(0, 0), 0)
        {
            Track = Track,
            IsVisionActive = true,
            OptimizedCalculation = false
        };
        _populationManager = new PopulationManager(100, Track);
    }

    /// <summary>The camera used to draw all objects on the canvas.</summary>
    public Camera Camera { get; }

    public WpfText CameraZoomLabel { get; }

    public TrackWPF Track { get; }

    public CarWPF Car { get; }

    public void Init()
    {
        CameraZoomLabel.DrawOn(CanvasControl);
        Camera.SetZoomUpdater(() => CameraZoomLabel.SetText(Strings.CameraZoom(Camera)));
        Track.DrawOn(CanvasControl);
        Car.DrawOn(CanvasControl);

        _populationManager.RunSimulationParallel(true);

        // temporary code
        var isNextCheckpoint = false;
        var checkpoint = 0;

        var bestFitness = _populationManager.Population[0].Fitness;
        var iteration = 0;
        const int minIterations = 50;
        var iterationsWithoutImprovement = 0;
        const int minIterationsWithoutImprovement = 10;
        var iterationsWithoutSolution = 0;
        const int maxIterationsWithoutSolution = (minIterations + minIterationsWithoutImprovement) * 10;

        Stopwatch stopwatch = new();
        stopwatch.Start();

        while (_populationManager.PrepareNextGeneration(isNextCheckpoint ? 1 : 0))
        {
            isNextCheckpoint = false;

            _populationManager.RunSimulationParallel(true);
            _ = _populationManager.AnalyzeGeneration();
            var bestOnGeneration = _populationManager.Population[0];

            if (bestFitness < bestOnGeneration.Fitness && iteration >= minIterations)
            {
                bestFitness = bestOnGeneration.Fitness;
                iterationsWithoutImprovement = 0;
            }
            else if (iteration >= minIterations)
            {
                iterationsWithoutImprovement++;
            }

            if (!bestOnGeneration.OnNextCheckpoint)
            {
                iterationsWithoutSolution++;
                if (iterationsWithoutSolution >= maxIterationsWithoutSolution)
                {
                    Console.WriteLine("No solution found, restarting...");
                    _ = _populationManager.PrepareNextGeneration(2);
                    _populationManager.RunSimulationParallel(true);
                    _ = _populationManager.AnalyzeGeneration();
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

            if (iteration < minIterations || iterationsWithoutImprovement < minIterationsWithoutImprovement ||
                !bestOnGeneration.OnNextCheckpoint) continue;

            iteration = 0;
            iterationsWithoutImprovement = 0;
            isNextCheckpoint = true;
            checkpoint++;
            Console.WriteLine($"Checkpoint {checkpoint}/{Track.Checkpoints.Length - 2}");
            Console.WriteLine(_populationManager.AnalyzeGeneration(true));
            var bestInfo = bestOnGeneration.GetInfo();
            Console.WriteLine($"Best's info: " +
                              $"Fitness: {bestInfo[0]} " +
                              $"Cur. gene: {bestInfo[3]} " +
                              $"Avg. speed: {bestInfo[4]}\n");
        }

        stopwatch.Stop();
        Car.TrackWidth = 6f;
        _evolution = false;

        Console.WriteLine($"Genome Time: {_populationManager.BestGenes.Count * 1000f / Config.TickRate}ms");
        Console.WriteLine($"Time: {stopwatch.ElapsedMilliseconds}ms");
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
                        _ = Track.OnCheckpoint(Car.Position, Car.Width);
                    }

                    Car.Update(Camera);
                    MoveCamera();

                    if (_evolution == false)
                    {
                        Config.CarActions[_populationManager.BestGenes[_playingIndex]](Car, 1f / Config.TickRate);
                        Car.Move(1f / Config.TickRate);
                        //Console.WriteLine($"Speed: {Car.Speed}");
                        _playingIndex++;
                        if (_playingIndex == _populationManager.BestGenes.Count)
                        {
                            _playingIndex = 0;
                            Track.CurrentCheckpointIndex = 0;
                            Car.ResetState();
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
    }

    public void MoveCamera()
    {
        if (Camera.IsFollowing)
        {
            //Camera.FollowUpdate(_lastUpdate.ElapsedMilliseconds / 1000f);
            Camera.FollowUpdate(1f / Config.TickRate);
        }
        else
        {
            Vector2 cameraMoveDirection = new(0, 0);
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
        Vector2 newMousePosition = new((float)position.X, (float)position.Y);
        var mousePositionWorld = Camera.ConvertIn(newMousePosition);

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
            mousePosition.Y > ActualHeight)
            return;

        if (e.Delta > 0)
            Camera.ZoomIn(e.Delta / 1000f);
        else
            Camera.ZoomOut(-e.Delta / 1000f);
    }

    private void Canvas_OnSizeChanged(object sender, SizeChangedEventArgs e)
    {
        Vector2 canvasSize = new((float)e.NewSize.Width, (float)e.NewSize.Height);
        var newCenter = canvasSize / 2;

        Camera.Move(Camera.Center - newCenter);
        Camera.Center = newCenter;
    }
}