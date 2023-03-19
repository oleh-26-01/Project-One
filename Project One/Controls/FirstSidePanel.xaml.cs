using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Numerics;
using System.Windows;
using System.Windows.Controls;
using Project_One.Helpers;
using Project_One_Objects;

namespace Project_One;

public partial class FirstSidePanel : UserControl
{
    public const string FilesType = "crv";
    private WpfCurve _curve;
    private Camera _camera;
    private ObservableCollection<CurveViewModel> _curveViewModels;
    private string _filesPath;
    private FirstTopPanel _firstTopPanel;
    private CurveViewModel? _selectedCurve;

    public FirstSidePanel()
    {
        InitializeComponent();
    }

    public string FilesPath
    {
        get => _filesPath;
        set
        {
            if (!Directory.Exists(value)) return;
            _filesPath = value;
        }
    }

    public void Init(FirstCanvas firstCanvas, FirstTopPanel firstTopPanel, string filesPath)
    {
        _firstTopPanel = firstTopPanel;
        _curve = firstCanvas.Curve;
        _camera = firstCanvas.Camera;
        FilesPath = filesPath;

        _curveViewModels = new ObservableCollection<CurveViewModel>();
        CurvesList.ItemsSource = _curveViewModels;
        Update();
    }

    public void Update()
    {
        _curveViewModels.Clear();
        var files = Directory.GetFiles(FilesPath);
        foreach (var file in files)
        {
            if (!file.EndsWith(FilesType)) continue;
            var curve = new WpfCurve(file);
            var curveViewModel = new CurveViewModel
            {
                FileName = file,
                PointCount = curve.Points.Count,
                IsSelected = false
            };

            if (_selectedCurve != null)
                if (_selectedCurve.FileName == file)
                    curveViewModel = _selectedCurve;

            _curveViewModels.Add(curveViewModel);
        }
    }

    public string FindFreeCurveName()
    {
        var index = 1;
        var curveFileName = "";
        while (true)
        {
            curveFileName = FilesPath + "curve" + index.ToString().PadLeft(3, '0') + "." + FilesType;
            if (!File.Exists(curveFileName)) break;
            index++;
        }

        return curveFileName;
    }

    public void SaveCurve(object sender, RoutedEventArgs e, bool newFile)
    {
        var curveFileName = FindFreeCurveName();
        if (!newFile)
        {
            if (_selectedCurve != null)
            {
                _selectedCurve.IsSelected = false;
                curveFileName = _selectedCurve.FileName;
            }
        }

        _selectedCurve = new CurveViewModel
        {
            FileName = curveFileName,
            PointCount = _curve.Points.Count,
            IsSelected = true
        };

        _curve.Save(curveFileName);
        Update();
    }

    public void Select_OnClick(object sender, RoutedEventArgs e)
    {
        if (((Button)sender).DataContext is not CurveViewModel curveViewModel) return;

        _curve.Load(curveViewModel.FileName);
        _camera.Position = -_camera.Center;
        _camera.Zoom = 1;
        if (_selectedCurve != null) _selectedCurve.IsSelected = false;
        _selectedCurve = curveViewModel;
        curveViewModel.IsSelected = true;

        Update();
        _firstTopPanel.Update();
        _firstTopPanel.ConfirmOptAngle_OnClick(sender, e);
    }

    public void Delete_OnClick(object sender, RoutedEventArgs e)
    {
        var messageBoxResult = MessageBox.Show("Confirm deleting?", "Confirm", MessageBoxButton.YesNo);

        switch (messageBoxResult)
        {
            case MessageBoxResult.Yes:
            {
                if (((Button)sender).DataContext is CurveViewModel curveViewModel)
                {
                    File.Delete(curveViewModel.FileName);
                    _curveViewModels.Remove(curveViewModel);
                }

                break;
            }
            case MessageBoxResult.No:
                break;
        }
    }
}