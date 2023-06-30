using System.Numerics;
using Project_One_Objects.Helpers;

namespace Project_One_Objects.Environment;

public class MaterialQuad
{
    private readonly Vector2[] _vertices;

    public MaterialQuad(float width, float height, Vector2 position, double angle)
    {
        Position = position;
        Angle = angle;

        _vertices = new Vector2[4]
        {
            new(height / 2f, -width / 2f),
            new(height / 2f, width / 2f),
            new(-height / 2f, width / 2f),
            new(-height / 2f, -width / 2f)
        };
    }

    public Vector2 Position { get; set; }
    public double Angle { get; set; }

    public Vector2[] GetAbsoluteVertices(Vector2 position, double angle)
    {
        var vertices = new Vector2[4];

        var rotation = Matrix3x2.CreateRotation((float)Angle);

        var positionOffset = Position.Rotate((float)angle) + position;

        for (var i = 0; i < 4; i++) vertices[i] = Vector2.Transform(_vertices[i], rotation) + positionOffset;
        return vertices;
    }
}