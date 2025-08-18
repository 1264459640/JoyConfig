using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using JoyConfig.Models.GameplayEffectDatabase;

namespace JoyConfig.Services;

/// <summary>
/// 数据验证服务接口
/// </summary>
public interface IValidationService
{
    /// <summary>
    /// 验证游戏效果数据
    /// </summary>
    /// <param name="effect">要验证的效果</param>
    /// <returns>验证结果</returns>
    Task<ValidationResult> ValidateAttributeEffectAsync(AttributeEffect effect);
    
    /// <summary>
    /// 验证属性修改器数据
    /// </summary>
    /// <param name="modifier">要验证的修改器</param>
    /// <returns>验证结果</returns>
    Task<ValidationResult> ValidateAttributeModifierAsync(AttributeModifier modifier);
    
    /// <summary>
    /// 验证效果ID的唯一性
    /// </summary>
    /// <param name="effectId">效果ID</param>
    /// <param name="excludeId">要排除的ID（用于更新时）</param>
    /// <returns>是否唯一</returns>
    Task<bool> IsEffectIdUniqueAsync(string effectId, string? excludeId = null);
    
    /// <summary>
    /// 验证属性类型是否存在
    /// </summary>
    /// <param name="attributeType">属性类型</param>
    /// <returns>是否存在</returns>
    Task<bool> IsAttributeTypeValidAsync(string attributeType);
    
    /// <summary>
    /// 验证数值范围
    /// </summary>
    /// <param name="value">数值</param>
    /// <param name="fieldName">字段名称</param>
    /// <param name="min">最小值</param>
    /// <param name="max">最大值</param>
    /// <returns>验证结果</returns>
    ValidationResult ValidateNumericRange(double value, string fieldName, double? min = null, double? max = null);
    
    /// <summary>
    /// 验证字符串长度
    /// </summary>
    /// <param name="value">字符串值</param>
    /// <param name="fieldName">字段名称</param>
    /// <param name="minLength">最小长度</param>
    /// <param name="maxLength">最大长度</param>
    /// <param name="required">是否必填</param>
    /// <returns>验证结果</returns>
    ValidationResult ValidateStringLength(string? value, string fieldName, int minLength = 0, int maxLength = int.MaxValue, bool required = false);
    
    /// <summary>
    /// 验证枚举值
    /// </summary>
    /// <param name="value">枚举值</param>
    /// <param name="validValues">有效值列表</param>
    /// <param name="fieldName">字段名称</param>
    /// <returns>验证结果</returns>
    ValidationResult ValidateEnumValue(string? value, IEnumerable<string> validValues, string fieldName);
}

/// <summary>
/// 验证结果
/// </summary>
public class ValidationResult
{
    /// <summary>
    /// 是否验证通过
    /// </summary>
    public bool IsValid { get; set; } = true;
    
    /// <summary>
    /// 错误信息列表
    /// </summary>
    public List<ValidationError> Errors { get; set; } = new();
    
    /// <summary>
    /// 警告信息列表
    /// </summary>
    public List<ValidationWarning> Warnings { get; set; } = new();
    
    /// <summary>
    /// 添加错误
    /// </summary>
    /// <param name="fieldName">字段名称</param>
    /// <param name="message">错误信息</param>
    /// <param name="errorCode">错误代码</param>
    public void AddError(string fieldName, string message, string? errorCode = null)
    {
        IsValid = false;
        Errors.Add(new ValidationError
        {
            FieldName = fieldName,
            Message = message,
            ErrorCode = errorCode
        });
    }
    
    /// <summary>
    /// 添加警告
    /// </summary>
    /// <param name="fieldName">字段名称</param>
    /// <param name="message">警告信息</param>
    /// <param name="warningCode">警告代码</param>
    public void AddWarning(string fieldName, string message, string? warningCode = null)
    {
        Warnings.Add(new ValidationWarning
        {
            FieldName = fieldName,
            Message = message,
            WarningCode = warningCode
        });
    }
    
    /// <summary>
    /// 合并其他验证结果
    /// </summary>
    /// <param name="other">其他验证结果</param>
    public void Merge(ValidationResult other)
    {
        if (!other.IsValid)
        {
            IsValid = false;
        }
        
        Errors.AddRange(other.Errors);
        Warnings.AddRange(other.Warnings);
    }
}

/// <summary>
/// 验证错误
/// </summary>
public class ValidationError
{
    /// <summary>
    /// 字段名称
    /// </summary>
    public string FieldName { get; set; } = string.Empty;
    
    /// <summary>
    /// 错误信息
    /// </summary>
    public string Message { get; set; } = string.Empty;
    
    /// <summary>
    /// 错误代码
    /// </summary>
    public string? ErrorCode { get; set; }
}

/// <summary>
/// 验证警告
/// </summary>
public class ValidationWarning
{
    /// <summary>
    /// 字段名称
    /// </summary>
    public string FieldName { get; set; } = string.Empty;
    
    /// <summary>
    /// 警告信息
    /// </summary>
    public string Message { get; set; } = string.Empty;
    
    /// <summary>
    /// 警告代码
    /// </summary>
    public string? WarningCode { get; set; }
}