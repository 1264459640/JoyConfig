using System.Windows.Input;

namespace JoyConfig.UI.Abstract.Abstractions.UserInterface;

/// <summary>
/// 工具栏管理抽象接口
/// 专注于工具栏按钮的管理
/// </summary>
public interface IToolBarManager
{
    /// <summary>
    /// 添加工具栏按钮
    /// </summary>
    /// <param name="toolBarName">工具栏名称</param>
    /// <param name="buttonId">按钮ID</param>
    /// <param name="command">按钮命令</param>
    /// <param name="icon">按钮图标</param>
    /// <param name="tooltip">工具提示</param>
    void AddToolBarButton(string toolBarName, string buttonId, ICommand command, object? icon = null, string? tooltip = null);
    
    /// <summary>
    /// 移除工具栏按钮
    /// </summary>
    /// <param name="toolBarName">工具栏名称</param>
    /// <param name="buttonId">按钮ID</param>
    void RemoveToolBarButton(string toolBarName, string buttonId);
    
    /// <summary>
    /// 添加工具栏分隔符
    /// </summary>
    /// <param name="toolBarName">工具栏名称</param>
    void AddToolBarSeparator(string toolBarName);
}

/// <summary>
/// 工具栏状态管理接口
/// 专门负责工具栏按钮的状态和可见性管理
/// </summary>
public interface IToolBarStateManager
{
    /// <summary>
    /// 启用工具栏按钮
    /// </summary>
    /// <param name="toolBarName">工具栏名称</param>
    /// <param name="buttonId">按钮ID</param>
    void EnableToolBarButton(string toolBarName, string buttonId);
    
    /// <summary>
    /// 禁用工具栏按钮
    /// </summary>
    /// <param name="toolBarName">工具栏名称</param>
    /// <param name="buttonId">按钮ID</param>
    void DisableToolBarButton(string toolBarName, string buttonId);
    
    /// <summary>
    /// 显示工具栏
    /// </summary>
    /// <param name="toolBarName">工具栏名称</param>
    void ShowToolBar(string toolBarName);
    
    /// <summary>
    /// 隐藏工具栏
    /// </summary>
    /// <param name="toolBarName">工具栏名称</param>
    void HideToolBar(string toolBarName);
}