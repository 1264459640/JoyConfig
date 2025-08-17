using System;
using System.Collections.Generic;

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

    public static readonly string[] All = { Add, Subtract, Multiply, Override, Percentage };
}
