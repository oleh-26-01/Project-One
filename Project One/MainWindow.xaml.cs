using System;
using System.Numerics;
using System.Windows;
using System.Windows.Controls;
using Project_One_Objects;

namespace Project_One;

// better is to use curve as wpf control and raise event when curve is changed

public partial class MainWindow
{
    private readonly Camera[] _cameraArray;

    private readonly Canvas[] _canvasArray;

    private readonly CanvasUpdater _canvasUpdater;
    private readonly WpfCurve _curve;
    private readonly WpfCurveEraser _curveEraser;
    private readonly EventsHandler _eventHandler;
    private readonly string _filesPath = "C:\\Coding\\C#\\Project One\\Project One\\Curves\\";
    private int _partIndex;
    private readonly string[] _parts = { "First", "Second", "Third" };

    public MainWindow()
    {
        InitializeComponent();

        _partIndex = 0;
        _curve = new WpfCurve();
        _curveEraser = new WpfCurveEraser();

        _cameraArray = new Camera[3];
        _canvasArray = new[]
        {
            FirstCanvas,
            SecondCanvas,
            ThirdCanvas
        };

        var cameraSize = new Vector2((float)CanvasGrid.ActualWidth, (float)CanvasGrid.ActualHeight);
        for (var i = 0; i < 3; i++) _cameraArray[i] = new Camera(cameraSize, new Vector2(0, 0));

        var dispatcher = Dispatcher;
        _canvasUpdater = new CanvasUpdater(
            dispatcher, _canvasArray, _cameraArray,
            _curve, _curveEraser);

        FirstTopPanel.Init(FirstSidePanel, _curve, _curveEraser, this);
        FirstSidePanel.Init(FirstTopPanel, _curve, _filesPath);


        _eventHandler = new EventsHandler(
            this,
            WindowGrid, CanvasGrid,
            _canvasUpdater, FirstTopPanel, FirstSidePanel,
            _cameraArray, _curve, _curveEraser);

        Closing += _eventHandler.WindowClosing;

        //Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
    }

    private void ChangePart(object sender, RoutedEventArgs e)
    {
        _canvasArray[_partIndex].Visibility = Visibility.Hidden;
        if ((Button)sender == NextPartControl)
            _partIndex++;
        else if ((Button)sender == PrevPartControl) 
            _partIndex--;

        _partIndex += _parts.Length;
        _partIndex %= _parts.Length;

        Console.WriteLine($"{_parts[_partIndex]} Part: {_partIndex}");
        PartTitle.Text = $"{_parts[_partIndex]} Part";

        _canvasUpdater.CanvasIndex = _partIndex;
        _eventHandler.CanvasIndex = _partIndex;
        _canvasArray[_partIndex].Visibility = Visibility.Visible;
    }
}