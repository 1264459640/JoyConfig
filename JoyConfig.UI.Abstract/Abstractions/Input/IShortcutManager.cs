using System.Collections.Generic;
using System.Windows.Input;

namespace JoyConfig.UI.Abstract.Abstractions.Input;

/// <summary>
/// 快捷键注册管理接口
/// 专注于快捷键的注册和取消注册
/// </summary>
public interface IShortcutRegistrationManager
{
    /// <summary>
    /// 注册快捷键
    /// </summary>
    /// <param name="shortcut">快捷键组合</param>
    /// <param name="command">关联命令</param>
    /// <param name="description">快捷键描述</param>
    void RegisterShortcut(string shortcut, ICommand command, string? description = null);
    
    /// <summary>
    /// 取消注册快捷键
    /// </summary>
    /// <param name="shortcut">快捷键组合</param>
    void UnregisterShortcut(string shortcut);
    
    /// <summary>
    /// 检查快捷键是否已注册
    /// </summary>
    /// <param name="shortcut">快捷键组合</param>
    /// <returns>是否已注册</returns>
    bool IsShortcutRegistered(string shortcut);
}

/// <summary>
/// 快捷键查询管理接口
/// 专门负责快捷键信息的查询
/// </summary>
public interface IShortcutQueryManager
{
    /// <summary>
    /// 获取所有已注册的快捷键
    /// </summary>
    /// <returns>快捷键字典</returns>
    Dictionary<string, (ICommand Command, string? Description)> GetRegisteredShortcuts();
    
    /// <summary>
    /// 根据命令获取快捷键
    /// </summary>
    /// <param name="command">命令</param>
    /// <returns>快捷键列表</returns>
    IEnumerable<string> GetShortcutsForCommand(ICommand command);
}

/// <summary>
/// 快捷键状态管理接口
/// 专门负责快捷键的启用/禁用状态管理
/// </summary>
public interface IShortcutStateManager
{
    /// <summary>
    /// 启用快捷键
    /// </summary>
    /// <param name="shortcut">快捷键组合</param>
    void EnableShortcut(string shortcut);
    
    /// <summary>
    /// 禁用快捷键
    /// </summary>
    /// <param name="shortcut">快捷键组合</param>
    void DisableShortcut(string shortcut);
    
    /// <summary>
    /// 检查快捷键是否启用
    /// </summary>
    /// <param name="shortcut">快捷键组合</param>
    /// <returns>是否启用</returns>
    bool IsShortcutEnabled(string shortcut);
}