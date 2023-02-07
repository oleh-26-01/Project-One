using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace Project_One;

internal class BoolToColorConverter : IValueConverter
{
    public Brush SelectedBrush { get; set; }
    public Brush NotSelectedBrush { get; set; }

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool) return (bool)value ? SelectedBrush : NotSelectedBrush;
        return NotSelectedBrush;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}