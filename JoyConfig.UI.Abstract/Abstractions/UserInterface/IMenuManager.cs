using System.Windows.Input;

namespace JoyConfig.UI.Abstract.Abstractions.UserInterface;

/// <summary>
/// 菜单管理抽象接口
/// 专注于菜单项的管理
/// </summary>
public interface IMenuManager
{
    /// <summary>
    /// 添加菜单项
    /// </summary>
    /// <param name="menuPath">菜单路径（如："File/New"）</param>
    /// <param name="command">菜单命令</param>
    /// <param name="icon">菜单图标</param>
    /// <param name="shortcut">快捷键</param>
    void AddMenuItem(string menuPath, ICommand command, object? icon = null, string? shortcut = null);
    
    /// <summary>
    /// 移除菜单项
    /// </summary>
    /// <param name="menuPath">菜单路径</param>
    void RemoveMenuItem(string menuPath);
    
    /// <summary>
    /// 添加菜单分隔符
    /// </summary>
    /// <param name="menuPath">菜单路径</param>
    void AddMenuSeparator(string menuPath);
    
    /// <summary>
    /// 清除菜单
    /// </summary>
    /// <param name="menuPath">菜单路径</param>
    void ClearMenu(string menuPath);
}

/// <summary>
/// 菜单状态管理接口
/// 专门负责菜单项的启用/禁用状态管理
/// </summary>
public interface IMenuStateManager
{
    /// <summary>
    /// 启用菜单项
    /// </summary>
    /// <param name="menuPath">菜单路径</param>
    void EnableMenuItem(string menuPath);
    
    /// <summary>
    /// 禁用菜单项
    /// </summary>
    /// <param name="menuPath">菜单路径</param>
    void DisableMenuItem(string menuPath);
    
    /// <summary>
    /// 检查菜单项是否启用
    /// </summary>
    /// <param name="menuPath">菜单路径</param>
    /// <returns>是否启用</returns>
    bool IsMenuItemEnabled(string menuPath);
}