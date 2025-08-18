using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using JoyConfig.Models.AttributeDatabase;
using Microsoft.EntityFrameworkCore;

namespace JoyConfig.Services;

/// <summary>
/// 属性类型服务实现
/// 提供全局的属性类型缓存和管理功能
/// </summary>
public partial class AttributeTypeService : ObservableObject, IAttributeTypeService
{
    private readonly IDbContextFactory _dbContextFactory;
    private readonly ObservableCollection<string> _attributeTypes = new();
    private readonly Dictionary<string, JoyConfig.Models.AttributeDatabase.Attribute> _attributeCache = new();

    [ObservableProperty]
    private bool _isInitialized;

    [ObservableProperty]
    private bool _isLoading;

    /// <summary>
    /// 所有可用的属性类型（只读集合）
    /// </summary>
    public ReadOnlyObservableCollection<string> AvailableAttributeTypes { get; }

    public AttributeTypeService(IDbContextFactory dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
        AvailableAttributeTypes = new ReadOnlyObservableCollection<string>(_attributeTypes);
    }

    /// <summary>
    /// 初始化属性类型缓存
    /// </summary>
    public async Task InitializeAsync()
    {
        if (IsInitialized || IsLoading)
            return;

        await LoadAttributeTypesAsync();
    }

    /// <summary>
    /// 刷新属性类型缓存
    /// </summary>
    public async Task RefreshAsync()
    {
        await LoadAttributeTypesAsync();
    }

    /// <summary>
    /// 检查属性类型是否存在
    /// </summary>
    public bool IsValidAttributeType(string attributeType)
    {
        return !string.IsNullOrWhiteSpace(attributeType) && _attributeCache.ContainsKey(attributeType);
    }

    /// <summary>
    /// 获取属性类型的详细信息
    /// </summary>
    public async Task<JoyConfig.Models.AttributeDatabase.Attribute?> GetAttributeInfoAsync(string attributeType)
    {
        if (string.IsNullOrWhiteSpace(attributeType))
            return null;

        // 先从缓存中查找
        if (_attributeCache.TryGetValue(attributeType, out var cachedAttribute))
            return cachedAttribute;

        // 如果缓存中没有，尝试从数据库加载
        try
        {
            using var context = _dbContextFactory.CreateAttributeDbContext();
            var attribute = await context.Attributes
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.Id == attributeType);

            if (attribute != null)
            {
                _attributeCache[attributeType] = attribute;
            }

            return attribute;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ERROR] 获取属性信息失败: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// 加载属性类型数据
    /// </summary>
    private async Task LoadAttributeTypesAsync()
    {
        try
        {
            IsLoading = true;

            using var context = _dbContextFactory.CreateAttributeDbContext();
            var attributes = await context.Attributes
                .AsNoTracking()
                .OrderBy(a => a.Id)
                .ToListAsync();

            // 更新UI线程中的集合
            await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
            {
                _attributeTypes.Clear();
                _attributeCache.Clear();

                foreach (var attribute in attributes)
                {
                    _attributeTypes.Add(attribute.Id);
                    _attributeCache[attribute.Id] = attribute;
                }

                // 如果没有找到属性，添加一些默认值作为后备
                if (_attributeTypes.Count == 0)
                {
                    var defaultTypes = new[] { "Health", "Mana", "Strength", "Agility", "Intelligence", "Damage", "Defense", "Speed" };
                    foreach (var type in defaultTypes)
                    {
                        _attributeTypes.Add(type);
                        _attributeCache[type] = new JoyConfig.Models.AttributeDatabase.Attribute { Id = type, Category = "Default", Description = $"Default {type} attribute" };
                    }
                }

                IsInitialized = true;
                System.Diagnostics.Debug.WriteLine($"[DEBUG] AttributeTypeService加载了 {_attributeTypes.Count} 个属性类型");
            });
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ERROR] AttributeTypeService加载失败: {ex.Message}");

            // 发生错误时使用默认属性类型
            await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
            {
                _attributeTypes.Clear();
                _attributeCache.Clear();

                var defaultTypes = new[] { "Health", "Mana", "Strength", "Agility", "Intelligence", "Damage", "Defense", "Speed" };
                foreach (var type in defaultTypes)
                {
                    _attributeTypes.Add(type);
                    _attributeCache[type] = new JoyConfig.Models.AttributeDatabase.Attribute { Id = type, Category = "Default", Description = $"Default {type} attribute" };
                }

                IsInitialized = true;
                System.Diagnostics.Debug.WriteLine($"[DEBUG] AttributeTypeService使用默认属性类型，共 {_attributeTypes.Count} 个");
            });
        }
        finally
        {
            IsLoading = false;
        }
    }
}