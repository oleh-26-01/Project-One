using Project_One_Objects.AIComponents;

namespace Project_One.ViewModels;

public class EvolutionViewModel
{
    private bool _isTraining = false;
    private bool _isTrainingPaused = false;
    private PopulationManager? _populationManager;

    public string ActionButtonContext => _isTraining ? _isTrainingPaused ? "Resume" : "Pause" : "Start";

    public string ActionButtonBackground => _isTraining ? _isTrainingPaused ? "#CC9900" : "#CC0000" : "#CC9900";

    public string ProgressInfo =>
        $"Progress: {_populationManager!.EvolutionStep}/{_populationManager!.StepsCount}";

    public string CurrentValueInfo => "Current AS: 0";
    public string TimeSpendInfo => "Time Spend: 0";
    public string SaveValueInfo => "Archive AS: 0";
    public string ValueToolTip => "Average Speed";
}