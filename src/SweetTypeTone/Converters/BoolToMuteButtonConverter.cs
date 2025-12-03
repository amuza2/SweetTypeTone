using Avalonia.Data.Converters;
using System;
using System.Globalization;

namespace SweetTypeTone.Converters;

public class BoolToMuteButtonConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool isMuted)
        {
            return isMuted ? "🔇 Unmute" : "🔊 Mute";
        }
        return "🔊 Mute";
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
