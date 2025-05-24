using System.Linq;
using System.Windows.Media;

namespace Project_One;

internal class TrackViewModel
{
    public string FileName { get; set; } = "";
    public int PointCount { get; set; }
    public string ShortInfo => FileName.Split("\\").Last() + " (" + PointCount + ")";
    public bool IsVisible { get; set; }
    public bool IsSelected { get; set; }
    public string DisplayButtonContext => IsSelected ? "❌" : "✔️";
    // Changed ButtonBrush to return Brush objects instead of strings
    public Brush ButtonBrush => IsSelected 
        ? new SolidColorBrush((Color)ColorConverter.ConvertFromString("#CC0000")) 
        : new SolidColorBrush((Color)ColorConverter.ConvertFromString("#339900"));
}