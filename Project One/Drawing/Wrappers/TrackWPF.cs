using System.Windows;
using System.Windows.Controls;
using System.Windows.Shapes;
using Project_One.Drawing.WpfOnly;
using Project_One_Objects.Environment;
using Project_One_Objects.Helpers;

namespace Project_One.Drawing.Wrappers;

public class TrackWPF : Track
{
    private readonly Line _checkpointLine;
    private readonly Polyline _polyLine;

    public TrackWPF(string filePath = "",
        float width = 6, float minCheckpointDistance = 10) : base(width, minCheckpointDistance)
    {
        _polyLine = WpfObjects.Polyline();
        _checkpointLine = WpfObjects.CheckpointLine();
        if (filePath != "") Load(filePath);
    }

    public bool ShowCheckpoints { get; set; } = true;

    public void DrawOn(Canvas canvas)
    {
        if (!canvas.Children.Contains(_polyLine)) canvas.Children.Add(_polyLine);
        if (!canvas.Children.Contains(_checkpointLine)) canvas.Children.Add(_checkpointLine);
    }

    public void RemoveFrom(Canvas canvas)
    {
        canvas.Children.Remove(_polyLine);
        canvas.Children.Remove(_checkpointLine);
    }

    public void SetVisibility(bool visible)
    {
        _polyLine.Visibility = visible ? Visibility.Visible : Visibility.Hidden;
        _checkpointLine.Visibility = visible && ShowCheckpoints ? Visibility.Visible : Visibility.Hidden;
    }

    public void Update(Camera camera)
    {
        var points = camera.ConvertOut(Points);

        var pointsCollection = _polyLine.Points;
        pointsCollection.Clear();
        foreach (var point in points) pointsCollection.Add(new Point(point.X, point.Y));
        if (points.Length > 0) pointsCollection.Add(new Point(points[0].X, points[0].Y));

        if (ShowCheckpoints && Checkpoints.Length > 0)
        {
            var checkpoint = Checkpoints[CurrentCheckpointIndex];
            var firstPoint = camera.ConvertOut(checkpoint.Item1);
            var secondPoint = camera.ConvertOut(checkpoint.Item2);
            _checkpointLine.X1 = firstPoint.X;
            _checkpointLine.Y1 = firstPoint.Y;
            _checkpointLine.X2 = secondPoint.X;
            _checkpointLine.Y2 = secondPoint.Y;
        }
    }
}