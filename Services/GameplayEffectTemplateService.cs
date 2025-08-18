using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using JoyConfig.Models.GameplayEffectDatabase;
using Microsoft.EntityFrameworkCore;

namespace JoyConfig.Services;

/// <summary>
/// 游戏效果模板服务实现
/// </summary>
public class GameplayEffectTemplateService : IGameplayEffectTemplateService
{
    private readonly IDbContextFactory _dbContextFactory;
    private readonly JsonSerializerOptions _jsonOptions;

    public GameplayEffectTemplateService(IDbContextFactory dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        };
    }

    /// <summary>
    /// 获取所有模板
    /// </summary>
    public async Task<List<GameplayEffectTemplate>> GetAllTemplatesAsync()
    {
        using var context = _dbContextFactory.CreateGameplayEffectDatabaseContext();
        return await context.GameplayEffectTemplates
            .OrderBy(t => t.Category)
            .ThenBy(t => t.Name)
            .ToListAsync();
    }

    /// <summary>
    /// 根据ID获取模板
    /// </summary>
    public async Task<GameplayEffectTemplate?> GetTemplateByIdAsync(string templateId)
    {
        using var context = _dbContextFactory.CreateGameplayEffectDatabaseContext();
        return await context.GameplayEffectTemplates
            .FirstOrDefaultAsync(t => t.Id == templateId);
    }

    /// <summary>
    /// 根据分类获取模板
    /// </summary>
    public async Task<List<GameplayEffectTemplate>> GetTemplatesByCategoryAsync(string category)
    {
        using var context = _dbContextFactory.CreateGameplayEffectDatabaseContext();
        return await context.GameplayEffectTemplates
            .Where(t => t.Category == category)
            .OrderBy(t => t.Name)
            .ToListAsync();
    }

    /// <summary>
    /// 搜索模板
    /// </summary>
    public async Task<List<GameplayEffectTemplate>> SearchTemplatesAsync(string searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
            return await GetAllTemplatesAsync();

        using var context = _dbContextFactory.CreateGameplayEffectDatabaseContext();
        var term = searchTerm.ToLower();

        return await context.GameplayEffectTemplates
            .Where(t => t.Name.ToLower().Contains(term) ||
                       (t.Description != null && t.Description.ToLower().Contains(term)) ||
                       (t.Tags != null && t.Tags.ToLower().Contains(term)) ||
                       t.Category.ToLower().Contains(term))
            .OrderBy(t => t.Category)
            .ThenBy(t => t.Name)
            .ToListAsync();
    }

    /// <summary>
    /// 创建新模板
    /// </summary>
    public async Task<GameplayEffectTemplate> CreateTemplateAsync(CreateTemplateRequest request)
    {
        return await CreateTemplateFromEffectAsync(
            request.SourceEffectId,
            request.Name,
            request.Description,
            request.Category,
            request.Author);
    }

    /// <summary>
    /// 从现有效果创建模板
    /// </summary>
    public async Task<GameplayEffectTemplate> CreateTemplateFromEffectAsync(
        string effectId,
        string templateName,
        string? description = null,
        string category = "Default",
        string? author = null)
    {
        using var context = _dbContextFactory.CreateGameplayEffectDatabaseContext();

        // 获取源效果和其修改器
        var sourceEffect = await context.AttributeEffects
            .Include(e => e.AttributeModifiers)
            .FirstOrDefaultAsync(e => e.Id == effectId);

        if (sourceEffect == null)
            throw new ArgumentException($"效果 '{effectId}' 不存在", nameof(effectId));

        // 创建模板数据
        var templateData = new GameplayEffectTemplateData
        {
            Effect = new AttributeEffect
            {
                Id = "", // 模板中不保存具体ID
                Name = sourceEffect.Name,
                Description = sourceEffect.Description,
                EffectType = sourceEffect.EffectType,
                StackingType = sourceEffect.StackingType,
                Tags = sourceEffect.Tags,
                DurationSeconds = sourceEffect.DurationSeconds,
                IsInfinite = sourceEffect.IsInfinite,
                MaxStacks = sourceEffect.MaxStacks,
                IsPassive = sourceEffect.IsPassive,
                Priority = sourceEffect.Priority,
                IsPeriodic = sourceEffect.IsPeriodic,
                IntervalSeconds = sourceEffect.IntervalSeconds,
                SourceType = sourceEffect.SourceType
            },
            Modifiers = sourceEffect.AttributeModifiers.Select(m => new AttributeModifier
            {
                Id = 0, // 模板中不保存具体ID
                EffectId = "", // 模板中不保存具体EffectId
                AttributeType = m.AttributeType,
                OperationType = m.OperationType,
                Value = m.Value,
                ExecutionOrder = m.ExecutionOrder
            }).ToList()
        };

        // 序列化数据
        var effectJson = JsonSerializer.Serialize(templateData.Effect, _jsonOptions);
        var modifiersJson = JsonSerializer.Serialize(templateData.Modifiers, _jsonOptions);

        // 创建模板
        var template = new GameplayEffectTemplate
        {
            Id = Guid.NewGuid().ToString(),
            Name = templateName,
            Description = description,
            Category = category,
            Author = author,
            Tags = sourceEffect.Tags,
            EffectData = effectJson,
            ModifiersData = modifiersJson,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        context.GameplayEffectTemplates.Add(template);
        await context.SaveChangesAsync();

        return template;
    }

    /// <summary>
    /// 更新模板
    /// </summary>
    public async Task<GameplayEffectTemplate> UpdateTemplateAsync(GameplayEffectTemplate template)
    {
        using var context = _dbContextFactory.CreateGameplayEffectDatabaseContext();

        var existingTemplate = await context.GameplayEffectTemplates
            .FirstOrDefaultAsync(t => t.Id == template.Id);

        if (existingTemplate == null)
            throw new ArgumentException($"模板 '{template.Id}' 不存在", nameof(template));

        // 更新字段
        existingTemplate.Name = template.Name;
        existingTemplate.Description = template.Description;
        existingTemplate.Category = template.Category;
        existingTemplate.Author = template.Author;
        existingTemplate.Tags = template.Tags;
        existingTemplate.Version = template.Version;
        existingTemplate.UpdatedAt = DateTime.UtcNow;

        await context.SaveChangesAsync();
        return existingTemplate;
    }

    /// <summary>
    /// 删除模板
    /// </summary>
    public async Task<bool> DeleteTemplateAsync(string templateId)
    {
        using var context = _dbContextFactory.CreateGameplayEffectDatabaseContext();

        var template = await context.GameplayEffectTemplates
            .FirstOrDefaultAsync(t => t.Id == templateId);

        if (template == null)
            return false;

        // 检查是否为内置模板
        if (template.IsBuiltIn)
            throw new InvalidOperationException("不能删除系统内置模板");

        context.GameplayEffectTemplates.Remove(template);
        await context.SaveChangesAsync();

        return true;
    }

    /// <summary>
    /// 从模板创建效果
    /// </summary>
    public async Task<AttributeEffect> CreateEffectFromTemplateAsync(string templateId, string newEffectId, string newEffectName)
    {
        using var context = _dbContextFactory.CreateGameplayEffectDatabaseContext();

        var template = await GetTemplateByIdAsync(templateId);
        if (template == null)
            throw new ArgumentException($"模板 '{templateId}' 不存在", nameof(templateId));

        // 检查新效果ID是否已存在
        var existingEffect = await context.AttributeEffects
            .FirstOrDefaultAsync(e => e.Id == newEffectId);
        if (existingEffect != null)
            throw new ArgumentException($"效果ID '{newEffectId}' 已存在", nameof(newEffectId));

        // 反序列化模板数据
        var templateData = await GetTemplateDataAsync(templateId);
        if (templateData == null)
            throw new InvalidOperationException($"无法解析模板 '{templateId}' 的数据");

        // 创建新效果
        var newEffect = new AttributeEffect
        {
            Id = newEffectId,
            Name = newEffectName,
            Description = templateData.Effect.Description,
            EffectType = templateData.Effect.EffectType,
            StackingType = templateData.Effect.StackingType,
            Tags = templateData.Effect.Tags,
            DurationSeconds = templateData.Effect.DurationSeconds,
            IsInfinite = templateData.Effect.IsInfinite,
            MaxStacks = templateData.Effect.MaxStacks,
            IsPassive = templateData.Effect.IsPassive,
            Priority = templateData.Effect.Priority,
            IsPeriodic = templateData.Effect.IsPeriodic,
            IntervalSeconds = templateData.Effect.IntervalSeconds,
            SourceType = templateData.Effect.SourceType
        };

        // 创建修改器
        foreach (var modifierTemplate in templateData.Modifiers)
        {
            var newModifier = new AttributeModifier
            {
                EffectId = newEffectId,
                AttributeType = modifierTemplate.AttributeType,
                OperationType = modifierTemplate.OperationType,
                Value = modifierTemplate.Value,
                ExecutionOrder = modifierTemplate.ExecutionOrder
            };
            newEffect.AttributeModifiers.Add(newModifier);
        }

        context.AttributeEffects.Add(newEffect);

        // 增加模板使用次数
        await IncrementUsageCountAsync(templateId);

        await context.SaveChangesAsync();

        return newEffect;
    }

    /// <summary>
    /// 获取模板数据
    /// </summary>
    public async Task<GameplayEffectTemplateData?> GetTemplateDataAsync(string templateId)
    {
        var template = await GetTemplateByIdAsync(templateId);
        if (template == null)
            return null;

        try
        {
            var effect = JsonSerializer.Deserialize<AttributeEffect>(template.EffectData, _jsonOptions);
            var modifiers = string.IsNullOrEmpty(template.ModifiersData)
                ? new List<AttributeModifier>()
                : JsonSerializer.Deserialize<List<AttributeModifier>>(template.ModifiersData, _jsonOptions);

            return new GameplayEffectTemplateData
            {
                Effect = effect ?? new AttributeEffect(),
                Modifiers = modifiers ?? new List<AttributeModifier>()
            };
        }
        catch (JsonException ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ERROR] 解析模板数据失败: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// 增加模板使用次数
    /// </summary>
    public async Task<int> IncrementUsageCountAsync(string templateId)
    {
        using var context = _dbContextFactory.CreateGameplayEffectDatabaseContext();

        var template = await context.GameplayEffectTemplates
            .FirstOrDefaultAsync(t => t.Id == templateId);

        if (template != null)
        {
            template.UsageCount++;
            template.UpdatedAt = DateTime.UtcNow;
            await context.SaveChangesAsync();
            return template.UsageCount;
        }

        return 0;
    }

    /// <summary>
    /// 获取所有模板分类
    /// </summary>
    public async Task<List<string>> GetAllCategoriesAsync()
    {
        using var context = _dbContextFactory.CreateGameplayEffectDatabaseContext();
        return await context.GameplayEffectTemplates
            .Select(t => t.Category)
            .Distinct()
            .OrderBy(c => c)
            .ToListAsync();
    }

    /// <summary>
    /// 验证模板数据的有效性
    /// </summary>
    public async Task<(bool IsValid, List<string> Errors)> ValidateTemplateDataAsync(GameplayEffectTemplateData templateData)
    {
        var errors = new List<string>();

        // 验证效果数据
        if (string.IsNullOrWhiteSpace(templateData.Effect.Name))
            errors.Add("效果名称不能为空");

        if (string.IsNullOrWhiteSpace(templateData.Effect.EffectType))
            errors.Add("效果类型不能为空");

        if (string.IsNullOrWhiteSpace(templateData.Effect.StackingType))
            errors.Add("堆叠类型不能为空");

        // 验证修改器数据
        for (int i = 0; i < templateData.Modifiers.Count; i++)
        {
            var modifier = templateData.Modifiers[i];

            if (string.IsNullOrWhiteSpace(modifier.AttributeType))
                errors.Add($"修改器 {i + 1} 的属性类型不能为空");

            if (string.IsNullOrWhiteSpace(modifier.OperationType))
                errors.Add($"修改器 {i + 1} 的操作类型不能为空");
        }

        return await Task.FromResult((errors.Count == 0, errors));
    }
}