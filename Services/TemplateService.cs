using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using JoyConfig.Models.Templates;
using JoyConfig.Models.AttributeDatabase;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace JoyConfig.Services;

/// <summary>
/// 模板服务实现
/// </summary>
public class TemplateService : ITemplateService
{
    private readonly IAttributeSetRepository _attributeSetRepository;
    private readonly string _templatesDirectory;
    private readonly ISerializer _yamlSerializer;
    private readonly IDeserializer _yamlDeserializer;
    
    public TemplateService(IAttributeSetRepository attributeSetRepository)
    {
        _attributeSetRepository = attributeSetRepository;
        _templatesDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Templates");
        
        // 确保模板目录存在
        Directory.CreateDirectory(_templatesDirectory);
        
        // 配置YAML序列化器
        _yamlSerializer = new SerializerBuilder()
            .WithNamingConvention(UnderscoredNamingConvention.Instance)
            .Build();
            
        _yamlDeserializer = new DeserializerBuilder()
            .WithNamingConvention(UnderscoredNamingConvention.Instance)
            .Build();
    }
    
    public async Task<List<AttributeSetTemplate>> GetAllTemplatesAsync()
    {
        Console.WriteLine($"[TemplateService] 获取所有模板，目录：{_templatesDirectory}");
        
        var templates = new List<AttributeSetTemplate>();
        
        if (!Directory.Exists(_templatesDirectory))
        {
            Console.WriteLine($"[TemplateService] 模板目录不存在，返回空列表");
            return templates;
        }
        
        var yamlFiles = Directory.GetFiles(_templatesDirectory, "*.yaml");
        Console.WriteLine($"[TemplateService] 找到 {yamlFiles.Length} 个YAML文件");
        
        foreach (var filePath in yamlFiles)
        {
            try
            {
                var yamlContent = await File.ReadAllTextAsync(filePath);
                var template = _yamlDeserializer.Deserialize<AttributeSetTemplate>(yamlContent);
                templates.Add(template);
                Console.WriteLine($"[TemplateService] 成功加载模板：{template.Id} - {template.Name}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[TemplateService] 加载模板文件失败：{filePath}, 错误：{ex.Message}");
            }
        }
        
        return templates.OrderBy(t => t.Name).ToList();
    }
    
    public async Task<AttributeSetTemplate?> GetTemplateByIdAsync(string id)
    {
        Console.WriteLine($"[TemplateService] 获取模板：{id}");
        
        var filePath = GetTemplateFilePath(id);
        if (!File.Exists(filePath))
        {
            Console.WriteLine($"[TemplateService] 模板文件不存在：{filePath}");
            return null;
        }
        
        try
        {
            var yamlContent = await File.ReadAllTextAsync(filePath);
            var template = _yamlDeserializer.Deserialize<AttributeSetTemplate>(yamlContent);
            Console.WriteLine($"[TemplateService] 成功加载模板：{template.Id} - {template.Name}");
            return template;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[TemplateService] 加载模板失败：{id}, 错误：{ex.Message}");
            return null;
        }
    }
    
    public async Task<AttributeSetTemplate> CreateTemplateAsync(AttributeSetTemplate template)
    {
        Console.WriteLine($"[TemplateService] 创建模板：{template.Id} - {template.Name}");
        
        // 确保ID唯一
        if (await TemplateExistsAsync(template.Id))
        {
            throw new InvalidOperationException($"模板ID '{template.Id}' 已存在");
        }
        
        // 设置时间戳
        template.CreatedAt = DateTime.Now;
        template.UpdatedAt = DateTime.Now;
        
        // 序列化为YAML
        var yamlContent = _yamlSerializer.Serialize(template);
        var filePath = GetTemplateFilePath(template.Id);
        
        await File.WriteAllTextAsync(filePath, yamlContent);
        Console.WriteLine($"[TemplateService] 模板文件已保存：{filePath}");
        
        return template;
    }
    
