using System.Numerics;

namespace Project_One_Objects.Helpers;

/// <summary> Compares two Vector2 by Y coordinate. </summary>
public class Vector2Comparer : IComparer<Vector2>
{
    public int Compare(Vector2 x, Vector2 y)
    {
        return x.Y.CompareTo(y.Y);
    }
}