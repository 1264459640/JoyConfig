using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace JoyConfig.UI.Abstract.Abstractions.Theming;

/// <summary>
/// 主题管理抽象接口
/// 专注于主题的应用和管理
/// </summary>
public interface IThemeManager
{
    /// <summary>
    /// 当前主题名称
    /// </summary>
    string CurrentTheme { get; }
    
    /// <summary>
    /// 可用主题列表
    /// </summary>
    IReadOnlyList<string> AvailableThemes { get; }
    
    /// <summary>
    /// 应用主题
    /// </summary>
    /// <param name="themeName">主题名称</param>
    /// <returns>应用任务</returns>
    Task ApplyThemeAsync(string themeName);
    
    /// <summary>
    /// 主题变更事件
    /// </summary>
    event EventHandler<string>? ThemeChanged;
}

/// <summary>
/// 主题资源管理接口
/// 专门负责主题资源的获取和注册
/// </summary>
public interface IThemeResourceManager
{
    /// <summary>
    /// 获取主题资源
    /// </summary>
    /// <param name="themeName">主题名称</param>
    /// <param name="resourceKey">资源键</param>
    /// <returns>资源值</returns>
    object? GetThemeResource(string themeName, string resourceKey);
    
    /// <summary>
    /// 注册主题
    /// </summary>
    /// <param name="themeName">主题名称</param>
    /// <param name="themeResourcePath">主题资源路径</param>
    void RegisterTheme(string themeName, string themeResourcePath);
    
    /// <summary>
    /// 取消注册主题
    /// </summary>
    /// <param name="themeName">主题名称</param>
    void UnregisterTheme(string themeName);
}