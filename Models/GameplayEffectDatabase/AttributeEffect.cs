using System;
using System.Collections.Generic;

namespace JoyConfig.Models.GameplayEffectDatabase;

public partial class AttributeEffect
{
    public string Id { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public string EffectType { get; set; } = null!;

    public string StackingType { get; set; } = null!;

    public string? Tags { get; set; }

    public double? DurationSeconds { get; set; }

    public bool? IsInfinite { get; set; }

    public int? MaxStacks { get; set; }

    public bool? IsPassive { get; set; }

    public int? Priority { get; set; }

    public bool? IsPeriodic { get; set; }

    public double? IntervalSeconds { get; set; }

    public string? SourceType { get; set; }

    public virtual ICollection<AttributeModifier> AttributeModifiers { get; set; } = new List<AttributeModifier>();
}

/// <summary>
/// 效果类型枚举
/// </summary>
public static class EffectTypes
{
    public const string Instant = "Instant";
    public const string Duration = "Duration";
    public const string Infinite = "Infinite";

    public static readonly string[] All = { Instant, Duration, Infinite };
}

/// <summary>
/// 堆叠类型枚举
/// </summary>
public static class StackingTypes
{
    public const string NoStack = "NoStack";
    public const string Stack = "Stack";
    public const string Replace = "Replace";
    public const string Duration = "Duration";

    public static readonly string[] All = { NoStack, Stack, Replace, Duration };
}

/// <summary>
/// 来源类型枚举
/// </summary>
public static class SourceTypes
{
    public const string Equipment = "Equipment";
    public const string Skill = "Skill";
    public const string Buff = "Buff";
    public const string Environment = "Environment";
    public const string System = "System";

    public static readonly string[] All = { Equipment, Skill, Buff, Environment, System };
}

/// <summary>
/// 标签类型枚举
/// </summary>
public static class TagTypes
{
    public const string Physical = "Physical";
    public const string Mental = "Mental";
    public const string Environmental = "Environmental";
    public const string Magical = "Magical";
    public const string Technological = "Technological";
    public const string Temporary = "Temporary";
    public const string Permanent = "Permanent";

    public static readonly string[] All = { Physical, Mental, Environmental, Magical, Technological, Temporary, Permanent };
}
