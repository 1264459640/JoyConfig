using System.Collections.Generic;
using Attribute = JoyConfig.Infrastructure.Models.AttributeDatabase.Attribute;
using AttributeSet = JoyConfig.Infrastructure.Models.AttributeDatabase.AttributeSet;

namespace JoyConfig.Infrastructure.Models.DTOs;

public class AttributeChangePreview
{
    public string OldId { get; set; } = "";
    public string NewId { get; set; } = "";
    public string OldCategory { get; set; } = "";
    public string NewCategory { get; set; } = "";
    public bool IsValid { get; set; }
    public string? ErrorMessage { get; set; }
    public List<Attribute> AffectedAttributes { get; set; } = new();
    public List<AttributeSet> AffectedAttributeSets { get; set; } = new();
    public int AffectedValueCount { get; set; }
}
