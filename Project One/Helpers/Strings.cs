using Project_One_Objects.Helpers;
using System;

namespace Project_One.Helpers;

internal class Strings
{
    public const string DrawAction = "DrawAction";
    public const string EraseAction = "EraseAction";
    public const string ChangingOptAngleAction = "ChangingOptAngleAction";

    public const string NewFileAction = "NewFileAction";
    public const string UpdateFileAction = "UpdateFileAction";

    public const string UnknownAction = "UnknowAction";

    public const string Forward = "Forward";
    public const string Backward = "Backward";

    public const string Collision = "Collided";
    public const string NoCollision = "No collision";

    public static string CameraZoom(Camera camera)
    {
        var zoom = Math.Round(camera.Zoom, Math.Clamp((int)Math.Log10(1 / camera.Zoom) + 1, 0, 15));
        return $"(±): {zoom}";
    }
}