﻿using System.Diagnostics;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Project_One_Objects.AIComponents;

// ReSharper disable InconsistentNaming

namespace Project_One_Objects.Helpers;

public static class MathExtensions
{
    public const double OneRad = Math.PI / 180;
    public const double TwoPi = Math.PI * 2;
    public const float FTwoPi = (float)Math.PI * 2;
    public const float CloseToZero = 1e-5f;
    public static readonly Vector2 NaNVector2 = new(float.NaN, float.NaN);
    private static readonly Random Random = new();
    private const int _pseudoTanCount = 3600;
    private const int _pseudoRandomCount = 10_000;
    private const int _pseudoRandomDoubleCount = 10_000;
    private static readonly float[] _pseudoTan = GetPseudoTan();
    public static int[] PseudoRandomCarActions { get; private set; } = GetPseudoRandom();
    public static double[] PseudoRandomDouble { get; private set; } = GetPseudoRandomDouble();

    private static T[] GenerateArray<T>(int count, Func<int, T> generator, string arrayName)
    {
        Stopwatch start = new();
        start.Start();

        var array = new T[count + 1];
        for (var i = 0; i < count + 1; i++)
            array[i] = generator(i);

        start.Stop();
        Console.WriteLine($"{arrayName} took {start.ElapsedMilliseconds}ms");
        Console.WriteLine($"Size of {arrayName}: {array.Length * Unsafe.SizeOf<T>() / 1024}kb");

        return array;
    }

    private static int[] GetPseudoRandom()
    {
        return GenerateArray(_pseudoRandomCount, _ => Random.Next(0, Config.CarActions.Count), "Pseudo random");
    }

    private static float[] GetPseudoTan()
    {
        return GenerateArray(_pseudoTanCount, i => (float)Math.Tan(TwoPi * i / _pseudoTanCount), "Tan approximation");
    }

    private static double[] GetPseudoRandomDouble()
    {
        return GenerateArray(_pseudoRandomDoubleCount, _ => Random.NextDouble(), "Pseudo random double");
    }


    public static double ToRad(this double degrees)
    {
        return degrees * OneRad;
    }

    public static double ToDeg(this double radians)
    {
        return radians / OneRad;
    }

    public static Vector2 Rotate(this Vector2 v, float radians)
    {
        return Vector2.Transform(v, Matrix3x2.CreateRotation(radians));
    }

    public static Vector2 Rotate(this Vector2 v, Matrix3x2 rotation)
    {
        return Vector2.Transform(v, rotation);
    }

    public static Vector2 Scale(this Vector2 v, double scalar)
    {
        return new Vector2((float)(v.X * scalar), (float)(v.Y * scalar));
    }

    /// <summary> Returns the slope and intercept of angle line defined by two points. </summary>
    /// <returns> The Vector2, where X is the slope and Y is the intercept. </returns>
    public static Vector2 SlopeIntercept(Vector2 v1, Vector2 v2)
    {
        if (v1.X - v2.X == 0) return new Vector2(float.MaxValue, -float.MaxValue);

        if (v1.Y - v2.Y == 0) return v1 with { X = 0 };

        var slope = (v2.Y - v1.Y) / (v2.X - v1.X);
        var intercept = v1.Y - slope * v1.X;
        return new Vector2(slope, intercept);
    }

    public static float CrossProd(this Vector2 v1, Vector2 v2)
    {
        return v1.X * v2.Y - v1.Y * v2.X;
    }

    public static double Length(this Vector2 v)
    {
        return Math.Sqrt(v.X * v.X + v.Y * v.Y);
    }

    /// <summary>
    ///     Returns the distance between point and the line segment defined by lineStart and lineEnd.
    /// </summary>
    /// <param name="point"> The point to find the distance to. </param>
    /// <param name="lineStart"> The start of the line segment. </param>
    /// <param name="lineEnd"> The end of the line segment. </param>
    /// <returns> The distance between the point and the line segment. </returns>
    public static float DistanceToSegment(Vector2 point, Vector2 lineStart, Vector2 lineEnd)
    {
        var A = lineEnd.Y - lineStart.Y;
        var B = lineStart.X - lineEnd.X;
        var C = A * lineStart.X + B * lineStart.Y;

        return Math.Abs(A * point.X + B * point.Y - C) / (float)Math.Sqrt(A * A + B * B);
    }

    /// <summary>
    ///     Returns the intersection point of two lines defined by the points angle and first, and second and d.
    /// </summary>
    /// <param name="a"> The first point of the first line. </param>
    /// <param name="b"> The second point of the first line. </param>
    /// <param name="c"> The first point of the second line. </param>
    /// <param name="d"> The second point of the second line. </param>
    /// <returns> The intersection point of the two lines. </returns>
    public static Vector2 LineIntersection(Vector2 a, Vector2 b, Vector2 c, Vector2 d)
    {
        var deltaAB = b - a;
        var deltaCD = d - c;
        var denominator = deltaAB.CrossProd(deltaCD);
        if (denominator == 0) return NaNVector2;

        var deltaAC = c - a;
        var t = deltaAC.CrossProd(deltaCD) / denominator;
        return a + deltaAB * t;
    }

