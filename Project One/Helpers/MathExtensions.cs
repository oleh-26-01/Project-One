using System;

namespace Project_One;

public static class MathExtensions
{
    public const double OneRad = Math.PI / 180;

    public static double ToRad(this double degrees)
    {
        return degrees * OneRad;
    }

    public static double ToDeg(this double radians)
    {
        return radians / OneRad;
    }
}