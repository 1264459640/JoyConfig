using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Input;
using JoyConfig.Models.AttributeDatabase;
using JoyConfig.ViewModels.AttributeDatabase;

namespace JoyConfig.Services;

/// <summary>
/// 属性集数据访问接口
/// </summary>
public interface IAttributeSetRepository
{
    /// <summary>
    /// 获取所有属性集
    /// </summary>
    Task<List<AttributeSet>> GetAllAttributeSetsAsync();
    
    /// <summary>
    /// 根据ID获取属性集
    /// </summary>
    Task<AttributeSet?> GetAttributeSetByIdAsync(string id);
    
    /// <summary>
    /// 获取属性集的属性值视图模型列表
    /// </summary>
    Task<List<AttributeValueViewModel>> GetAttributeValueViewModelsAsync(string attributeSetId, ICommand? removeCommand = null);
    
    /// <summary>
    /// 创建新属性集
    /// </summary>
    Task<AttributeSet> CreateAttributeSetAsync(AttributeSet attributeSet);
    
    /// <summary>
    /// 更新属性集
    /// </summary>
    Task UpdateAttributeSetAsync(AttributeSet attributeSet);
    
    /// <summary>
    /// 删除属性集
    /// </summary>
    Task DeleteAttributeSetAsync(string attributeSetId);
    
    /// <summary>
    /// 保存属性集的属性值变更
    /// </summary>
    Task SaveAttributeSetChangesAsync(string attributeSetId, string name, string description);
    
    /// <summary>
    /// 修改属性集ID（包括所有相关的外键引用）
    /// </summary>
    Task UpdateAttributeSetIdAsync(string oldId, string newId);
    
    /// <summary>
    /// 添加属性值到属性集
    /// </summary>
    Task AddAttributeValueAsync(string attributeSetId, string attributeId);
    
    /// <summary>
    /// 从属性集移除属性值
    /// </summary>
    Task RemoveAttributeValueAsync(string attributeSetId, string attributeId);
    
    /// <summary>
    /// 更新属性值
    /// </summary>
    Task UpdateAttributeValueAsync(AttributeValue attributeValue);
    
    /// <summary>
    /// 检查属性集ID是否已存在
    /// </summary>
    Task<bool> AttributeSetExistsAsync(string id);
    
    /// <summary>
    /// 获取引用指定属性ID列表的属性集
    /// </summary>
    Task<List<AttributeSet>> GetReferencingAttributeSetsAsync(List<string> attributeIds);
}