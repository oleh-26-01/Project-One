using System.Numerics;
using Project_One_Objects.Helpers;

namespace Project_One_Tests;

public class Tests
{
    private List<Vector2> _convertArray;
    private int _convertArraySize;

    public Tests(List<Vector2> convertArray)
    {
        _convertArray = convertArray;
    }


    [SetUp]
    public void Setup()
    {
        _convertArraySize = 100;
        _convertArray = new List<Vector2>();
        for (var i = 0; i < _convertArraySize; i++) _convertArray.Add(new Vector2(i, i));
    }


    [Test]
    public void ConvertIn_Point()
    {
        var camera = new Camera(new Vector2(640, 360), new Vector2(100, 100));
        Assert.IsTrue(camera.ConvertIn(new Vector2(300, 200)) == new Vector2(400, 300));

        camera.Zoom = 2;
        Assert.IsTrue(camera.ConvertIn(new Vector2(300, 200)) == new Vector2(570, 380));

        camera.Move(new Vector2(100, 100));
        Assert.IsTrue(camera.ConvertIn(new Vector2(300, 200)) == new Vector2(670, 480));
    }

    [Test]
    public void ConvertOut_Point()
    {
        var camera = new Camera(new Vector2(640, 360), new Vector2(100, 100));
        Assert.IsTrue(camera.ConvertOut(new Vector2(400, 300)) == new Vector2(300, 200));

        camera.Zoom = 2;
        Assert.IsTrue(camera.ConvertOut(new Vector2(400, 300)) == new Vector2(-40, 40));

        camera.Move(new Vector2(100, 100));
        Assert.IsTrue(camera.ConvertOut(new Vector2(400, 300)) == new Vector2(-240, -160));
    }

    [Test]
    public void ConvertIn_Array()
    {
        var camera = new Camera(new Vector2(1280, 720), new Vector2(100, 100));
        List<Vector2> points = new(_convertArray);

        var result = camera.ConvertIn(points);
        var expected = new Vector2[_convertArraySize];
        for (var i = 0; i < _convertArraySize; i++)
        {
            expected[i] = camera.ConvertIn(points[i]);
        }

        Assert.That(expected, Is.EqualTo(result));
        Assert.That(points, Is.EqualTo(_convertArray));
    }

    [Test]
    public void ConvertOut_Array()
    {
        var camera = new Camera(new Vector2(1280, 720), new Vector2(100, 100));
        List<Vector2> points = new(_convertArray);

        var result = camera.ConvertOut(points);
        var expected = new Vector2[_convertArraySize];
        for (var i = 0; i < _convertArraySize; i++)
        {
            expected[i] = camera.ConvertOut(points[i]);
        }

        Assert.That(expected, Is.EqualTo(result));
        Assert.That(points, Is.EqualTo(_convertArray));
    }
}