using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JoyConfig.Models.GameplayEffectDatabase;
using Microsoft.EntityFrameworkCore;

namespace JoyConfig.Services;

/// <summary>
/// 数据验证服务实现
/// </summary>
public class ValidationService : IValidationService
{
    private readonly IDbContextFactory _dbContextFactory;
    private readonly IAttributeTypeService _attributeTypeService;
    
    public ValidationService(IDbContextFactory dbContextFactory, IAttributeTypeService attributeTypeService)
    {
        _dbContextFactory = dbContextFactory;
        _attributeTypeService = attributeTypeService;
    }
    
    /// <summary>
    /// 验证游戏效果数据
    /// </summary>
    public Task<ValidationResult> ValidateAttributeEffectAsync(AttributeEffect effect)
    {
        var result = new ValidationResult();
        
        // 验证ID
        var idValidation = ValidateStringLength(effect.Id, "ID", 1, 100, true);
        result.Merge(idValidation);
        
        if (idValidation.IsValid)
        {
            // 验证ID格式（只允许字母、数字、下划线、点）
            if (!System.Text.RegularExpressions.Regex.IsMatch(effect.Id, @"^[a-zA-Z0-9_.]+$"))
            {
                result.AddError("ID", "ID只能包含字母、数字、下划线和点", "INVALID_ID_FORMAT");
            }
        }
        
        // 验证名称
        var nameValidation = ValidateStringLength(effect.Name, "名称", 1, 200, true);
        result.Merge(nameValidation);
        
        // 验证描述长度
        if (!string.IsNullOrEmpty(effect.Description))
        {
            var descValidation = ValidateStringLength(effect.Description, "描述", 0, 1000);
            result.Merge(descValidation);
        }
        
        // 验证效果类型
        var effectTypeValidation = ValidateEnumValue(effect.EffectType, EffectTypes.All, "效果类型");
        result.Merge(effectTypeValidation);
        
        // 验证堆叠类型
        var stackingTypeValidation = ValidateEnumValue(effect.StackingType, StackingTypes.All, "堆叠类型");
        result.Merge(stackingTypeValidation);
        
        // 验证来源类型
        if (!string.IsNullOrEmpty(effect.SourceType))
        {
            var sourceTypeValidation = ValidateEnumValue(effect.SourceType, SourceTypes.All, "来源类型");
            result.Merge(sourceTypeValidation);
        }
        
        // 验证持续时间
        if (effect.DurationSeconds.HasValue)
        {
            var durationValidation = ValidateNumericRange(effect.DurationSeconds.Value, "持续时间", 0.1, 86400); // 最大24小时
            result.Merge(durationValidation);
            
            // 检查效果类型与持续时间的一致性
            if (effect.EffectType == EffectTypes.Instant && effect.DurationSeconds > 0)
            {
                result.AddWarning("持续时间", "即时效果通常不需要设置持续时间", "INSTANT_WITH_DURATION");
            }
        }
        
        // 验证最大堆叠数
        if (effect.MaxStacks.HasValue)
        {
            var maxStacksValidation = ValidateNumericRange((double)effect.MaxStacks.Value, "最大堆叠数", 1, 999);
            result.Merge(maxStacksValidation);
            
            // 检查堆叠类型与最大堆叠数的一致性
            if (effect.StackingType == StackingTypes.NoStack && effect.MaxStacks > 1)
            {
                result.AddWarning("最大堆叠数", "不堆叠类型的效果通常不需要设置最大堆叠数", "NO_STACK_WITH_MAX_STACKS");
            }
        }
        
        // 验证优先级
        if (effect.Priority.HasValue)
        {
            var priorityValidation = ValidateNumericRange((double)effect.Priority.Value, "优先级", -1000, 1000);
            result.Merge(priorityValidation);
        }
        
        // 验证周期间隔
        if (effect.IntervalSeconds.HasValue)
        {
            var intervalValidation = ValidateNumericRange(effect.IntervalSeconds.Value, "周期间隔", 0.1, 3600); // 最大1小时
            result.Merge(intervalValidation);
            
            // 检查是否为周期性效果
            if (effect.IsPeriodic != true)
            {
                result.AddWarning("周期间隔", "非周期性效果不需要设置周期间隔", "NON_PERIODIC_WITH_INTERVAL");
            }
        }
        
        // 验证标签格式
        if (!string.IsNullOrEmpty(effect.Tags))
        {
            var tags = effect.Tags.Split(',', StringSplitOptions.RemoveEmptyEntries);
            foreach (var tag in tags)
            {
                var trimmedTag = tag.Trim();
                if (string.IsNullOrEmpty(trimmedTag))
                {
                    result.AddError("标签", "标签不能为空", "EMPTY_TAG");
                }
                else if (trimmedTag.Length > 50)
                {
                    result.AddError("标签", $"标签 '{trimmedTag}' 长度不能超过50个字符", "TAG_TOO_LONG");
                }
            }
        }
        
        return Task.FromResult(result);
    }
    
