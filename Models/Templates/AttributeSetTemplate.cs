using System;
using System.Collections.Generic;
using YamlDotNet.Serialization;

namespace JoyConfig.Models.Templates;

/// <summary>
/// 属性集模板数据模型
/// </summary>
public class AttributeSetTemplate
{
    /// <summary>
    /// 模板ID
    /// </summary>
    [YamlMember(Alias = "id")]
    public string Id { get; set; } = string.Empty;
    
    /// <summary>
    /// 模板名称
    /// </summary>
    [YamlMember(Alias = "name")]
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// 模板描述
    /// </summary>
    [YamlMember(Alias = "description")]
    public string Description { get; set; } = string.Empty;
    
    /// <summary>
    /// 创建时间
    /// </summary>
    [YamlMember(Alias = "created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    
    /// <summary>
    /// 最后修改时间
    /// </summary>
    [YamlMember(Alias = "updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.Now;
    
    /// <summary>
    /// 模板版本
    /// </summary>
    [YamlMember(Alias = "version")]
    public string Version { get; set; } = "1.0.0";
    
    /// <summary>
    /// 模板标签
    /// </summary>
    [YamlMember(Alias = "tags")]
    public List<string> Tags { get; set; } = new();
    
    /// <summary>
    /// 属性值列表
    /// </summary>
    [YamlMember(Alias = "attributes")]
    public List<AttributeValueTemplate> Attributes { get; set; } = new();
}

/// <summary>
/// 属性值模板数据模型
/// </summary>
public class AttributeValueTemplate
{
    /// <summary>
    /// 属性ID
    /// </summary>
    [YamlMember(Alias = "id")]
    public string Id { get; set; } = string.Empty;
    
    /// <summary>
    /// 属性分类
    /// </summary>
    [YamlMember(Alias = "category")]
    public string Category { get; set; } = string.Empty;
    
    /// <summary>
    /// 基础值
    /// </summary>
    [YamlMember(Alias = "base_value")]
    public double BaseValue { get; set; }
    
    /// <summary>
    /// 最小值
    /// </summary>
    [YamlMember(Alias = "min_value")]
    public double MinValue { get; set; }
    
    /// <summary>
    /// 最大值
    /// </summary>
    [YamlMember(Alias = "max_value")]
    public double MaxValue { get; set; }
    
    /// <summary>
    /// 注释
    /// </summary>
    [YamlMember(Alias = "comment")]
    public string? Comment { get; set; }
}