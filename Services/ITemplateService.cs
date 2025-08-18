using System.Collections.Generic;
using System.Threading.Tasks;
using JoyConfig.Models.Templates;

namespace JoyConfig.Services;

/// <summary>
/// 模板服务接口
/// </summary>
public interface ITemplateService
{
    /// <summary>
    /// 获取所有模板
    /// </summary>
    Task<List<AttributeSetTemplate>> GetAllTemplatesAsync();

    /// <summary>
    /// 根据ID获取模板
    /// </summary>
    Task<AttributeSetTemplate?> GetTemplateByIdAsync(string id);

    /// <summary>
    /// 创建新模板
    /// </summary>
    Task<AttributeSetTemplate> CreateTemplateAsync(AttributeSetTemplate template);

    /// <summary>
    /// 更新模板
    /// </summary>
    Task<AttributeSetTemplate> UpdateTemplateAsync(AttributeSetTemplate template);

    /// <summary>
    /// 删除模板
    /// </summary>
    Task<bool> DeleteTemplateAsync(string id);

    /// <summary>
    /// 检查模板ID是否存在
    /// </summary>
    Task<bool> TemplateExistsAsync(string id);

    /// <summary>
    /// 从属性集创建模板
    /// </summary>
    Task<AttributeSetTemplate> CreateTemplateFromAttributeSetAsync(string attributeSetId, string templateName, string? description = null);

    /// <summary>
    /// 根据模板创建属性集
    /// </summary>
    Task CreateAttributeSetFromTemplateAsync(string templateId, string newAttributeSetId, string newAttributeSetName);

    /// <summary>
    /// 获取模板文件路径
    /// </summary>
    string GetTemplateFilePath(string templateId);

    /// <summary>
    /// 获取模板目录路径
    /// </summary>
    string GetTemplatesDirectoryPath();
}