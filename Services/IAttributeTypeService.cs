using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;

namespace JoyConfig.Services;

/// <summary>
/// 属性类型服务接口
/// 提供全局的属性类型缓存和管理功能
/// </summary>
public interface IAttributeTypeService : INotifyPropertyChanged
{
    /// <summary>
    /// 所有可用的属性类型（只读集合）
    /// </summary>
    ReadOnlyObservableCollection<string> AvailableAttributeTypes { get; }

    /// <summary>
    /// 是否已初始化
    /// </summary>
    bool IsInitialized { get; }

    /// <summary>
    /// 是否正在加载
    /// </summary>
    bool IsLoading { get; }

    /// <summary>
    /// 初始化属性类型缓存
    /// </summary>
    Task InitializeAsync();

    /// <summary>
    /// 刷新属性类型缓存
    /// </summary>
    Task RefreshAsync();

    /// <summary>
    /// 检查属性类型是否存在
    /// </summary>
    /// <param name="attributeType">属性类型ID</param>
    /// <returns>是否存在</returns>
    bool IsValidAttributeType(string attributeType);

    /// <summary>
    /// 获取属性类型的详细信息
    /// </summary>
    /// <param name="attributeType">属性类型ID</param>
    /// <returns>属性信息，如果不存在则返回null</returns>
    Task<Models.AttributeDatabase.Attribute?> GetAttributeInfoAsync(string attributeType);
}