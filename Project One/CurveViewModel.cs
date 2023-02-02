using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_One;

public class CurveViewModel
{
    public string FileName { get; set; }
    public string DisplayFileName => "name: " + FileName.Split("\\").Last();
    public int PointCount { get; set; }
    public string DisplayPointCount => "points: " + PointCount.ToString();
}