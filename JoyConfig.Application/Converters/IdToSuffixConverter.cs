using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace JoyConfig.Application.Converters;

public class IdToSuffixConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is string id)
        {
            var parts = id.Split('.');
            return parts.Length > 1 ? parts[^1] : id;
        }
        return value;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
