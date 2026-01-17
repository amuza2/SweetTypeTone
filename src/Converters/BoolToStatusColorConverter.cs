using Avalonia.Data.Converters;
using Avalonia.Media;
using System;
using System.Globalization;

namespace SweetTypeTone.Converters;

public class BoolToStatusColorConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool isActive)
        {
            return isActive ? new SolidColorBrush(Color.Parse("#4ECCA3")) : new SolidColorBrush(Color.Parse("#888888"));
        }
        return new SolidColorBrush(Color.Parse("#888888"));
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
