using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace JoyConfig.Models.GameplayEffectDatabase;

/// <summary>
/// 游戏效果模板模型
/// </summary>
public class GameplayEffectTemplate
{
    /// <summary>
    /// 模板唯一标识符
    /// </summary>
    [Key]
    public string Id { get; set; } = string.Empty;
    
    /// <summary>
    /// 模板名称
    /// </summary>
    [Required]
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// 模板描述
    /// </summary>
    public string? Description { get; set; }
    
    /// <summary>
    /// 模板分类
    /// </summary>
    public string Category { get; set; } = "Default";
    
    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// 最后修改时间
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// 模板版本
    /// </summary>
    public string Version { get; set; } = "1.0.0";
    
    /// <summary>
    /// 创建者
    /// </summary>
    public string? Author { get; set; }
    
    /// <summary>
    /// 模板标签（用于搜索和分类）
    /// </summary>
    public string? Tags { get; set; }
    
    /// <summary>
    /// 效果数据（JSON格式存储）
    /// </summary>
    [Required]
    public string EffectData { get; set; } = string.Empty;
    
    /// <summary>
    /// 修改器数据（JSON格式存储）
    /// </summary>
    public string? ModifiersData { get; set; }
    
    /// <summary>
    /// 是否为系统内置模板
    /// </summary>
    public bool IsBuiltIn { get; set; } = false;
    
    /// <summary>
    /// 使用次数统计
    /// </summary>
    public int UsageCount { get; set; } = 0;
}

/// <summary>
/// 模板数据传输对象
/// </summary>
public class GameplayEffectTemplateData
{
    /// <summary>
    /// 效果基本信息
    /// </summary>
    public AttributeEffect Effect { get; set; } = new();
    
    /// <summary>
    /// 关联的修改器列表
    /// </summary>
    public List<AttributeModifier> Modifiers { get; set; } = new();
}

/// <summary>
/// 模板创建请求
/// </summary>
public class CreateTemplateRequest
{
    /// <summary>
    /// 模板名称
    /// </summary>
    [Required]
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// 模板描述
    /// </summary>
    public string? Description { get; set; }
    
    /// <summary>
    /// 模板分类
    /// </summary>
    public string Category { get; set; } = "Default";
    
    /// <summary>
    /// 创建者
    /// </summary>
    public string? Author { get; set; }
    
    /// <summary>
    /// 模板标签
    /// </summary>
    public string? Tags { get; set; }
    
    /// <summary>
    /// 源效果ID
    /// </summary>
    [Required]
    public string SourceEffectId { get; set; } = string.Empty;
}