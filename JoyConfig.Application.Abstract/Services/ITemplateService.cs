using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using JoyConfig.Infrastructure.Models.DTOs;
using JoyConfig.Infrastructure.Models.AttributeDatabase;
using AttributeSet = JoyConfig.Infrastructure.Models.AttributeDatabase.AttributeSet;

namespace JoyConfig.Application.Abstract.Services;

/// <summary>
/// 模板服务接口 - 提供模板管理功能的抽象契�?/// 遵循依赖倒置原则，使模板操作与具体实现解�?/// </summary>
public interface ITemplateService
{
    #region Template File Management
    
    /// <summary>
    /// 获取所有模板文�?    /// </summary>
    /// <returns>模板文件名列�?/returns>
    Task<List<string>> GetAllTemplateFilesAsync();
    
    /// <summary>
    /// 加载模板
    /// </summary>
    /// <param name="templateFileName">模板文件�?/param>
    /// <returns>模板对象</returns>
    Task<AttributeSetTemplate?> LoadTemplateAsync(string templateFileName);
    
    /// <summary>
    /// 保存模板
    /// </summary>
    /// <param name="template">模板对象</param>
    /// <param name="fileName">文件�?/param>
    /// <returns>保存任务</returns>
    Task SaveTemplateAsync(AttributeSetTemplate template, string fileName);
    
    /// <summary>
    /// 删除模板文件
    /// </summary>
    /// <param name="templateFileName">模板文件�?/param>
    /// <returns>删除任务</returns>
    Task DeleteTemplateAsync(string templateFileName);
    
    /// <summary>
    /// 重命名模板文�?    /// </summary>
    /// <param name="oldFileName">旧文件名</param>
    /// <param name="newFileName">新文件名</param>
    /// <returns>重命名任�?/returns>
    Task RenameTemplateAsync(string oldFileName, string newFileName);
    
    /// <summary>
    /// 复制模板文件
    /// </summary>
    /// <param name="sourceFileName">源文件名</param>
    /// <param name="targetFileName">目标文件�?/param>
    /// <returns>复制任务</returns>
    Task CopyTemplateAsync(string sourceFileName, string targetFileName);
    
    /// <summary>
    /// 检查模板文件是否存�?    /// </summary>
    /// <param name="templateFileName">模板文件�?/param>
    /// <returns>是否存在</returns>
    Task<bool> TemplateExistsAsync(string templateFileName);
    
    #endregion
    
    #region Template Operations
    
    /// <summary>
    /// 从属性集创建模板
    /// </summary>
    /// <param name="attributeSet">属性集</param>
    /// <param name="templateName">模板名称</param>
    /// <param name="description">模板描述</param>
    /// <returns>创建的模�?/returns>
    Task<AttributeSetTemplate> CreateTemplateFromAttributeSetAsync(AttributeSet attributeSet, string templateName, string? description = null);
    
    /// <summary>
    /// 应用模板到属性集
    /// </summary>
    /// <param name="template">模板对象</param>
    /// <param name="targetAttributeSetId">目标属性集ID</param>
    /// <returns>应用结果</returns>
    Task<bool> ApplyTemplateToAttributeSetAsync(AttributeSetTemplate template, string targetAttributeSetId);
    
    /// <summary>
    /// 从模板创建新属性集
    /// </summary>
    /// <param name="template">模板对象</param>
    /// <param name="newAttributeSetId">新属性集ID</param>
    /// <param name="newAttributeSetName">新属性集名称</param>
    /// <returns>创建的属性集</returns>
    Task<AttributeSet> CreateAttributeSetFromTemplateAsync(AttributeSetTemplate template, string newAttributeSetId, string newAttributeSetName);
    
    /// <summary>
    /// 验证模板格式
    /// </summary>
    /// <param name="template">模板对象</param>
    /// <returns>验证结果</returns>
    Task<TemplateValidationResult> ValidateTemplateAsync(AttributeSetTemplate template);
    
