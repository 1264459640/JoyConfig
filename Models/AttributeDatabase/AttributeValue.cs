using System;
using System.Collections.Generic;

namespace JoyConfig.Models.AttributeDatabase;

public partial class AttributeValue
{
    public int Id { get; set; }

    public string AttributeSetId { get; set; } = null!;

    public string AttributeType { get; set; } = null!;

    public string AttributeCategory => AttributeTypeNavigation?.Category ?? "N/A";

    public string? AttributeTypeComment => AttributeTypeNavigation?.Description;

    public double BaseValue { get; set; }

    public double MinValue { get; set; }

    public double MaxValue { get; set; }

    public virtual AttributeSet AttributeSet { get; set; } = null!;

    public virtual Attribute AttributeTypeNavigation { get; set; } = null!;
}
