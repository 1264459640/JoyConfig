using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using JoyConfig.Infrastructure.Models.AttributeDatabase;
using JoyConfig.Infrastructure.Models.DTOs;
using AttributeSet = JoyConfig.Infrastructure.Models.AttributeDatabase.AttributeSet;
using Attribute = JoyConfig.Infrastructure.Models.AttributeDatabase.Attribute;
using AttributeValue = JoyConfig.Infrastructure.Models.AttributeDatabase.AttributeValue;

namespace JoyConfig.Application.Abstract.Services;

/// <summary>
/// 数据访问抽象接口 - 提供统一的数据访问契�?/// 遵循依赖倒置原则，使上层不依赖具体的数据访问实现
/// </summary>
public interface IDataRepository
{
    #region AttributeSet Operations
    
    /// <summary>
    /// 获取所有属性集
    /// </summary>
    /// <returns>属性集列表</returns>
    Task<List<AttributeSet>> GetAllAttributeSetsAsync();
    
    /// <summary>
    /// 根据ID获取属性集（包含属性值）
    /// </summary>
    /// <param name="id">属性集ID</param>
    /// <returns>属性集实体</returns>
    Task<AttributeSet?> GetAttributeSetByIdAsync(string id);
    
    /// <summary>
    /// 创建新的属性集
    /// </summary>
    /// <param name="attributeSet">属性集实体</param>
    /// <returns>创建的属性集</returns>
    Task<AttributeSet> CreateAttributeSetAsync(AttributeSet attributeSet);
    
    /// <summary>
    /// 添加属性集（别名方法）
    /// </summary>
    /// <param name="attributeSet">属性集实体</param>
    /// <returns>添加的属性集</returns>
    Task<AttributeSet> AddAttributeSetAsync(AttributeSet attributeSet);
    
    /// <summary>
    /// 更新属性集
    /// </summary>
    /// <param name="attributeSet">属性集实体</param>
    /// <returns>更新后的属性集</returns>
    Task<AttributeSet> UpdateAttributeSetAsync(AttributeSet attributeSet);
    
    /// <summary>
    /// 删除属性集
    /// </summary>
    /// <param name="id">属性集ID</param>
    /// <returns>删除操作结果</returns>
    Task<bool> DeleteAttributeSetAsync(string id);
    
    #endregion
    
    #region Attribute Operations
    
    /// <summary>
    /// 获取所有属�?    /// </summary>
    /// <returns>属性列�?/returns>
    Task<List<Attribute>> GetAllAttributesAsync();
    
    /// <summary>
    /// 根据分类获取属�?    /// </summary>
    /// <param name="category">属性分�?/param>
    /// <returns>属性列�?/returns>
    Task<List<Attribute>> GetAttributesByCategoryAsync(string category);
    
    /// <summary>
    /// 根据ID获取属�?    /// </summary>
    /// <param name="id">属性ID</param>
    /// <returns>属性实�?/returns>
    Task<Attribute?> GetAttributeByIdAsync(string id);
    
    /// <summary>
    /// 创建新属�?    /// </summary>
    /// <param name="attribute">属性实�?/param>
    /// <returns>创建的属�?/returns>
    Task<Attribute> CreateAttributeAsync(Attribute attribute);
    
    /// <summary>
    /// 更新属�?    /// </summary>
    /// <param name="attribute">属性实�?/param>
    /// <returns>更新后的属�?/returns>
    Task<Attribute> UpdateAttributeAsync(Attribute attribute);
    
    /// <summary>
    /// 删除属�?    /// </summary>
    /// <param name="id">属性ID</param>
    /// <returns>删除操作结果</returns>
    Task<bool> DeleteAttributeAsync(string id);
    
    /// <summary>
    /// 获取引用指定属性的属性集
    /// </summary>
    /// <param name="attributeId">属性ID</param>
    /// <returns>引用该属性的属性集列表</returns>
    Task<List<AttributeSet>> GetReferencingAttributeSetsAsync(string attributeId);
    
    #endregion
    
    #region AttributeValue Operations
    
