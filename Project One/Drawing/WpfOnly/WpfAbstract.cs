using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Project_One_Objects.Helpers;

namespace Project_One.Drawing.WpfOnly;

public abstract class WpfAbstract
{
    internal List<dynamic> Objects = new();

    public void DrawOn(Canvas canvas)
    {
        foreach (var obj in Objects.Where(obj => !canvas.Children.Contains(obj))) canvas.Children.Add(obj);
    }

    public void RemoveFrom(Canvas canvas)
    {
        foreach (var obj in Objects.Where(obj => canvas.Children.Contains(obj))) canvas.Children.Remove(obj);
    }

    public void SetVisibility(bool visible)
    {
        foreach (var obj in Objects) obj.Visibility = visible ? Visibility.Visible : Visibility.Hidden;
    }

    public void Update(Camera camera)
    {
    }
}