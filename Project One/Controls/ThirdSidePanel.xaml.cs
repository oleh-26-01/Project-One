using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using Project_One.Drawing.Wrappers;
using Project_One_Objects.AIComponents;
using Project_One_Objects.Helpers;

namespace Project_One.Controls;

public partial class ThirdSidePanel : UserControl
{
    public const string FilesType = "crv";
    private Camera _camera;
    private float _cameraZoom;
    private CarWPF _car;
    private string _filesPath;
    private TrackViewModel? _selectedTrack;
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
            var track = new TrackWPF(file);
            var trackViewModel = new TrackViewModel
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

    public void Delete_OnClick(object sender, RoutedEventArgs e)
    {
        var messageBoxResult = MessageBox.Show("Confirm deleting?", "Confirm", MessageBoxButton.YesNo);

        switch (messageBoxResult)
        {
            case MessageBoxResult.Yes:
            {
                if (((Button)sender).DataContext is TrackViewModel trackViewModel)
                {
                    File.Delete(trackViewModel.FileName);
                    _trackViewModels.Remove(trackViewModel);
                }

                break;
            }
            case MessageBoxResult.No:
                break;
        }


        Update();
    }

    public void Select_OnClick(object sender, RoutedEventArgs e)
    {
        if (((Button)sender).DataContext is not TrackViewModel trackViewModel) return;

        trackViewModel.IsSelected = !trackViewModel.IsSelected;

        _track.Load(trackViewModel.FileName);
        var newPopulationManager = new PopulationManager(100, _track);
        _thirdCanvas.PopulationManager = newPopulationManager;
        _thirdCanvas.BestOnGeneration.Clear();
        _car.Track = _track;
        _camera.Position = -_camera.Center;
        _camera.Zoom = _cameraZoom;
        if (_selectedTrack != null) _selectedTrack.IsVisible = false;
        _selectedTrack = trackViewModel;
        trackViewModel.IsVisible = true;

        Update();
    }
}