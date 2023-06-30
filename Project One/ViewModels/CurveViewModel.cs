using System.Linq;

namespace Project_One;

public class CurveViewModel
{
    public string FileName { get; set; } = "";
    public int PointCount { get; set; }
    public bool IsSelected { get; set; }
    public string ShortInfo => FileName.Split("\\").Last() + " (" + PointCount + ")";
}