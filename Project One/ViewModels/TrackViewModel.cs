using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Project_One;

class TrackViewModel
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