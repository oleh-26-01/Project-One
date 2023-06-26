using System.Windows.Controls;
using Project_One.ViewModels;

namespace Project_One.Controls;

public partial class ThirdTopPanel : UserControl
{
    private EvolutionViewModel _evolutionViewModel;


    public ThirdTopPanel()
    {
        InitializeComponent();
    }

    public void Init(ThirdCanvas thirdCanvas, ThirdSidePanel thirdSidePanel)
    {
        FollowCarCheckBox.Click += (_, _) =>
        {
            if (FollowCarCheckBox.IsChecked == true)
                thirdCanvas.Camera.Follow(() => thirdCanvas.Car.Position);
            else
                thirdCanvas.Camera.FollowStop();
        };
        thirdCanvas.Camera.Follow(FollowCarCheckBox.IsChecked == true ? () => thirdCanvas.Car.Position : null);

        _evolutionViewModel = new EvolutionViewModel();
        DataContext = _evolutionViewModel;
    }
}