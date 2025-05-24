using Project_One_Objects.AIComponents;
using System.Windows.Media;

namespace Project_One.ViewModels;

public class EvolutionViewModel
{
    private bool _isTraining = false;
    private bool _isTrainingPaused = false;
    private PopulationManager? _populationManager;

    public string ActionButtonContext => _isTraining ? _isTrainingPaused ? "Resume" : "Pause" : "Start";

    // Changed ActionButtonBackground to return Brush objects
    public Brush ActionButtonBackground => _isTraining 
        ? (_isTrainingPaused 
            ? new SolidColorBrush((Color)ColorConverter.ConvertFromString("#CC9900")) // Resume state (Orange/Yellow)
            : new SolidColorBrush((Color)ColorConverter.ConvertFromString("#CC0000"))) // Pause state (Red)
        : new SolidColorBrush((Color)ColorConverter.ConvertFromString("#388E3C"));    // Start state (Green, using ConstructiveActionBrush color)

    public string ProgressInfo =>
        $"Progress: {_populationManager!.EvolutionStep}/{_populationManager!.StepsCount}";

    public string CurrentValueInfo => "Current AS: 0";
    public string TimeSpendInfo => "Time Spend: 0";
    public string SaveValueInfo => "Archive AS: 0";
    public string ValueToolTip => "Average Speed";
}