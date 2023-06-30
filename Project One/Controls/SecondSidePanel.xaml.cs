using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using Project_One.Drawing.Wrappers;
using Project_One_Objects.Helpers;

namespace Project_One.Controls;

public partial class SecondSidePanel
{
    public const string FilesType = "crv";
    private Camera _camera;
    private float _cameraZoom;
    private CarWPF _car;
    private string _filesPath;
    private TrackViewModel? _selectedTrack;
    private TrackWPF _track;
    private ObservableCollection<TrackViewModel> _trackViewModels;

    public SecondSidePanel()
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

    public void Init(SecondCanvas secondCanvas, string filesPath)
    {
        _car = secondCanvas.Car;
        _track = secondCanvas.Track;
        _camera = secondCanvas.Camera;
        _cameraZoom = secondCanvas.CameraZoom;
        FilesPath = filesPath;

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

    public void View_OnClick(object sender, RoutedEventArgs e)
    {
        if (((Button)sender).DataContext is not TrackViewModel trackViewModel) return;

        _track.Load(trackViewModel.FileName);
        _car.Track = _track;
        _camera.Position = -_camera.Center;
        _camera.Zoom = _cameraZoom;
        if (_selectedTrack != null) _selectedTrack.IsVisible = false;

        _selectedTrack = trackViewModel;
        trackViewModel.IsVisible = true;

        Update();
    }

    public void Select_OnClick(object sender, RoutedEventArgs e)
    {
        if (((Button)sender).DataContext is not TrackViewModel trackViewModel) return;

        trackViewModel.IsSelected = !trackViewModel.IsSelected;
        Update();
    }
}