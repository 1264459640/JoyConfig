using System;
using System.Collections.Generic;
using System.Linq;

namespace JoyConfig.Models.GameplayEffectDatabase;

public partial class AttributeModifier
{
    public int Id { get; set; }

    public string EffectId { get; set; } = null!;

    public string AttributeType { get; set; } = null!;

    public string OperationType { get; set; } = null!;

    public double Value { get; set; }

    public int? ExecutionOrder { get; set; }

    public virtual AttributeEffect Effect { get; set; } = null!;
}

/// <summary>
/// 操作类型枚举
/// </summary>
public static class OperationTypes
{
    public const string Add = "Add";
    public const string Subtract = "Subtract";
    public const string Multiply = "Multiply";
    public const string Override = "Override";
    public const string Percentage = "Percentage";

    private static readonly List<string> _customTypes = new();
    private static readonly object _lock = new();

    /// <summary>
    /// 获取所有操作类型（包括自定义类型）
    /// </summary>
    public static string[] All
    {
        get
        {
            lock (_lock)
            {
                var defaultTypes = new[] { Add, Subtract, Multiply, Override, Percentage };
                return defaultTypes.Concat(_customTypes).Distinct().ToArray();
            }
        }
    }

    /// <summary>
    /// 添加自定义操作类型
    /// </summary>
    /// <param name="operationType">操作类型名称</param>
    public static void AddCustomType(string operationType)
    {
        if (string.IsNullOrWhiteSpace(operationType))
            return;

        lock (_lock)
        {
            if (!_customTypes.Contains(operationType) &&
                !new[] { Add, Subtract, Multiply, Override, Percentage }.Contains(operationType))
            {
                _customTypes.Add(operationType);
            }
        }
    }

    /// <summary>
    /// 移除自定义操作类型
    /// </summary>
    /// <param name="operationType">操作类型名称</param>
    public static void RemoveCustomType(string operationType)
    {
        if (string.IsNullOrWhiteSpace(operationType))
            return;

        lock (_lock)
        {
            _customTypes.Remove(operationType);
        }
    }

    /// <summary>
    /// 检查是否为有效的操作类型
    /// </summary>
    /// <param name="operationType">操作类型名称</param>
    /// <returns>是否有效</returns>
    public static bool IsValid(string operationType)
    {
        return All.Contains(operationType);
    }
}
