using System;
using System.Collections.Generic;

namespace JoyConfig.Models.AttributeDatabase;

using System.ComponentModel.DataAnnotations.Schema;

public partial class AttributeValue
{
    public int Id { get; set; }

    public string AttributeSetId { get; set; } = null!;

    // This property maps to the 'AttributeType' column in the database.
    [Column("AttributeType")]
    public string AttributeId { get; set; } = null!;
    
    // This property maps to the 'AttributeCategory' column in the database.
    [Column("AttributeCategory")]
    public string AttributeCategory { get; set; } = null!;

    public double BaseValue { get; set; }

    public double MinValue { get; set; }

    public double MaxValue { get; set; }
    
    // This property maps to the 'AttributeTypeComment' column in the database.
    [Column("AttributeTypeComment")]
    public string? Comment { get; set; }

    public virtual AttributeSet AttributeSet { get; set; } = null!;

    public virtual Attribute Attribute { get; set; } = null!;
}
