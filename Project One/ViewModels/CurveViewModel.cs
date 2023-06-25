using System.Linq;

namespace Project_One;

public class CurveViewModel
{
    public string FileName { get; set; }
    public string DisplayFileName => "name: " + FileName.Split("\\").Last();
    public int PointCount { get; set; }
    public string DisplayPointCount => "points: " + PointCount;
    public bool IsSelected { get; set; }
    public string ShortInfo => FileName.Split("\\").Last() + " (" + PointCount + ")";
}