    public async Task<AttributeSetTemplate> UpdateTemplateAsync(AttributeSetTemplate template)
    {
        Console.WriteLine($"[TemplateService] 更新模板：{template.Id} - {template.Name}");
        
        var filePath = GetTemplateFilePath(template.Id);
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"模板文件不存在：{template.Id}");
        }
        
        // 更新时间戳
        template.UpdatedAt = DateTime.Now;
        
        // 序列化为YAML
        var yamlContent = _yamlSerializer.Serialize(template);
        await File.WriteAllTextAsync(filePath, yamlContent);
        
        Console.WriteLine($"[TemplateService] 模板文件已更新：{filePath}");
        return template;
    }
    
    public async Task<bool> DeleteTemplateAsync(string id)
    {
        Console.WriteLine($"[TemplateService] 删除模板：{id}");
        
        var filePath = GetTemplateFilePath(id);
        if (!File.Exists(filePath))
        {
            Console.WriteLine($"[TemplateService] 模板文件不存在：{filePath}");
            return false;
        }
        
        try
        {
            await Task.Run(() => File.Delete(filePath));
            Console.WriteLine($"[TemplateService] 模板文件已删除：{filePath}");
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[TemplateService] 删除模板文件失败：{filePath}, 错误：{ex.Message}");
            return false;
        }
    }
    
    public Task<bool> TemplateExistsAsync(string id)
    {
        var filePath = GetTemplateFilePath(id);
        return Task.FromResult(File.Exists(filePath));
    }
    
    public async Task<AttributeSetTemplate> CreateTemplateFromAttributeSetAsync(string attributeSetId, string templateName, string? description = null)
    {
        Console.WriteLine($"[TemplateService] 从属性集创建模板：{attributeSetId} -> {templateName}");
        
        // 获取属性集数据
        var attributeSet = await _attributeSetRepository.GetAttributeSetByIdAsync(attributeSetId);
        if (attributeSet == null)
        {
            throw new ArgumentException($"属性集不存在：{attributeSetId}");
        }
        
        var attributeValues = await _attributeSetRepository.GetAttributeValueViewModelsAsync(attributeSetId);
        
        // 生成模板ID
        var guidString = Guid.NewGuid().ToString("N");
        var templateId = $"template_{DateTime.Now:yyyyMMdd_HHmmss}_{guidString[..8]}";
        
        // 创建模板对象
        var template = new AttributeSetTemplate
        {
            Id = templateId,
            Name = templateName,
            Description = description ?? $"基于属性集 '{attributeSet.Name}' 创建的模板",
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now,
            Version = "1.0.0",
            Tags = new List<string> { "auto-generated", attributeSet.Name },
            Attributes = attributeValues
                .Where(av => av?.AttributeValue != null)
                .Select(av => new AttributeValueTemplate
                {
                    Id = av.AttributeValue.AttributeId,
                    Category = av.AttributeValue.AttributeCategory,
                    BaseValue = av.AttributeValue.BaseValue,
                    MinValue = av.AttributeValue.MinValue,
                    MaxValue = av.AttributeValue.MaxValue,
                    Comment = av.AttributeValue.Comment
                })
                .ToList()
        };
        
        Console.WriteLine($"[TemplateService] 模板包含 {template.Attributes.Count} 个属性值");
        
        return await CreateTemplateAsync(template);
    }
    
    public async Task CreateAttributeSetFromTemplateAsync(string templateId, string newAttributeSetId, string newAttributeSetName)
    {
        Console.WriteLine($"[TemplateService] 从模板创建属性集：{templateId} -> {newAttributeSetId}");
        
        // 获取模板
        var template = await GetTemplateByIdAsync(templateId);
        if (template == null)
        {
            throw new ArgumentException($"模板不存在：{templateId}");
        }
        
        // 检查属性集ID是否已存在
        if (await _attributeSetRepository.AttributeSetExistsAsync(newAttributeSetId))
        {
            throw new InvalidOperationException($"属性集ID '{newAttributeSetId}' 已存在");
        }
        
        // 创建新属性集
        var newAttributeSet = new AttributeSet
        {
            Id = newAttributeSetId,
            Name = newAttributeSetName,
            Description = $"基于模板 '{template.Name}' 创建"
        };
        
        await _attributeSetRepository.CreateAttributeSetAsync(newAttributeSet);
        Console.WriteLine($"[TemplateService] 属性集已创建：{newAttributeSetId}");
        
        // 添加属性值
        foreach (var templateAttribute in template.Attributes)
        {
            try
            {
                await _attributeSetRepository.AddAttributeValueAsync(newAttributeSetId, templateAttribute.Id);
                
                // 更新属性值数据
                var attributeValue = new AttributeValue
                {
                    AttributeSetId = newAttributeSetId,
                    AttributeId = templateAttribute.Id,
                    AttributeCategory = templateAttribute.Category,
                    BaseValue = templateAttribute.BaseValue,
                    MinValue = templateAttribute.MinValue,
                    MaxValue = templateAttribute.MaxValue,
                    Comment = templateAttribute.Comment
                };
                
                await _attributeSetRepository.UpdateAttributeValueAsync(attributeValue);
                Console.WriteLine($"[TemplateService] 属性值已添加：{templateAttribute.Id}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[TemplateService] 添加属性值失败：{templateAttribute.Id}, 错误：{ex.Message}");
            }
        }
        
        Console.WriteLine($"[TemplateService] 从模板创建属性集完成：{newAttributeSetId}");
    }
    
    public string GetTemplateFilePath(string templateId)
    {
        return Path.Combine(_templatesDirectory, $"{templateId}.yaml");
    }
    
    public string GetTemplatesDirectoryPath()
    {
        return _templatesDirectory;
    }
}