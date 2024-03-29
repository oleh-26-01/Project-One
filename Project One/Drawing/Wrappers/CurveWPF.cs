﻿using System.Windows;
using System.Windows.Controls;
using System.Windows.Shapes;
using Project_One.Drawing.WpfOnly;
using Project_One_Objects.Environment;
using Project_One_Objects.Helpers;

namespace Project_One.Drawing.Wrappers;

public class CurveWPF : Curve
{
    private readonly Polyline _polyLine;

    public CurveWPF(string filePath = "", double optimizationAngle = 0) : base(optimizationAngle)
    {
        _polyLine = WpfObjects.Polyline();
        if (filePath != "") _ = Load(filePath);
    }

    public void DrawOn(Canvas canvas)
    {
        if (!canvas.Children.Contains(_polyLine)) _ = canvas.Children.Add(_polyLine);
    }

    public void RemoveFrom(Canvas canvas)
    {
        canvas.Children.Remove(_polyLine);
    }

    public void SetVisibility(bool visible)
    {
        _polyLine.Visibility = visible ? Visibility.Visible : Visibility.Hidden;
    }

    public void Update(Camera camera)
    {
        var points = camera.ConvertOut(Points);

        var pointsCollection = _polyLine.Points;
        pointsCollection.Clear();
        foreach (var point in points) pointsCollection.Add(new Point(point.X, point.Y));
    }
}