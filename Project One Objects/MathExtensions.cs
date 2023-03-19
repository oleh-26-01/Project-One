using System.Buffers;
using System.Numerics;
using Microsoft.Diagnostics.Tracing.Analysis.GC;

namespace Project_One_Objects;

public static class MathExtensions
{
    public const double OneRad = Math.PI / 180;
    public const double TwoPi = Math.PI * 2;

    /// <summary> Converts degrees to radians. </summary>
    /// <param name="degrees"> The angle in degrees. </param>
    /// <returns> The angle in radians. </returns>
    public static double ToRad(this double degrees)
    {
        return degrees * OneRad;
    }
    
    /// <summary> Converts radians to degrees. </summary>
    /// <param name="radians"> The angle in radians. </param>
    /// <returns> The angle in degrees. </returns>
    public static double ToDeg(this double radians)
    {
        return radians / OneRad;
    }

    /// <summary> Rotates a vector by a given angle in radians. </summary>
    /// <param name="v"> The vector to rotate. </param>
    /// <param name="angle"> The angle to rotate by. </param>
    /// <returns> The rotated vector. </returns>
    public static Vector2 Rotate(this Vector2 v, float angle)
    {
        return Vector2.Transform(v, Matrix3x2.CreateRotation(angle));
    }

    /// <summary> Rotates a vector by a given angle in radians. </summary>
    /// <param name="v"> The vector to rotate. </param>
    /// <param name="rotation"> The rotation matrix to use. </param>
    /// <returns> The rotated vector. </returns>
    public static Vector2 Rotate(this Vector2 v, Matrix3x2 rotation)
    {
        return Vector2.Transform(v, rotation);
    }

    /// <summary> Scales a vector by a scalar. </summary>
    /// <typeparam name="T"> The type of the scalar. </typeparam>
    /// <param name="v"> The vector to scale. </param>
    /// <param name="scalar"> The scalar to scale by. </param>
    /// <returns> The scaled vector. </returns>
    public static Vector2 Scale<T>(this Vector2 v, T scalar) where T : struct, IConvertible
    {
        var scale = Convert.ToSingle(scalar);
        return new Vector2(v.X * scale, v.Y * scale);
    }

    /// <summary> Returns the slope and intercept of a line defined by two points. </summary>
    /// <returns> The Vector2, where X is the slope and Y is the intercept. </returns>
    public static Vector2 SlopeIntercept(Vector2 v1, Vector2 v2)
    {
        if (v1.X - v2.X == 0) return new Vector2(float.MaxValue, v1.X);
        if (v1.Y - v2.Y == 0) return new Vector2(0, v1.Y);

        var slope = (v2.Y - v1.Y) / (v2.X - v1.X);
        var intercept = v1.Y - slope * v1.X;
        return new Vector2(slope, intercept);
    }

    public static float Cross(this Vector2 v1, Vector2 v2)
    {
        return v1.X * v2.Y - v1.Y * v2.X;
    }

    public static float Length(this Vector2 v)
    {
        return (float)Math.Sqrt(v.X * v.X + v.Y * v.Y);
    }

    public static float DistanceToSegment(Vector2 point, Vector2 lineStart, Vector2 lineEnd)
    {
        var A = lineEnd.Y - lineStart.Y;
        var B = lineStart.X - lineEnd.X;
        var C = A * lineStart.X + B * lineStart.Y;

        return (float)Math.Abs(A * point.X + B * point.Y - C) / (float)Math.Sqrt(A * A + B * B);
    }

    public static Vector2 LineIntersection(Vector2 a, Vector2 b, Vector2 c, Vector2 d)
    {
        Vector2 deltaAB = b - a;
        Vector2 deltaCD = d - c;
        float denominator = deltaAB.Cross(deltaCD);
        if (denominator == 0)
        {
            return new Vector2(float.NaN, float.NaN);
        }

        Vector2 deltaAC = c - a;
        float t = deltaAC.Cross(deltaCD) / denominator;
        return a + deltaAB * t;
    }

    public static Vector2 LineIntersectionOpt(Vector2 a, Vector2 deltaAB, Vector2 deltaCD, Vector2 deltaAC)
    {
        float denominator = deltaAB.Cross(deltaCD);
        if (denominator == 0)
        {
            return new Vector2(float.NaN, float.NaN);
        }

        float t = deltaAC.Cross(deltaCD) / denominator;
        return a + deltaAB * t;
    }

    /// <summary>
    /// Returns angles relative to the specific position and body angle.
    /// </summary>
    /// <param name="points"> The points to find the angles of. </param>
    /// <param name="position"> The position to find the angles relative to. </param>
    /// <param name="bodyAngle"> The body angle to find the angles relative to. </param>
    /// <param name="targetArray"> The array to store the angles in. </param>
    /// <returns></returns>
    public static float[] FindRelativeAngles(Vector2[] points, Vector2 position, double bodyAngle = 0, float[]? targetArray = null)
    {
        //var angles = ArrayPool<float>.Shared.Rent(points.Length);
        var angles = targetArray ?? new float[points.Length];
        for (var i = 0; i < points.Length; i++)
        {
            var point = points[i];
            var angle = Math.Atan2(point.Y - position.Y, point.X - position.X);
            angles[i] = (float)angle.Mod(TwoPi);
        }
        return angles;
    }

    public static float[] CalcVectorAngles(int count, double bodyAngle = 0, float[]? targetArray = null)
    {
        //var angles = ArrayPool<float>.Shared.Rent(count);
        var angles = targetArray ?? new float[count];
        var angle = 0d;
        var angleStep = TwoPi / count;
        for (var i = 0; i < count; i++)
        {
            angles[i] = (float)((bodyAngle + angle) % TwoPi); // !!!
            angle += angleStep;
        }
        return angles;
    }

    public static Vector2[] AngleToSlopeIntercept(float[] angles, Vector2 position, Vector2[]? targetArray = null)
    {
        //var points = ArrayPool<Vector2>.Shared.Rent(angles.Length);
        var points = targetArray ?? new Vector2[angles.Length];
        for (var i = 0; i < angles.Length; i++)
        {
            points[i].X = (float)Math.Tan(angles[i]);
            points[i].Y = position.Y - points[i].X * position.X;
        }
        return points;
    }

    public static bool IsAngleBetween<T>(T angle, T first, T second) where T : struct, IConvertible
    {
        var a = Convert.ToSingle(angle);
        var f = Convert.ToSingle(first);
        var s = Convert.ToSingle(second);
        if (f > s)
        {
            (f, s) = (s, f);
        }

        if (s - f > Math.PI)
        {
            (f, s) = (s, f + (float)TwoPi);
        }

        if (f <= a && a < s) 
        {
            return true;
        }

        a += (float)TwoPi;

        if (f <= a && a < s)
        {
            return true;
        }

        return false;
    }

    public static T Mod<T>(this T a, T b)
    {
        dynamic divisor = b;
        dynamic dividend = a % divisor;

        return dividend >= 0 ? dividend : dividend + divisor;
    }
}