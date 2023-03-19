using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using Project_One.Drawing;
using Project_One_Objects;

namespace Project_One;

/// <summary>
/// Interaction logic for SecondSidePanel.xaml
/// </summary>
public partial class SecondSidePanel : UserControl
{
    public const string FilesType = "crv";
    private string _filesPath;
    private WpfCar _car;
    private WpfTrack _track;
    private Camera _camera;
    private float _cameraZoom;
    private ObservableCollection<TrackViewModel> _trackViewModels;
    private TrackViewModel? _selectedTrack;

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
            var track = new WpfTrack(file);
            var trackViewModel = new TrackViewModel
            {
                FileName = file,
                PointCount = track.Points.Length,
                IsSelected = false
            };
            if (_selectedTrack != null)
                if (_selectedTrack.FileName == file)
                {
                    trackViewModel = _selectedTrack;
                }

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