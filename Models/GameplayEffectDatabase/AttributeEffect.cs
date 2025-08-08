using System;
using System.Collections.Generic;

namespace AttributeDatabaseEditor.Models.GameplayEffectDatabase;

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
