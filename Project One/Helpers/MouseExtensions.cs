using System.Numerics;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Project_One_Objects.Helpers;

namespace Project_One;

public class MouseExtensions
{
    [DllImport("user32.dll")]
    private static extern bool SetCursorPos(int a, int b);

    public static Vector2 WrapMouseMove(IInputElement control)
    {
        if (control as Canvas is not { } canvas) return Vector2.Zero;
        var mouseOnControlPos = Mouse.GetPosition(canvas);
        var position = new Vector2((float)mouseOnControlPos.X, (float)mouseOnControlPos.Y);
        var dropMousePosition = new Vector2(
            position.X.Mod((float)canvas.ActualWidth),
            position.Y.Mod((float)canvas.ActualHeight));

        if (dropMousePosition == position) return position;
        var windowPosition = canvas.PointToScreen(new Point(0, 0));
        var newCursorPos = dropMousePosition + new Vector2(
            (float)windowPosition.X,
            (float)windowPosition.Y);
        SetCursorPos((int)newCursorPos.X, (int)newCursorPos.Y);
        return dropMousePosition;
    }
}