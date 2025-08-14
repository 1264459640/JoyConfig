using System;
using System.Collections.Generic;

namespace JoyConfig.Infrastructure.Models.AttributeDatabase;

public partial class AttributeSet
{
    public string Id { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public virtual ICollection<AttributeValue> AttributeValues { get; set; } = new List<AttributeValue>();
}
