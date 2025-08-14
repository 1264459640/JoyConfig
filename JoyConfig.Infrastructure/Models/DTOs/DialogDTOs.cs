using JoyConfig.Infrastructure.Models.AttributeDatabase;
using Attribute = JoyConfig.Infrastructure.Models.AttributeDatabase.Attribute;

namespace JoyConfig.Infrastructure.Models.DTOs;

/// <summary>
/// 确认对话框数据传输对象
/// </summary>
public class ConfirmationDialogDto
{
    /// <summary>
    /// 对话框标题
    /// </summary>
    public string Title { get; set; } = string.Empty;
    
    /// <summary>
    /// 确认消息
    /// </summary>
    public string Message { get; set; } = string.Empty;
    
    /// <summary>
    /// 确认按钮文本
    /// </summary>
    public string ConfirmButtonText { get; set; } = "确定";
    
    /// <summary>
    /// 取消按钮文本
    /// </summary>
    public string CancelButtonText { get; set; } = "取消";
    
    /// <summary>
    /// 是否显示取消按钮
    /// </summary>
    public bool ShowCancelButton { get; set; } = true;
}

/// <summary>
/// 输入对话框数据传输对象
/// </summary>
public class InputDialogDto
{
    /// <summary>
    /// 对话框标题
    /// </summary>
    public string Title { get; set; } = string.Empty;
    
    /// <summary>
    /// 提示文本
    /// </summary>
    public string Prompt { get; set; } = string.Empty;
    
    /// <summary>
    /// 默认输入值
    /// </summary>
    public string DefaultValue { get; set; } = string.Empty;
    
    /// <summary>
    /// 占位符文本
    /// </summary>
    public string Placeholder { get; set; } = string.Empty;
    
    /// <summary>
    /// 是否为密码输入
    /// </summary>
    public bool IsPassword { get; set; } = false;
    
    /// <summary>
    /// 最大输入长度
    /// </summary>
    public int MaxLength { get; set; } = 255;
}

/// <summary>
/// 属性选择对话框数据传输对象
/// </summary>
public class SelectAttributeDialogDto
{
    /// <summary>
    /// 对话框标题
    /// </summary>
    public string Title { get; set; } = "选择属性";
    
    /// <summary>
    /// 可选属性列表
    /// </summary>
    public List<Attribute> AvailableAttributes { get; set; } = new();
    
    /// <summary>
    /// 过滤器文本
    /// </summary>
    public string FilterText { get; set; } = string.Empty;
    
    /// <summary>
    /// 是否允许多选
    /// </summary>
    public bool AllowMultipleSelection { get; set; } = false;
    
    /// <summary>
    /// 预选属性ID列表
    /// </summary>
    public List<string> PreselectedAttributeIds { get; set; } = new();
}

/// <summary>
/// 对话框结果基类
/// </summary>
public abstract class DialogResultBase
{
    /// <summary>
    /// 是否确认（点击了确定按钮）
    /// </summary>
    public bool IsConfirmed { get; set; }
    
    /// <summary>
    /// 是否取消（点击了取消按钮或关闭对话框）
    /// </summary>
    public bool IsCancelled => !IsConfirmed;
}

/// <summary>
/// 确认对话框结果
/// </summary>
public class ConfirmationDialogResult : DialogResultBase
{
    // 确认对话框只需要基类的IsConfirmed属性
}

/// <summary>
/// 输入对话框结果
/// </summary>
public class InputDialogResult : DialogResultBase
{
    /// <summary>
    /// 用户输入的文本
    /// </summary>
    public string InputText { get; set; } = string.Empty;
}

/// <summary>
/// 属性选择对话框结果
/// </summary>
public class SelectAttributeDialogResult : DialogResultBase
{
    /// <summary>
    /// 选中的属性列表
    /// </summary>
    public List<Attribute> SelectedAttributes { get; set; } = new();
    
    /// <summary>
    /// 选中的单个属性（用于单选模式）
    /// </summary>
    public Attribute? SelectedAttribute => SelectedAttributes.FirstOrDefault();
}