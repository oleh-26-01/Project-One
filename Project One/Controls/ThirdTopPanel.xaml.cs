using System.Windows.Controls;

namespace Project_One.Controls;

public partial class ThirdTopPanel : UserControl
{
    public ThirdTopPanel()
    {
        InitializeComponent();
    }

    public void Init(ThirdCanvas thirdCanvas, ThirdSidePanel thirdSidePanel)
    {
        FollowCarCheckBox.Click += (sender, e) =>
        {
            if (FollowCarCheckBox.IsChecked == true)
                thirdCanvas.Camera.Follow(() => thirdCanvas.Car.Position);
            else
                thirdCanvas.Camera.FollowStop();
        };
        thirdCanvas.Camera.Follow(FollowCarCheckBox.IsChecked == true ? () => thirdCanvas.Car.Position : null);
        ShowVisionCheckBox.Click += (sender, e) => { thirdCanvas.Car.IsVisionActive = ShowVisionCheckBox.IsChecked == true; };
    }
}