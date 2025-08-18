using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;
using JoyConfig.Models.GameplayEffectDatabase;

namespace JoyConfig.ViewModels.Converters;

/// <summary>
/// 操作类型颜色转换器
/// </summary>
public class OperationTypeColorConverter : IValueConverter
{
    public static readonly OperationTypeColorConverter Instance = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not string operationType)
            return Brushes.Gray;

        return operationType switch
        {
            OperationTypes.Add => Brushes.LimeGreen,      // 绿色 - 增加
            OperationTypes.Subtract => Brushes.OrangeRed, // 红色 - 减少
            OperationTypes.Multiply => Brushes.Orange,     // 橙色 - 乘法
            OperationTypes.Override => Brushes.Purple,     // 紫色 - 覆盖
            OperationTypes.Percentage => Brushes.DodgerBlue, // 蓝色 - 百分比
            _ => Brushes.Gray // 默认灰色
        };
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}