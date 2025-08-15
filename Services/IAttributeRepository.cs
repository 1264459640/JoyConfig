using System.Collections.Generic;
using System.Threading.Tasks;
using JoyConfig.Models.AttributeDatabase;
using JoyConfig.Models.DTOs;

namespace JoyConfig.Services;

/// <summary>
/// 属性数据访问接口
/// </summary>
public interface IAttributeRepository
{
    /// <summary>
    /// 获取所有属性
    /// </summary>
    Task<List<Attribute>> GetAllAttributesAsync();
    
    /// <summary>
    /// 根据ID获取属性
    /// </summary>
    Task<Attribute?> GetAttributeByIdAsync(string id);
    
    /// <summary>
    /// 根据分类获取属性列表
    /// </summary>
    Task<List<Attribute>> GetAttributesByCategoryAsync(string category);
    
    /// <summary>
    /// 创建新属性
    /// </summary>
    Task<Attribute> CreateAttributeAsync(Attribute attribute);
    
    /// <summary>
    /// 更新属性
    /// </summary>
    Task UpdateAttributeAsync(Attribute attribute);
    
    /// <summary>
    /// 删除属性
    /// </summary>
    Task DeleteAttributeAsync(string attributeId);
    
    /// <summary>
    /// 预览属性变更影响
    /// </summary>
    Task<AttributeChangePreview> PreviewAttributeChangeAsync(string oldId, string newId, string oldCategory, string newCategory);
    
    /// <summary>
    /// 执行属性变更
    /// </summary>
    Task ExecuteAttributeChangeAsync(AttributeChangePreview preview);
    
    /// <summary>
    /// 删除分类
    /// </summary>
    Task DeleteCategoryAsync(string categoryName);
    
    /// <summary>
    /// 获取引用指定属性的属性集
    /// </summary>
    Task<List<AttributeSet>> GetReferencingAttributeSetsAsync(string attributeId);
    
    /// <summary>
    /// 获取引用指定属性列表的属性集
    /// </summary>
    Task<List<AttributeSet>> GetReferencingAttributeSetsAsync(List<string> attributeIds);
    
    /// <summary>
    /// 获取指定属性ID列表的属性值数量
    /// </summary>
    Task<int> GetAttributeValueCountAsync(List<string> attributeIds);
}