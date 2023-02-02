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

public partial class MainWindow
{
    string[] _parts = { "First", "Second", "Third" };
    int _partIndex = 0;

    private Canvas[] _canvasArray;
    private Camera[] _cameraArray;

    private CanvasUpdater _canvasUpdater;
    private EventHandler _eventHandler;
    private CurveListViewModel _curveListViewModel;

    private string _filesFolder = "C:\\Coding\\C#\\Project One\\Curves\\";
    private WpfCurve _curve;

    public MainWindow()
    {
        InitializeComponent();
        _curve = new WpfCurve();
        //_curve.Load(_filesFolder + "file.txt");
        Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        _partIndex = 0;
        var cameraSize = new Vector2((float)CanvasGrid.ActualWidth, (float)CanvasGrid.ActualHeight);
        _cameraArray = new Camera[3];
        for (var i = 0; i < 3; i++)
        {
            _cameraArray[i] = new Camera(cameraSize, new Vector2(0, 0));
        }

        _canvasArray = new Canvas[]
        {
            FirstCanvas,
            SecondCanvas,
            ThirdCanvas
        };

        Dispatcher dispatcher = Dispatcher;
        _canvasUpdater = new CanvasUpdater(dispatcher, _canvasArray, _cameraArray, _curve);
        
        // ----------------------------change to SidePanel------------------------------------
        _curveListViewModel = new CurveListViewModel(_filesFolder, CurveList, _curve); 
        // -----------------------------------------------------------------------------------
        
        _eventHandler = new EventHandler(
            WindowGrid, CanvasGrid, FirstTopPanel,
            _canvasUpdater, _curveListViewModel,
            _cameraArray, _curve);
        Closing += _eventHandler.WindowClosing;

        WindowGrid.Focus();
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
        _curveListViewModel.DeleteCurve(sender, e);
    }

    private void SelectCurve_OnClick(object sender, RoutedEventArgs e)
    {
        _curveListViewModel.SelectCurve(sender, e);
    }
}