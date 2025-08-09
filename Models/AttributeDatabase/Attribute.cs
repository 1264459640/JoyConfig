using System;
using System.Collections.Generic;

namespace JoyConfig.Models.AttributeDatabase;

public partial class Attribute
{
    public string Id { get; set; } = null!;

    public string Category { get; set; } = null!;

    public string? Description { get; set; }

    public virtual ICollection<AttributeValue> AttributeValues { get; set; } = new List<AttributeValue>();
}
