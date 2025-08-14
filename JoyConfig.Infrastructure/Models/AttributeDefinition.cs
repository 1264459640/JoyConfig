using System.ComponentModel.DataAnnotations;

namespace JoyConfig.Infrastructure.Models;

public class AttributeDefinition
{
    [Key]
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public float DefaultValue { get; set; }
}
