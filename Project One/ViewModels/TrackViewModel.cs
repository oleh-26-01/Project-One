using System.Linq;

namespace Project_One;

internal class TrackViewModel
{
    public string FileName { get; set; } = "";
    public int PointCount { get; set; }
    public string ShortInfo => FileName.Split("\\").Last() + " (" + PointCount + ")";
    public bool IsVisible { get; set; }
    public bool IsSelected { get; set; }
    public string DisplayButtonContext => IsSelected ? "❌" : "✔️";
    public string ButtonBrush => IsSelected ? "#CC0000" : "#339900";
}