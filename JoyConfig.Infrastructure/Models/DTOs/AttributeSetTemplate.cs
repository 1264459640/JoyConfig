using System.Collections.Generic;

namespace JoyConfig.Infrastructure.Models.DTOs;

public class AttributeSetTemplate
{
    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public List<AttributeValueTemplate> AttributeValues { get; set; } = new();
}

public class AttributeValueTemplate
{
    public string AttributeId { get; set; } = null!;
    
    public string AttributeCategory { get; set; } = null!;

    public double BaseValue { get; set; }

    public double MinValue { get; set; }

    public double MaxValue { get; set; }
    
    public string? Comment { get; set; }
}
