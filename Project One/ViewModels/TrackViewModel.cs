using System.Linq;

namespace Project_One;

internal class TrackViewModel
{
    public string FileName { get; set; }
    public string DisplayFileName => "name: " + FileName.Split("\\").Last();
    public int PointCount { get; set; }
    public string DisplayPointCount => "points: " + PointCount;
    public bool IsVisible { get; set; }
    public bool IsSelected { get; set; }
    public string DisplayButtonContext => IsSelected ? "Unselect" : "Select";
    public string ButtonBrush => IsSelected ? "#CC0000" : "#CC9900";
}