    /// <summary>
    ///     Calculate angles relative to the specific position and body angle.
    /// </summary>
    /// <param name="points"> The points to find the angles of. </param>
    /// <param name="position"> The position to find the angles relative to. </param>
    /// <param name="targetArray"> The array to store the angles in. </param>
    /// <param name="optimization"> Whether to use PseudoAngle or Atan2. </param>
    public static void CalcRelativeAngles(Vector2[] points, Vector2 position, float[] targetArray, bool optimization = false)
    {
        if (optimization)
        {
            for (var i = 0; i < points.Length; i++)
            {
                var point = points[i];
                targetArray[i] = PseudoAngle(point.X - position.X, point.Y - position.Y);
            }
        }
        else
        {
            for (var i = 0; i < points.Length; i++)
            {
                var point = points[i];
                var angle = Math.Atan2(point.Y - position.Y, point.X - position.X);
                targetArray[i] = (float)angle.Mod(TwoPi);
            }
        }
    }

    ///// <inheritdoc cref="CalcRelativeAngles(Vector2[],Vector2,float[])" />
    //public static void CalcRelativeAnglesOpt(Vector2[] points, Vector2 position, float[] targetArray)
    //{
    //    for (var i = 0; i < points.Length; i++)
    //    {
    //        var point = points[i];
    //        targetArray[i] = PseudoAngle(point.X - position.X, point.Y - position.Y);
    //    }
    //}

    /// <summary>
    ///     Calculate angles of the vectors, released in angle circle, relative to the specific body angle.
    /// </summary>
    /// <param name="count"> The number of vectors to find the angles of. </param>
    /// <param name="bodyAngle"> The body angle to find the angles relative to. </param>
    /// <param name="targetArray"> The array to store the angles in. </param>
    public static void CalcVectorAngles(int count, double bodyAngle, float[] targetArray)
    {
        var angle = 0d;
        var angleStep = TwoPi / count;
        for (var i = 0; i < count; i++)
        {
            targetArray[i] = (float)((bodyAngle + angle) % TwoPi); // !!!
            angle += angleStep;
        }
    }

    /// <summary> Transform angles to slope-intercept form. </summary>
    /// <param name="angles"> The angles to transform. </param>
    /// <param name="position"> The position to transform the angles relative to. </param>
    /// <param name="targetArray"> The array to store the slope-intercept form in. </param>
    /// <remarks> The slope-intercept form is stored in the Vector2, where X is the slope and Y is the intercept. </remarks>
    public static void AngleToSlopeIntercept(float[] angles, Vector2 position, Vector2[] targetArray)
    {
        for (var i = 0; i < angles.Length; i++)
        {
            //targetArray[i].X = (float)Math.Tan(angles[i]);
            targetArray[i].X = PseudoTan(angles[i]);
            targetArray[i].Y = position.Y - targetArray[i].X * position.X;
        }
    }

    /// <inheritdoc>
    ///     <summary> Check if an angle is between two other angles. </summary>
    ///     <remarks> Angles in radians. </remarks>
    /// </inheritdoc>
    public static bool IsAngleBetween(double angle, double first, double second)
    {
        if (first > second)
        {
            (first, second) = (second, first);
        }

        if (second - first > Math.PI)
        {
            (first, second) = (second, first + TwoPi);
        }

        return (first <= angle && angle < second) || (first <= angle + TwoPi && angle + TwoPi < second);
    }

    /// <inheritdoc>
    ///     <summary> Returns mod of angle and first. The result is always positive. </summary>
    ///     <param name="a"> The dividend. </param>
    ///     <param name="b"> The divisor. </param>
    ///     <returns> The remainder of angle / first. </returns>
    /// </inheritdoc>
    public static int Mod(this int a, int b)
    {
        return a - b * (int)Math.Floor(a / (double)b);
    }

    /// <inheritdoc cref="Mod(int, int)" />
    public static float Mod(this float a, float b)
    {
        return a - b * (float)Math.Floor(a / b);
    }

    /// <inheritdoc cref="Mod(int, int)" />
    public static double Mod(this double a, double b)
    {
        return a - b * Math.Floor(a / b);
    }

    private static float PseudoAngle(float dx, float dy)
    {
        var p = dx / (Math.Abs(dx) + Math.Abs(dy));
        return (dy < 0 ? 3 + p : 1 - p) * 1.57f;
    }

    private static float PseudoTan(float angle)
    {
        var index = (int)(angle / TwoPi * _pseudoTanCount);
        return _pseudoTan[index];
    }

    /// <summary>
    /// Sorts an array of Vector2 objects by their Y coordinate using the Insertion Sort algorithm.
    /// </summary>
    /// <param name="array">The array of Vector2 objects to be sorted.</param>
    public static void InsertionSort(Vector2[] array)
    {
        for (var i = 1; i < array.Length; i++)
        {
            var key = array[i];
            var j = i - 1;

            while (j >= 0 && array[j].Y > key.Y)
            {
                array[j + 1] = array[j];
                j--;
            }

            array[j + 1] = key;
        }
    }

    [DllImport("CMathExtension.dll")]
    public static extern void insertion_sort(IntPtr array, int length);

    public static void CInsertionSort(Vector2[] array)
    {
        var vectorSize = Marshal.SizeOf<Vector2>();
        var arraySize = array.Length;
        var vectorArrayPtr = Marshal.AllocHGlobal(vectorSize * arraySize);

        for (var i = 0; i < arraySize; i++)
        {
            Marshal.StructureToPtr(array[i], vectorArrayPtr + vectorSize * i, false);
        }

        insertion_sort(vectorArrayPtr, arraySize);

        for (var i = 0; i < arraySize; i++)
        {
            array[i] = Marshal.PtrToStructure<Vector2>(vectorArrayPtr + vectorSize * i);
        }

        Marshal.FreeHGlobal(vectorArrayPtr);
    }
}