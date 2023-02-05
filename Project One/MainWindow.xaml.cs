using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Numerics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Shapes;
using System.Windows.Threading;
using CommandLine.Text;
using Perfolizer.Mathematics.Randomization;
using Project_One_Objects;

namespace Project_One;

// better is to use curve as wpf control and raise event when curve is changed

public partial class MainWindow
{
    string[] _parts = { "First", "Second", "Third" };
    int _partIndex = 0;

    private Canvas[] _canvasArray;
    private Camera[] _cameraArray;

    private CanvasUpdater _canvasUpdater;
    private EventHandler _eventHandler;
    private TopPanel _topPanel;
    private SidePanel _sidePanel;

    private string _filesFolder = "C:\\Coding\\C#\\Project One\\Project One\\Curves\\";
    private WpfCurve _curve;
    private WpfCurveEraser _curveEraser;

    public MainWindow()
    {
        InitializeComponent();
        
        _partIndex = 0;
        _curve = new WpfCurve();
        _curveEraser = new WpfCurveEraser();

        _cameraArray = new Camera[3];
        _canvasArray = new Canvas[]
        {
            FirstCanvas,
            SecondCanvas,
            ThirdCanvas
        };

        var cameraSize = new Vector2((float)CanvasGrid.ActualWidth, (float)CanvasGrid.ActualHeight);
        for (var i = 0; i < 3; i++)
        {
            _cameraArray[i] = new Camera(cameraSize, new Vector2(0, 0));
        }

        Dispatcher dispatcher = Dispatcher;
        _canvasUpdater = new CanvasUpdater(
            dispatcher, _canvasArray, _cameraArray, 
            _curve, _curveEraser);

        _topPanel = new TopPanel(FirstTopPanel, _curve, _curveEraser);
        _sidePanel = new SidePanel(_filesFolder, CurvesList, _curve);

        _eventHandler = new EventHandler(
            WindowGrid, CanvasGrid, FirstTopPanel,
            _canvasUpdater, _topPanel, _sidePanel,
            _cameraArray, _curve, _curveEraser);

        Closing += _eventHandler.WindowClosing;
        WindowGrid.Focus();

        //Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    { 
        var canvasSize = new Vector2((float)CanvasGrid.ActualWidth, (float)CanvasGrid.ActualHeight);
        var actualCenter = canvasSize / 2;
        for (var i = 0; i < 3; i++)
        {
            _cameraArray[i].Center = actualCenter;
        }
        
    }

    private void ChangePart(object sender, RoutedEventArgs e)
    {
        _canvasArray[_partIndex].Visibility = Visibility.Hidden;
        if ((Button)sender == NextPartControl)
        {
            _partIndex++;
        }
        else if ((Button)sender == PrevPartControl)
        {
            _partIndex--;
        }

        _partIndex += _parts.Length;
        _partIndex %= _parts.Length;

        Console.WriteLine($"{_parts[_partIndex]} Part: {_partIndex}");
        PartTitle.Text = $"{_parts[_partIndex]} Part";

        _canvasUpdater.CanvasIndex = _partIndex;
        _eventHandler.CanvasIndex = _partIndex;
        _canvasArray[_partIndex].Visibility = Visibility.Visible;
    }

    private void DeleteCurve_OnClick(object sender, RoutedEventArgs e)
    {
        _eventHandler.DeleteCurve_OnClick(sender, e);
    }

    private void SelectCurve_OnClick(object sender, RoutedEventArgs e)
    {
        _eventHandler.SelectCurve_OnClick(sender, e);
    }
}