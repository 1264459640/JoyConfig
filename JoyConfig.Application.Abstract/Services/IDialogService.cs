using System.Threading.Tasks;
using Avalonia.Platform.Storage;
using JoyConfig.Infrastructure.Models.DTOs;
using Attribute = JoyConfig.Infrastructure.Models.AttributeDatabase.Attribute;

namespace JoyConfig.Application.Abstract.Services;

/// <summary>
/// 对话框服务接口 - 提供各种对话框操作的抽象契约
/// 使用DTOs而非ViewModels，遵循分层架构原则
/// </summary>
public interface IDialogService
{
    /// <summary>
    /// 显示确认对话框
    /// </summary>
    /// <param name="dialogData">对话框数据</param>
    /// <returns>对话框结果</returns>
    Task<ConfirmationDialogResult> ShowConfirmationDialogAsync(ConfirmationDialogDto dialogData);
    
    /// <summary>
    /// 显示输入对话框
    /// </summary>
    /// <param name="dialogData">对话框数据</param>
    /// <returns>对话框结果</returns>
    Task<InputDialogResult> ShowInputDialogAsync(InputDialogDto dialogData);
    
    /// <summary>
    /// 显示属性选择对话框
    /// </summary>
    /// <param name="dialogData">对话框数据</param>
    /// <returns>对话框结果</returns>
    Task<SelectAttributeDialogResult> ShowSelectAttributeDialogAsync(SelectAttributeDialogDto dialogData);
    
    /// <summary>
    /// 显示模板选择对话框
    /// </summary>
    /// <returns>选中的模板名称，如果取消则返回null</returns>
    Task<string?> ShowSelectTemplateDialogAsync();
    
    /// <summary>
    /// 显示文件打开对话框
    /// </summary>
    /// <param name="title">对话框标题</param>
    /// <param name="filter">文件类型过滤器</param>
    /// <returns>选中的文件路径，如果取消则返回null</returns>
    Task<string?> ShowOpenFileDialogAsync(string title, FilePickerFileType filter);
    
    /// <summary>
    /// 显示消息框
    /// </summary>
    /// <param name="title">标题</param>
    /// <param name="message">消息内容</param>
    Task ShowMessageBoxAsync(string title, string message);
}
