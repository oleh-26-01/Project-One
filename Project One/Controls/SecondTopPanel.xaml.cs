using System.Windows.Controls;
using Project_One.Drawing;
using Project_One_Objects.Helpers;

namespace Project_One;

/// <summary>
///     Interaction logic for SecondTopPanel.xaml
/// </summary>
public partial class SecondTopPanel : UserControl
{
    private Camera _camera;
    private WpfCar _car;
    private FpsMeter _fpsMeter;

    public SecondTopPanel()
    {
        InitializeComponent();
    }

    public void Init(SecondCanvas secondCanvas, SecondSidePanel secondSidePanel)
    {
        _car = secondCanvas.Car;
        _camera = secondCanvas.Camera;

        FollowCarCheckBox.Click += (sender, e) =>
        {
            if (FollowCarCheckBox.IsChecked == true)
                _camera.Follow(() => _car.Position);
            else
                _camera.FollowStop();
        };
        _camera.Follow(FollowCarCheckBox.IsChecked == true ? () => _car.Position : null);
        ShowVisionCheckBox.Click += (sender, e) => { _car.IsVisionActive = ShowVisionCheckBox.IsChecked == true; };
    }
}