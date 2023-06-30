using System.Numerics;

namespace Project_One_Objects.Helpers;

public static class MemoryExtensions
{
    public static void Copy<T>(Span<T> source, Span<T> destination)
    {
    }

    public static void FullCopyTo(this List<Vector2>[] source, List<Vector2>[] destination)
    {
        for (var i = 0; i < source.Length; i++)
        {
            destination[i] = new List<Vector2>();
            for (var j = 0; j < source[i].Count; j++) destination[i].Add(new Vector2(source[i][j].X, source[i][j].Y));
        }
    }

    public static void FullCopyTo(this Vector2[] source, Vector2[] destination)
    {
        for (var i = 0; i < source.Length; i++) destination[i] = new Vector2(source[i].X, source[i].Y);
    }
}