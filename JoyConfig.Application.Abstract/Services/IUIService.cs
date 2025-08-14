using System;
using JoyConfig.Infrastructure.Models.AttributeDatabase;
using Attribute = JoyConfig.Infrastructure.Models.AttributeDatabase.Attribute;

namespace JoyConfig.Application.Abstract.Services;

/// <summary>
/// UI服务接口 - 提供用户界面操作的抽象契�?/// 遵循依赖倒置原则，使业务逻辑层不依赖具体的UI实现
/// </summary>
public interface IUIService
{
    #region Editor Management
    
    /// <summary>
    /// 打开编辑�?    /// </summary>
    /// <param name="editorType">编辑器类�?/param>
    /// <param name="context">编辑上下�?/param>
    void OpenEditor(string editorType, object? context = null);
    
    /// <summary>
    /// 打开属性编辑器
    /// </summary>
    /// <param name="attribute">要编辑的属�?/param>
    void OpenAttributeEditor(Attribute attribute);
    
    /// <summary>
    /// 打开属性集编辑�?    /// </summary>
    /// <param name="attributeSet">要编辑的属性集</param>
    void OpenAttributeSetEditor(AttributeSet attributeSet);
    
    /// <summary>
    /// 打开模板管理�?    /// </summary>
    void OpenTemplateManager();
    
    /// <summary>
    /// 关闭当前编辑�?    /// </summary>
    void CloseCurrentEditor();
    
    #endregion
    
    #region Workspace Management
    
    /// <summary>
    /// 切换工作�?    /// </summary>
    /// <param name="workspaceType">工作区类�?/param>
    void SwitchWorkspace(string workspaceType);
    
    /// <summary>
    /// 打开属性数据库工作�?    /// </summary>
    void OpenAttributeDatabaseWorkspace();
    
    /// <summary>
    /// 打开游戏效果数据库工作区
    /// </summary>
    void OpenGameplayEffectDatabaseWorkspace();
    
    /// <summary>
    /// 打开设置页面
    /// </summary>
    void OpenSettings();
    
    /// <summary>
    /// 关闭设置页面
    /// </summary>
    void CloseSettings();
    
    #endregion
    
    #region Status and Messaging
    
    /// <summary>
    /// 设置状态消�?    /// </summary>
    /// <param name="message">状态消�?/param>
    void SetStatusMessage(string message);
    
    /// <summary>
    /// 清除状态消�?    /// </summary>
    void ClearStatusMessage();
    
    /// <summary>
    /// 显示通知消息
    /// </summary>
    /// <param name="message">通知消息</param>
    /// <param name="type">消息类型</param>
    void ShowNotification(string message, NotificationType type = NotificationType.Info);
    
    #endregion
    
    #region Navigation
    
    /// <summary>
    /// 导航到指定项�?    /// </summary>
    /// <param name="targetType">目标类型</param>
    /// <param name="targetId">目标ID</param>
    void NavigateTo(string targetType, string targetId);
    
    /// <summary>
    /// 导航到属�?    /// </summary>
    /// <param name="attributeId">属性ID</param>
    void NavigateToAttribute(string attributeId);
    
    /// <summary>
    /// 导航到属性集
    /// </summary>
    /// <param name="attributeSetId">属性集ID</param>
    void NavigateToAttributeSet(string attributeSetId);
    
    /// <summary>
    /// 返回上一�?    /// </summary>
    void GoBack();
    
    /// <summary>
    /// 前进到下一�?    /// </summary>
    void GoForward();
    
    #endregion
    
    #region Events
    
    /// <summary>
    /// 编辑器更改事�?    /// </summary>
    event Action<string, object?>? EditorChanged;
    
    /// <summary>
    /// 工作区更改事�?    /// </summary>
    event Action<string>? WorkspaceChanged;
    
    /// <summary>
    /// 状态消息更改事�?    /// </summary>
    event Action<string>? StatusMessageChanged;
    
    #endregion
}

/// <summary>
/// 通知消息类型
/// </summary>
public enum NotificationType
{
    /// <summary>
    /// 信息
    /// </summary>
    Info,
    
    /// <summary>
    /// 成功
    /// </summary>
    Success,
    
    /// <summary>
    /// 警告
    /// </summary>
    Warning,
    
    /// <summary>
    /// 错误
    /// </summary>
    Error
}
