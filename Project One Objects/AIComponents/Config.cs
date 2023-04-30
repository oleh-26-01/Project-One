using Project_One_Objects.Environment;

namespace Project_One_Objects.AIComponents;

public static class Config
{
    public static readonly Dictionary<int, Action<Car, float>> CarActions = new()
    {
        {
            0, (car, dt) =>
            {
                car.StopTurning(dt);
                car.Stop(dt);
            }
        },
        {
            1, (car, dt) =>
            {
                car.SpeedUp(dt);
                car.StopTurning(dt);
            }
        },
        {
            2, (car, dt) =>
            {
                car.SpeedDown(dt);
                car.StopTurning(dt);
            }
        },
        {
            3, (car, dt) =>
            {
                car.TurnLeft(dt);
                car.Stop(dt);
            }
        },
        { 4, (car, dt) => car.TurnRight(dt) },
        {
            5, (car, dt) =>
            {
                car.TurnLeft(dt);
                car.SpeedUp(dt);
            }
        },
        {
            6, (car, dt) =>
            {
                car.TurnRight(dt);
                car.SpeedUp(dt);
            }
        },
        {
            7, (car, dt) =>
            {
                car.TurnLeft(dt);
                car.SpeedDown(dt);
            }
        },
        {
            8, (car, dt) =>
            {
                car.TurnRight(dt);
                car.SpeedDown(dt);
            }
        }
    };
    public static readonly List<int> CarActionsKeys = CarActions.Keys.ToList();

    public static readonly int TickRate = 30; // need to test up to 18 ticks per second.
    public static readonly float MutationRate = 0.01f;
    public static readonly Dictionary<int, string> Origins = new()
    {
        { 0, "Random" },
        { 1, "Crossover" },
        { 2, "RandomCross" },
        { 3, "Mutate" },
        { 4, "Best" },
        { 5, "SurvivorIssue" }
    };
    public static readonly Dictionary<string, int> OriginsKeys = Origins.ToDictionary(x => x.Value, x => x.Key);

    public static readonly float BestGenomesRate = 0.5f;
    public static readonly float RandomCrossGenomesRate = 0.225f;
    public static readonly float CrossoverGenomesRate = 0.225f;
    public static readonly float MutatedGenomesRate = 0.05f;
    public static readonly float RandomGenomesRate = 0.1f;
    
    public static readonly int MaxDegreeOfParallelism = 5;
}