    /// <summary>
    /// 验证属性修改器数据
    /// </summary>
    public async Task<ValidationResult> ValidateAttributeModifierAsync(AttributeModifier modifier)
    {
        var result = new ValidationResult();
        
        // 验证效果ID
        var effectIdValidation = ValidateStringLength(modifier.EffectId, "效果ID", 1, 100, true);
        result.Merge(effectIdValidation);
        
        // 验证属性类型
        var attributeTypeValidation = ValidateStringLength(modifier.AttributeType, "属性类型", 1, 100, true);
        result.Merge(attributeTypeValidation);
        
        if (attributeTypeValidation.IsValid)
        {
            // 验证属性类型是否存在
            var isValidAttributeType = await IsAttributeTypeValidAsync(modifier.AttributeType);
            if (!isValidAttributeType)
            {
                result.AddError("属性类型", $"属性类型 '{modifier.AttributeType}' 在属性数据库中不存在", "INVALID_ATTRIBUTE_TYPE");
            }
        }
        
        // 验证操作类型
        var operationTypeValidation = ValidateEnumValue(modifier.OperationType, OperationTypes.All, "操作类型");
        result.Merge(operationTypeValidation);
        
        // 验证数值
        var valueValidation = ValidateNumericRange(modifier.Value, "数值", -999999, 999999);
        result.Merge(valueValidation);
        
        // 根据操作类型验证数值的合理性
        if (operationTypeValidation.IsValid)
        {
            switch (modifier.OperationType)
            {
                case OperationTypes.Multiply:
                    if (modifier.Value < 0)
                    {
                        result.AddWarning("数值", "乘法修饰使用负数可能导致意外结果", "NEGATIVE_MULTIPLY");
                    }
                    break;
                    
                case OperationTypes.Percentage:
                    if (modifier.Value < -100)
                    {
                        result.AddWarning("数值", "百分比修饰小于-100%可能导致意外结果", "EXTREME_PERCENTAGE");
                    }
                    else if (modifier.Value > 1000)
                    {
                        result.AddWarning("数值", "百分比修饰大于1000%可能导致数值过大", "EXTREME_PERCENTAGE");
                    }
                    break;
            }
        }
        
        // 验证执行顺序
        var executionOrderValidation = ValidateNumericRange((double)(modifier.ExecutionOrder ?? 0), "执行顺序", -100, 100);
        result.Merge(executionOrderValidation);
        
        return result;
    }
    
    /// <summary>
    /// 验证效果ID的唯一性
    /// </summary>
    public async Task<bool> IsEffectIdUniqueAsync(string effectId, string? excludeId = null)
    {
        try
        {
            using var context = _dbContextFactory.CreateGameplayEffectDatabaseContext();
            
            var query = context.AttributeEffects.Where(e => e.Id == effectId);
            
            if (!string.IsNullOrEmpty(excludeId))
            {
                query = query.Where(e => e.Id != excludeId);
            }
            
            return !await query.AnyAsync();
        }
        catch
        {
            return false;
        }
    }
    
    /// <summary>
    /// 验证属性类型是否存在
    /// </summary>
    public Task<bool> IsAttributeTypeValidAsync(string attributeType)
    {
        try
        {
            return Task.FromResult(_attributeTypeService.IsValidAttributeType(attributeType));
        }
        catch
        {
            return Task.FromResult(false);
        }
    }
    
    /// <summary>
    /// 验证数值范围
    /// </summary>
    public ValidationResult ValidateNumericRange(double value, string fieldName, double? min = null, double? max = null)
    {
        var result = new ValidationResult();
        
        if (double.IsNaN(value) || double.IsInfinity(value))
        {
            result.AddError(fieldName, $"{fieldName}必须是有效的数值", "INVALID_NUMBER");
            return result;
        }
        
        if (min.HasValue && value < min.Value)
        {
            result.AddError(fieldName, $"{fieldName}不能小于{min.Value}", "VALUE_TOO_SMALL");
        }
        
        if (max.HasValue && value > max.Value)
        {
            result.AddError(fieldName, $"{fieldName}不能大于{max.Value}", "VALUE_TOO_LARGE");
        }
        
        return result;
    }
    
    /// <summary>
    /// 验证字符串长度
    /// </summary>
    public ValidationResult ValidateStringLength(string? value, string fieldName, int minLength = 0, int maxLength = int.MaxValue, bool required = false)
    {
        var result = new ValidationResult();
        
        if (required && string.IsNullOrWhiteSpace(value))
        {
            result.AddError(fieldName, $"{fieldName}不能为空", "REQUIRED_FIELD");
            return result;
        }
        
        if (!string.IsNullOrEmpty(value))
        {
            if (value.Length < minLength)
            {
                result.AddError(fieldName, $"{fieldName}长度不能少于{minLength}个字符", "STRING_TOO_SHORT");
            }
            
            if (value.Length > maxLength)
            {
                result.AddError(fieldName, $"{fieldName}长度不能超过{maxLength}个字符", "STRING_TOO_LONG");
            }
        }
        
        return result;
    }
    
    /// <summary>
    /// 验证枚举值
    /// </summary>
    public ValidationResult ValidateEnumValue(string? value, IEnumerable<string> validValues, string fieldName)
    {
        var result = new ValidationResult();
        
        if (string.IsNullOrWhiteSpace(value))
        {
            result.AddError(fieldName, $"{fieldName}不能为空", "REQUIRED_FIELD");
            return result;
        }
        
        if (!validValues.Contains(value))
        {
            var validValuesStr = string.Join(", ", validValues);
            result.AddError(fieldName, $"{fieldName}必须是以下值之一: {validValuesStr}", "INVALID_ENUM_VALUE");
        }
        
        return result;
    }
}