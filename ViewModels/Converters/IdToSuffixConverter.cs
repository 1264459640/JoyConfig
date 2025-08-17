using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Avalonia.Data.Converters;

namespace JoyConfig.ViewModels.Converters;

public class IdToSuffixConverter : IMultiValueConverter
{
    public static readonly IdToSuffixConverter Instance = new();

    public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values.Count == 2 && values[0] is string id && values[1] is string category)
        {
            var prefix = $"{category}.";
            if (id.StartsWith(prefix))
            {
                return id.Substring(prefix.Length);
            }
            return id;
        }

        return values.FirstOrDefault();
    }
}