    /// <summary>
    /// 获取属性集的所有属性�?    /// </summary>
    /// <param name="attributeSetId">属性集ID</param>
    /// <returns>属性值列�?/returns>
    Task<List<AttributeValue>> GetAttributeValuesBySetIdAsync(string attributeSetId);
    
    /// <summary>
    /// 创建或更新属性�?    /// </summary>
    /// <param name="attributeValue">属性值实�?/param>
    /// <returns>创建或更新的属性�?/returns>
    Task<AttributeValue> SaveAttributeValueAsync(AttributeValue attributeValue);
    
    /// <summary>
    /// 删除属性�?    /// </summary>
    /// <param name="attributeSetId">属性集ID</param>
    /// <param name="attributeId">属性ID</param>
    /// <returns>删除操作结果</returns>
    Task<bool> DeleteAttributeValueAsync(string attributeSetId, string attributeId);
    
    #endregion
    
    #region Advanced Operations
    
    /// <summary>
    /// 预览属性更改影�?    /// </summary>
    /// <param name="attributeId">属性ID</param>
    /// <param name="newName">新属性名</param>
    /// <returns>更改预览信息</returns>
    Task<AttributeChangePreview> PreviewAttributeChangeAsync(string attributeId, string newName);
    
    /// <summary>
    /// 执行属性更�?    /// </summary>
    /// <param name="preview">更改预览信息</param>
    /// <returns>执行结果</returns>
    Task<bool> ExecuteAttributeChangeAsync(AttributeChangePreview preview);
    
    /// <summary>
    /// 获取所有属性分�?    /// </summary>
    /// <returns>分类列表</returns>
    Task<List<string>> GetAllCategoriesAsync();
    
    /// <summary>
    /// 搜索属�?    /// </summary>
    /// <param name="searchTerm">搜索关键�?/param>
    /// <returns>匹配的属性列�?/returns>
    Task<List<Attribute>> SearchAttributesAsync(string searchTerm);
    
    /// <summary>
    /// 检查属性ID是否存在
    /// </summary>
    /// <param name="attributeId">属性ID</param>
    /// <returns>是否存在</returns>
    Task<bool> AttributeExistsAsync(string attributeId);
    
    /// <summary>
    /// 检查属性集ID是否存在
    /// </summary>
    /// <param name="attributeSetId">属性集ID</param>
    /// <returns>是否存在</returns>
    Task<bool> AttributeSetExistsAsync(string attributeSetId);
    
    /// <summary>
    /// 获取属性值数�?    /// </summary>
    /// <param name="attributeIds">属性ID列表</param>
    /// <returns>属性值数�?/returns>
    Task<int> GetAttributeValuesCountAsync(List<string> attributeIds);
    
    /// <summary>
    /// 删除分类及其所有属�?    /// </summary>
    /// <param name="categoryName">分类名称</param>
    /// <returns>删除结果</returns>
    Task<bool> DeleteCategoryAsync(string categoryName);
    
    /// <summary>
    /// 获取引用指定属性ID列表的属性集
    /// </summary>
    /// <param name="attributeIds">属性ID列表</param>
    /// <returns>引用这些属性的属性集列表</returns>
    Task<List<AttributeSet>> GetReferencingAttributeSetsAsync(List<string> attributeIds);
    
    /// <summary>
    /// 预览属性更改影响（重载方法�?    /// </summary>
    /// <param name="oldId">旧属性ID</param>
    /// <param name="newId">新属性ID</param>
    /// <param name="category">属性分�?/param>
    /// <param name="newName">新属性名</param>
    /// <returns>更改预览信息</returns>
    Task<AttributeChangePreview> PreviewAttributeChangeAsync(string oldId, string newId, string category, string newName);
    
    #endregion
    
    #region Transaction Support
    
    /// <summary>
    /// 开始事�?    /// </summary>
    /// <returns>事务上下�?/returns>
    Task<IDisposable> BeginTransactionAsync();
    
    /// <summary>
    /// 保存更改
    /// </summary>
    /// <returns>保存结果</returns>
    Task<int> SaveChangesAsync();
    
    #endregion
}