    /// <summary>
    /// 合并多个模板
    /// </summary>
    /// <param name="templates">模板列表</param>
    /// <param name="mergedTemplateName">合并后的模板名称</param>
    /// <returns>合并后的模板</returns>
    Task<AttributeSetTemplate> MergeTemplatesAsync(List<AttributeSetTemplate> templates, string mergedTemplateName);
    
    #endregion
    
    #region Template Search and Filter
    
    /// <summary>
    /// 搜索模板
    /// </summary>
    /// <param name="searchTerm">搜索关键�?/param>
    /// <returns>匹配的模板列�?/returns>
    Task<List<AttributeSetTemplate>> SearchTemplatesAsync(string searchTerm);
    
    /// <summary>
    /// 按标签筛选模�?    /// </summary>
    /// <param name="tags">标签列表</param>
    /// <returns>匹配的模板列�?/returns>
    Task<List<AttributeSetTemplate>> FilterTemplatesByTagsAsync(List<string> tags);
    
    /// <summary>
    /// 获取模板统计信息
    /// </summary>
    /// <param name="templateFileName">模板文件�?/param>
    /// <returns>统计信息</returns>
    Task<TemplateStatistics> GetTemplateStatisticsAsync(string templateFileName);
    
    #endregion
    
    #region Template Directory Management
    
    /// <summary>
    /// 获取模板目录路径
    /// </summary>
    /// <returns>模板目录路径</returns>
    string GetTemplateDirectory();
    
    /// <summary>
    /// 创建模板目录
    /// </summary>
    /// <returns>创建任务</returns>
    Task EnsureTemplateDirectoryExistsAsync();
    
    /// <summary>
    /// 清理无效的模板文�?    /// </summary>
    /// <returns>清理任务</returns>
    Task CleanupInvalidTemplatesAsync();
    
    /// <summary>
    /// 导入模板文件
    /// </summary>
    /// <param name="sourceFilePath">源文件路�?/param>
    /// <param name="targetFileName">目标文件�?/param>
    /// <returns>导入任务</returns>
    Task ImportTemplateAsync(string sourceFilePath, string? targetFileName = null);
    
    /// <summary>
    /// 导出模板文件
    /// </summary>
    /// <param name="templateFileName">模板文件�?/param>
    /// <param name="targetFilePath">目标文件路径</param>
    /// <returns>导出任务</returns>
    Task ExportTemplateAsync(string templateFileName, string targetFilePath);
    
    #endregion
    
    #region Events
    
    /// <summary>
    /// 模板创建事件
    /// </summary>
    event Action<string>? TemplateCreated;
    
    /// <summary>
    /// 模板删除事件
    /// </summary>
    event Action<string>? TemplateDeleted;
    
    /// <summary>
    /// 模板更新事件
    /// </summary>
    event Action<string>? TemplateUpdated;
    
    /// <summary>
    /// 模板应用事件
    /// </summary>
    event Action<string, string>? TemplateApplied;
    
    #endregion
}

/// <summary>
/// 模板验证结果
/// </summary>
public class TemplateValidationResult
{
    /// <summary>
    /// 是否有效
    /// </summary>
    public bool IsValid { get; set; }
    
    /// <summary>
    /// 错误消息列表
    /// </summary>
    public List<string> Errors { get; set; } = new();
    
    /// <summary>
    /// 警告消息列表
    /// </summary>
    public List<string> Warnings { get; set; } = new();
}

/// <summary>
/// 模板统计信息
/// </summary>
public class TemplateStatistics
{
    /// <summary>
    /// 属性数�?    /// </summary>
    public int AttributeCount { get; set; }
    
    /// <summary>
    /// 分类数量
    /// </summary>
    public int CategoryCount { get; set; }
    
    /// <summary>
    /// 文件大小（字节）
    /// </summary>
    public long FileSizeBytes { get; set; }
    
    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreatedAt { get; set; }
    
    /// <summary>
    /// 最后修改时�?    /// </summary>
    public DateTime LastModified { get; set; }
    
    /// <summary>
    /// 使用次数
    /// </summary>
    public int UsageCount { get; set; }
}
