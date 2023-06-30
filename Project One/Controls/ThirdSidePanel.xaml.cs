using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Project_One.Drawing.Wrappers;
using Project_One_Objects.AIComponents;
using Project_One_Objects.Helpers;

namespace Project_One.Controls;

public partial class ThirdSidePanel
{
    public const string FilesType = "crv";
    private TrackViewModel _selectedTrack;
    private Camera _camera;
    private float _cameraZoom;
    private CarWPF _car;
    private string _filesPath;

    private bool _propChanging;
    private ThirdCanvas _thirdCanvas;
    private TrackWPF _track;
    private ObservableCollection<TrackViewModel> _trackViewModels;

    public ThirdSidePanel()
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

    public void Init(ThirdCanvas thirdCanvas, string filesPath)
    {
        _car = thirdCanvas.Car;
        _track = thirdCanvas.Track;
        _camera = thirdCanvas.Camera;
        _cameraZoom = thirdCanvas.CameraZoom;
        FilesPath = filesPath;
        _thirdCanvas = thirdCanvas;

        _trackViewModels = new ObservableCollection<TrackViewModel>();
        TracksList.ItemsSource = _trackViewModels;
        Update();
    }

    public void Update()
    {
        _trackViewModels.Clear();
        var files = Directory.GetFiles(FilesPath);
        foreach (var file in files)
        {
            if (!file.EndsWith(FilesType)) continue;

            TrackWPF track = new(file);
            TrackViewModel trackViewModel = new()
            {
                FileName = file,
                PointCount = track.Points.Length,
                IsSelected = false
            };
            if (_selectedTrack != null)
                if (_selectedTrack.FileName == file)
                    trackViewModel = _selectedTrack;

            _trackViewModels.Add(trackViewModel);
        }
    }

    public void Select_OnClick(object sender, RoutedEventArgs e)
    {
        if (((Button)sender).DataContext is not TrackViewModel trackViewModel) return;

        _track.Load(trackViewModel.FileName);
        _car.Track = _track;
        _camera.Position = -_camera.Center;
        _camera.Zoom = _cameraZoom;
        _selectedTrack.IsVisible = false;
        _selectedTrack = trackViewModel;
        trackViewModel.IsVisible = true;

        Update();
    }

    private void StackPanelMouseClick(object sender, MouseButtonEventArgs e)
    {
        var isDown = e.LeftButton == MouseButtonState.Pressed;
        Mouse.Capture(isDown ? ExpanderControlStackPanel : null);
        _ = Mouse.SetCursor(isDown ? Cursors.Hand : Cursors.Arrow);
        _propChanging = isDown;
    }

    private void GridContent_OnMouseMove(object sender, MouseEventArgs e)
    {
        if (!_propChanging) return;

        var trackListHeight = e.GetPosition(GridContent).Y / GridContent.ActualHeight;
        trackListHeight = Math.Clamp(trackListHeight, 0, 1);
        var settingsHeight =
            1 - trackListHeight - GridContent.RowDefinitions[1].Height.Value / GridContent.ActualHeight;
        settingsHeight = Math.Clamp(settingsHeight, 0, 1);
        GridContent.RowDefinitions[0].Height = new GridLength(trackListHeight, GridUnitType.Star);
        GridContent.RowDefinitions[2].Height = new GridLength(settingsHeight, GridUnitType.Star);
    }

    private void TracksExpanderCollapse(object sender, RoutedEventArgs e)
    {
        GridContent.RowDefinitions[0].Height = new GridLength(1, GridUnitType.Auto);
        GridContent.RowDefinitions[1].Height = new GridLength(0, GridUnitType.Pixel);
        GridContent.RowDefinitions[2].Height = new GridLength(1, GridUnitType.Star);
    }

    private void SettingsExpanderCollapse(object sender, RoutedEventArgs e)
    {
        GridContent.RowDefinitions[1].Height = new GridLength(0, GridUnitType.Pixel);
        if (!TracksExpander.IsExpanded) return;

        GridContent.RowDefinitions[0].Height = new GridLength(1, GridUnitType.Star);
        GridContent.RowDefinitions[2].Height = new GridLength(1, GridUnitType.Auto);
    }

    private void ExpanderExpand(object sender, RoutedEventArgs e)
    {
        if (TracksExpander == null || SettingsExpander == null) return;

        var isFirstExpander = (Expander)sender == TracksExpander;
        if (isFirstExpander)
        {
            GridContent.RowDefinitions[0].Height = new GridLength(0.5, GridUnitType.Star);
            GridContent.RowDefinitions[1].Height = new GridLength(10, GridUnitType.Pixel);
        }
        else
        {
            GridContent.RowDefinitions[2].Height = new GridLength(0.5, GridUnitType.Star);
            if (TracksExpander.IsExpanded)
                GridContent.RowDefinitions[1].Height = new GridLength(10, GridUnitType.Pixel);
        }
    }
}