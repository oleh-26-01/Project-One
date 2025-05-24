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
        {
            4, (car, dt) =>
            {
                car.TurnRight(dt);
                car.Stop(dt);
            }
        },
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

    public static readonly int TickRate = 30; // minimum 18
    public static readonly float MutationRate = 0.03f; // minimum 0.01f
    public static readonly int StepWidth = 3; // minimum 2

    public enum Origin : byte
    {
        Random,
        Crossover,
        RandomCrossover,
        Mutate,
        Best,
        ValueCrossover,
        SmoothCrossover
    }

    public static readonly float BestGenomesRate = 0.3f;
    public static readonly float CrossoverGenomesRate = 0.1f;
    public static readonly float RandomCrossGenomesRate = 0.1f;
    public static readonly float ValueCrossGenomesRate = 0.1f;
    public static readonly float SmoothCrossGenomesRate = 0.1f;
    public static readonly float MutatedGenomesRate = 0.2f;
    public static readonly float RandomGenomesRate = 0.1f; // equal what left

    public static readonly int MaxDegreeOfParallelism = 12;

    public static readonly ParallelOptions OptimizationOptions = new()
    {
        MaxDegreeOfParallelism = MaxDegreeOfParallelism
    };

    public static readonly int[] SumToAction = GetSumToAction();

    private static int[] GetSumToAction()
    {
        var size = CarActions.Count * 2;
        var newArray = new int[size];

        for (var i = 0; i < size; i++)
        {
            newArray[i] = i switch
            {
                3 => 0,
                7 => 0,
                11 => 1,
                15 => 2,
                12 => 3,
                14 => 4,
                _ => 0
            };
        }

        return newArray;
    }
}