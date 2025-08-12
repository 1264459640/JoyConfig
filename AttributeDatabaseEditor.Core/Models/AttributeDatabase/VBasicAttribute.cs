using System;
using System.Collections.Generic;

namespace JoyConfig.Core.Models.AttributeDatabase;

public partial class VBasicAttribute
{
    public int? Id { get; set; }

    public string? AttributeSetId { get; set; }

    public string? AttributeType { get; set; }

    public string? AttributeCategory { get; set; }

    public string? AttributeTypeComment { get; set; }

    public double? BaseValue { get; set; }

    public double? MinValue { get; set; }

    public double? MaxValue { get; set; }
}
