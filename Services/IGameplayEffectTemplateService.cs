using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using JoyConfig.Models.GameplayEffectDatabase;

namespace JoyConfig.Services;

/// <summary>
/// 游戏效果模板服务接口
/// </summary>
public interface IGameplayEffectTemplateService
{
    /// <summary>
    /// 获取所有模板
    /// </summary>
    /// <returns>模板列表</returns>
    Task<List<GameplayEffectTemplate>> GetAllTemplatesAsync();

    /// <summary>
    /// 根据ID获取模板
    /// </summary>
    /// <param name="templateId">模板ID</param>
    /// <returns>模板对象，如果不存在则返回null</returns>
    Task<GameplayEffectTemplate?> GetTemplateByIdAsync(string templateId);

    /// <summary>
    /// 根据分类获取模板
    /// </summary>
    /// <param name="category">分类名称</param>
    /// <returns>模板列表</returns>
    Task<List<GameplayEffectTemplate>> GetTemplatesByCategoryAsync(string category);

    /// <summary>
    /// 搜索模板
    /// </summary>
    /// <param name="searchTerm">搜索关键词</param>
    /// <returns>匹配的模板列表</returns>
    Task<List<GameplayEffectTemplate>> SearchTemplatesAsync(string searchTerm);

    /// <summary>
    /// 创建新模板
    /// </summary>
    /// <param name="request">创建请求</param>
    /// <returns>创建的模板</returns>
    Task<GameplayEffectTemplate> CreateTemplateAsync(CreateTemplateRequest request);

    /// <summary>
    /// 从现有效果创建模板
    /// </summary>
    /// <param name="effectId">效果ID</param>
    /// <param name="templateName">模板名称</param>
    /// <param name="description">模板描述</param>
    /// <param name="category">模板分类</param>
    /// <param name="author">创建者</param>
    /// <returns>创建的模板</returns>
    Task<GameplayEffectTemplate> CreateTemplateFromEffectAsync(
        string effectId,
        string templateName,
        string? description = null,
        string category = "Default",
        string? author = null);

    /// <summary>
    /// 更新模板
    /// </summary>
    /// <param name="template">要更新的模板</param>
    /// <returns>更新后的模板</returns>
    Task<GameplayEffectTemplate> UpdateTemplateAsync(GameplayEffectTemplate template);

    /// <summary>
    /// 删除模板
    /// </summary>
    /// <param name="templateId">模板ID</param>
    /// <returns>是否删除成功</returns>
    Task<bool> DeleteTemplateAsync(string templateId);

    /// <summary>
    /// 从模板创建效果
    /// </summary>
    /// <param name="templateId">模板ID</param>
    /// <param name="newEffectId">新效果ID</param>
    /// <param name="newEffectName">新效果名称</param>
    /// <returns>创建的效果</returns>
    Task<AttributeEffect> CreateEffectFromTemplateAsync(string templateId, string newEffectId, string newEffectName);

    /// <summary>
    /// 获取模板数据（包含效果和修改器）
    /// </summary>
    /// <param name="templateId">模板ID</param>
    /// <returns>模板数据</returns>
    Task<GameplayEffectTemplateData?> GetTemplateDataAsync(string templateId);

    /// <summary>
    /// 增加模板使用次数
    /// </summary>
    /// <param name="templateId">模板ID</param>
    /// <returns>更新后的使用次数</returns>
    Task<int> IncrementUsageCountAsync(string templateId);

    /// <summary>
    /// 获取所有模板分类
    /// </summary>
    /// <returns>分类列表</returns>
    Task<List<string>> GetAllCategoriesAsync();

    /// <summary>
    /// 验证模板数据的有效性
    /// </summary>
    /// <param name="templateData">模板数据</param>
    /// <returns>验证结果和错误信息</returns>
    Task<(bool IsValid, List<string> Errors)> ValidateTemplateDataAsync(GameplayEffectTemplateData templateData);
}