using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;

namespace Project_One;

public class BoolToColorConverter : IValueConverter
{
    public Brush SelectedBrush { get; set; }
    public Brush NotSelectedBrush { get; set; }

    public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
        if (value is bool)
        {
            return (bool)value ? SelectedBrush : NotSelectedBrush;
        }
        return NotSelectedBrush;
    }

    public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}