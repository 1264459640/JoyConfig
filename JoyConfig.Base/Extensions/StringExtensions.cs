using System;
using System.Globalization;

namespace JoyConfig.Base.Extensions;

/// <summary>
/// 字符串扩展方�?/// </summary>
public static class StringExtensions
{
    /// <summary>
    /// 检查字符串是否为空或仅包含空白字符
    /// </summary>
    /// <param name="value">要检查的字符�?/param>
    /// <returns>如果字符串为空或仅包含空白字符则返回true</returns>
    public static bool IsNullOrWhiteSpace(this string? value)
    {
        return string.IsNullOrWhiteSpace(value);
    }

    /// <summary>
    /// 安全地截取字符串
    /// </summary>
    /// <param name="value">源字符串</param>
    /// <param name="maxLength">最大长�?/param>
    /// <returns>截取后的字符�?/returns>
    public static string SafeSubstring(this string? value, int maxLength)
    {
        if (string.IsNullOrEmpty(value))
            return string.Empty;

        return value.Length <= maxLength ? value : value.Substring(0, maxLength);
    }

    /// <summary>
    /// 将字符串转换为标题格式（首字母大写）
    /// </summary>
    /// <param name="value">源字符串</param>
    /// <returns>标题格式的字符串</returns>
    public static string ToTitleCase(this string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return string.Empty;

        return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(value.ToLower());
    }

    /// <summary>
    /// 安全地比较字符串（忽略大小写�?    /// </summary>
    /// <param name="value">源字符串</param>
    /// <param name="other">比较字符�?/param>
    /// <returns>如果字符串相等则返回true</returns>
    public static bool EqualsIgnoreCase(this string? value, string? other)
    {
        return string.Equals(value, other, StringComparison.OrdinalIgnoreCase);
    }
}
