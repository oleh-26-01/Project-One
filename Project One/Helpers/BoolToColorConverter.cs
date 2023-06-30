using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace Project_One.Helpers;

internal class BoolToColorConverter : IValueConverter
{
    public Brush SelectedBrush { get; set; } = Brushes.Transparent;
    public Brush NotSelectedBrush { get; set; } = Brushes.Transparent;

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return (bool)value ? SelectedBrush : NotSelectedBrush;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}