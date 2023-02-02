using System.Numerics;
using Project_One_Objects;

namespace Project_One_Tests
{
    public class Tests
    {
        private int _convertArraySize;
        private List<Vector2> _convertArray;


        [SetUp]
        public void Setup()
        {
            _convertArraySize = 100;
            _convertArray = new List<Vector2>();
            for (int i = 0; i < _convertArraySize; i++)
            {
                _convertArray.Add(new Vector2(i, i));
            }
        }


        [Test]
        public void ConvertIn_Point()
        {
            var camera = new Camera(new Vector2(1280, 720), new Vector2(100, 100), 1);
            Assert.IsTrue(camera.ConvertIn(new Vector2(300, 200)) == new Vector2(400, 300));
            
            camera.Zoom = 2;
            Assert.IsTrue(camera.ConvertIn(new Vector2(300, 200)) == new Vector2(250, 200));
            
            camera.Move(new Vector2(100, 100));
            Assert.IsTrue(camera.ConvertIn(new Vector2(300, 200)) == new Vector2(350, 300));
        }

        [Test]
        public void ConvertOut_Point()
        {
            var camera = new Camera(new Vector2(1280, 720), new Vector2(100, 100), 1);
            Assert.IsTrue(camera.ConvertOut(new Vector2(400, 300)) == new Vector2(300, 200));
            
            camera.Zoom = 2;
            Assert.IsTrue(camera.ConvertOut(new Vector2(400, 300)) == new Vector2(-40, 40));

            camera.Move(new Vector2(100, 100));
            Assert.IsTrue(camera.ConvertOut(new Vector2(400, 300)) == new Vector2(-240, -160));
        }

        [Test]
        public void ConvertIn_Array()
        {
            var camera = new Camera(new Vector2(1280, 720), new Vector2(100, 100), 1);
            List<Vector2> points = new List<Vector2>(_convertArray);

            var result = camera.ConvertIn(points);
            var expected = new Vector2[_convertArraySize];
            for (int i = 0; i < _convertArraySize; i++)
            {
                expected[i] = camera.ConvertIn(points[i]);
            }

            Assert.That(expected, Is.EqualTo(result));
            Assert.That(points, Is.EqualTo(_convertArray));
        }

        [Test]
        public void ConvertOut_Array()
        {
            var camera = new Camera(new Vector2(1280, 720), new Vector2(100, 100), 1);
            List<Vector2> points = new List<Vector2>(_convertArray);

            var result = camera.ConvertOut(points);
            var expected = new Vector2[_convertArraySize];
            for (int i = 0; i < _convertArraySize; i++)
            {
                expected[i] = camera.ConvertOut(points[i]);
            }

            Assert.That(expected, Is.EqualTo(result));
            Assert.That(points, Is.EqualTo(_convertArray));
        }
    }
}