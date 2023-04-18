using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Project_One_Objects.Helpers;

namespace Project_One_Objects.Environment;

public class MaterialQuad
{
    private readonly float _width;
    private readonly float _height;

    private readonly Vector2[] _vertices;

    public MaterialQuad(float width, float height, Vector2 position, double angle)
    {
        _width = width;
        _height = height;
        Position = position;
        Angle = angle;

        _vertices = new Vector2[4]
        {
            new(_height / 2f, -_width / 2f),
            new(_height / 2f, _width / 2f),
            new(-_height / 2f, _width / 2f),
            new(-_height / 2f, -_width / 2f)
        };
    }

    public Vector2 Position { get; set; }
    public double Angle { get; set; }

    public Vector2[] GetAbsoluteVertices(Vector2 position, double angle)
    {
        var vertices = new Vector2[4];

        var rotation = Matrix3x2.CreateRotation((float)Angle);

        var positionOffset = Position.Rotate((float)angle) + position;

        for (var i = 0; i < 4; i++)
        {
            vertices[i] = Vector2.Transform(_vertices[i], rotation) + positionOffset;
        }
        return vertices;
    }
}