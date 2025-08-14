using System;
using System.Collections.Generic;

namespace JoyConfig.Infrastructure.Models.GameplayEffectDatabase;

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
