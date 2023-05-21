using System.Windows.Controls;
using Project_One.Drawing;
using Project_One_Objects.Helpers;

namespace Project_One;

/// <summary>
///     Interaction logic for ThirdTopPanel.xaml
/// </summary>
public partial class ThirdTopPanel : UserControl
{
    private Camera _camera;
    private WpfCar _car;
    private FpsMeter _fpsMeter;

    public ThirdTopPanel()
    {
        InitializeComponent();
    }

    public void Init(ThirdCanvas thirdCanvas, ThirdSidePanel thirdSidePanel)
    {
        _car = thirdCanvas.Car;
        _camera = thirdCanvas.Camera;

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