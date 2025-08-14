using System;
using System.Globalization;
using System.Linq;
using Avalonia.Data.Converters;

namespace JoyConfig.Application.ViewModels;

public class EnumToItemsSourceConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is Type type && type.IsEnum)
        {
            return Enum.GetValues(type).Cast<object>();
        }
        return null;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
