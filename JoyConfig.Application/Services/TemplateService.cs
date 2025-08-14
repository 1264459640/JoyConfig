using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using JoyConfig.Application.Abstract.Services;
using JoyConfig.Infrastructure.Models.DTOs;
using JoyConfig.Infrastructure.Models.AttributeDatabase;
using JoyConfig.Base.Utilities;
using AttributeSet = JoyConfig.Infrastructure.Models.AttributeDatabase.AttributeSet;

namespace JoyConfig.Infrastructure.Services;

/// <summary>
/// 模板服务实现 - 提供模板管理功能的具体实�?/// 实现ITemplateService接口，基于文件系统的模板存储
/// </summary>
public class TemplateService : ITemplateService
{
    private readonly IDataRepository _dataRepository;
    private readonly string _templateDirectory;
    private const string TemplateFileExtension = ".json";
    
    public TemplateService(IDataRepository dataRepository)
    {
        _dataRepository = dataRepository ?? throw new ArgumentNullException(nameof(dataRepository));
        _templateDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "JoyConfig", "Templates");
    }
    
    #region Template File Management
    
    public async Task<List<string>> GetAllTemplateFilesAsync()
    {
        await EnsureTemplateDirectoryExistsAsync();
        
        var files = Directory.GetFiles(_templateDirectory, $"*{TemplateFileExtension}")
            .Select(Path.GetFileNameWithoutExtension)
            .Where(name => !string.IsNullOrEmpty(name))
            .OrderBy(name => name)
            .ToList();
            
        return files!;
    }
    
    public async Task<AttributeSetTemplate?> LoadTemplateAsync(string templateFileName)
    {
        try
        {
            var filePath = GetTemplateFilePath(templateFileName);
            if (!File.Exists(filePath))
                return null;
                
            var json = await File.ReadAllTextAsync(filePath);
            var template = JsonSerializer.Deserialize<AttributeSetTemplate>(json);
            return template;
        }
        catch (Exception ex)
        {
            // Log error
            Console.WriteLine($"Error loading template {templateFileName}: {ex.Message}");
            return null;
        }
    }
    
    public async Task SaveTemplateAsync(AttributeSetTemplate template, string fileName)
    {
        await EnsureTemplateDirectoryExistsAsync();
        
        var filePath = GetTemplateFilePath(fileName);
        var json = JsonSerializer.Serialize(template, new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
        
        await File.WriteAllTextAsync(filePath, json);
        TemplateCreated?.Invoke(fileName);
    }
    
    public async Task DeleteTemplateAsync(string templateFileName)
    {
        var filePath = GetTemplateFilePath(templateFileName);
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
            TemplateDeleted?.Invoke(templateFileName);
        }
        await Task.CompletedTask;
    }
    
    public async Task RenameTemplateAsync(string oldFileName, string newFileName)
    {
        var oldPath = GetTemplateFilePath(oldFileName);
        var newPath = GetTemplateFilePath(newFileName);
        
        if (File.Exists(oldPath) && !File.Exists(newPath))
        {
            File.Move(oldPath, newPath);
            TemplateUpdated?.Invoke(newFileName);
        }
        await Task.CompletedTask;
    }
    
    public async Task CopyTemplateAsync(string sourceFileName, string targetFileName)
    {
        var sourcePath = GetTemplateFilePath(sourceFileName);
        var targetPath = GetTemplateFilePath(targetFileName);
        
        if (File.Exists(sourcePath) && !File.Exists(targetPath))
        {
            File.Copy(sourcePath, targetPath);
            TemplateCreated?.Invoke(targetFileName);
        }
        await Task.CompletedTask;
    }
    
    public async Task<bool> TemplateExistsAsync(string templateFileName)
    {
        var filePath = GetTemplateFilePath(templateFileName);
        return File.Exists(filePath);
    }
    
    #endregion
    
    #region Template Operations
    
    public async Task<AttributeSetTemplate> CreateTemplateFromAttributeSetAsync(AttributeSet attributeSet, string templateName, string? description = null)
    {
        var attributeValues = await _dataRepository.GetAttributeValuesBySetIdAsync(attributeSet.Id);
        
        var template = new AttributeSetTemplate
        {
            Name = templateName,
            Description = description ?? $"Template created from {attributeSet.Name}",
            AttributeValues = attributeValues.Select(av => new AttributeValueTemplate
            {
                AttributeId = av.AttributeId,
                BaseValue = av.BaseValue,
                MinValue = av.MinValue,
                MaxValue = av.MaxValue,
                AttributeCategory = av.AttributeCategory,
                Comment = av.Comment
            }).ToList()
        };
        
        return template;
    }
    
    public async Task<bool> ApplyTemplateToAttributeSetAsync(AttributeSetTemplate template, string targetAttributeSetId)
    {
        try
        {
            var attributeSet = await _dataRepository.GetAttributeSetByIdAsync(targetAttributeSetId);
            if (attributeSet == null)
                return false;
                
            foreach (var templateValue in template.AttributeValues)
            {
                var attributeValue = new AttributeValue
                {
                    AttributeSetId = targetAttributeSetId,
                    AttributeId = templateValue.AttributeId,
                    BaseValue = templateValue.BaseValue,
                    MinValue = templateValue.MinValue,
                    MaxValue = templateValue.MaxValue,
                    AttributeCategory = templateValue.AttributeCategory,
                    Comment = templateValue.Comment
                };
                
                await _dataRepository.SaveAttributeValueAsync(attributeValue);
            }
            
            TemplateApplied?.Invoke(template.Name, targetAttributeSetId);
            return true;
        }
        catch
        {
            return false;
        }
    }
    
    public async Task<AttributeSet> CreateAttributeSetFromTemplateAsync(AttributeSetTemplate template, string newAttributeSetId, string newAttributeSetName)
    {
        var attributeSet = new AttributeSet
        {
            Id = newAttributeSetId,
            Name = newAttributeSetName,
            Description = $"Created from template: {template.Name}"
        };
        
        var createdSet = await _dataRepository.CreateAttributeSetAsync(attributeSet);
        
        foreach (var templateValue in template.AttributeValues)
        {
            var attributeValue = new AttributeValue
            {
                AttributeSetId = newAttributeSetId,
                AttributeId = templateValue.AttributeId,
                BaseValue = templateValue.BaseValue,
                MinValue = templateValue.MinValue,
                MaxValue = templateValue.MaxValue,
                AttributeCategory = templateValue.AttributeCategory,
                Comment = templateValue.Comment
            };
            
            await _dataRepository.SaveAttributeValueAsync(attributeValue);
        }
        
        return createdSet;
    }
    
    public async Task<TemplateValidationResult> ValidateTemplateAsync(AttributeSetTemplate template)
    {
        var result = new TemplateValidationResult { IsValid = true };
        
        if (string.IsNullOrWhiteSpace(template.Name))
        {
            result.IsValid = false;
            result.Errors.Add("Template name is required");
        }
        
        if (template.AttributeValues == null || !template.AttributeValues.Any())
        {
            result.IsValid = false;
            result.Errors.Add("Template must contain at least one attribute value");
        }
        else
        {
            foreach (var attributeValue in template.AttributeValues)
            {
                if (string.IsNullOrWhiteSpace(attributeValue.AttributeId))
                {
                    result.IsValid = false;
                    result.Errors.Add("All attribute values must have a valid AttributeId");
                }
                else if (!await _dataRepository.AttributeExistsAsync(attributeValue.AttributeId))
                {
                    result.Warnings.Add($"Attribute '{attributeValue.AttributeId}' does not exist in the database");
                }
            }
        }
        
        return result;
    }
    
    public async Task<AttributeSetTemplate> MergeTemplatesAsync(List<AttributeSetTemplate> templates, string mergedTemplateName)
    {
        var mergedTemplate = new AttributeSetTemplate
        {
            Name = mergedTemplateName,
            Description = $"Merged from {templates.Count} templates: {string.Join(", ", templates.Select(t => t.Name))}",
            AttributeValues = new List<AttributeValueTemplate>()
        };
        
        var attributeDict = new Dictionary<string, AttributeValueTemplate>();
        
        foreach (var template in templates)
        {
            foreach (var attributeValue in template.AttributeValues)
            {
                // Last template wins in case of conflicts
                attributeDict[attributeValue.AttributeId] = attributeValue;
            }
        }
        
        mergedTemplate.AttributeValues = attributeDict.Values.ToList();
        return mergedTemplate;
    }
    
    #endregion
    
    #region Template Search and Filter
    
    public async Task<List<AttributeSetTemplate>> SearchTemplatesAsync(string searchTerm)
    {
        var allTemplateFiles = await GetAllTemplateFilesAsync();
        var matchingTemplates = new List<AttributeSetTemplate>();
        
        foreach (var fileName in allTemplateFiles)
        {
            var template = await LoadTemplateAsync(fileName);
            if (template != null && 
                (template.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                 (template.Description?.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ?? false)))
            {
                matchingTemplates.Add(template);
            }
        }
        
        return matchingTemplates;
    }
    
    public async Task<List<AttributeSetTemplate>> FilterTemplatesByTagsAsync(List<string> tags)
    {
        // For now, return all templates since we don't have tag support yet
        var allTemplateFiles = await GetAllTemplateFilesAsync();
        var templates = new List<AttributeSetTemplate>();
        
        foreach (var fileName in allTemplateFiles)
        {
            var template = await LoadTemplateAsync(fileName);
            if (template != null)
            {
                templates.Add(template);
            }
        }
        
        return templates;
    }
    
    public async Task<TemplateStatistics> GetTemplateStatisticsAsync(string templateFileName)
    {
        var filePath = GetTemplateFilePath(templateFileName);
        var fileInfo = new FileInfo(filePath);
        var template = await LoadTemplateAsync(templateFileName);
        
        var stats = new TemplateStatistics
        {
            FileSizeBytes = fileInfo.Exists ? fileInfo.Length : 0,
            CreatedAt = fileInfo.Exists ? fileInfo.CreationTime : DateTime.MinValue,
            LastModified = fileInfo.Exists ? fileInfo.LastWriteTime : DateTime.MinValue,
            AttributeCount = template?.AttributeValues?.Count ?? 0,
            CategoryCount = template?.AttributeValues?.Select(av => av.AttributeCategory).Distinct().Count() ?? 0,
            UsageCount = 0 // TODO: Implement usage tracking
        };
        
        return stats;
    }
    
    #endregion
    
    #region Template Directory Management
    
    public string GetTemplateDirectory()
    {
        return _templateDirectory;
    }
    
    public async Task EnsureTemplateDirectoryExistsAsync()
    {
        if (!Directory.Exists(_templateDirectory))
        {
            Directory.CreateDirectory(_templateDirectory);
        }
        await Task.CompletedTask;
    }
    
    public async Task CleanupInvalidTemplatesAsync()
    {
        var templateFiles = await GetAllTemplateFilesAsync();
        var invalidFiles = new List<string>();
        
        foreach (var fileName in templateFiles)
        {
            var template = await LoadTemplateAsync(fileName);
            if (template == null)
            {
                invalidFiles.Add(fileName);
            }
        }
        
        foreach (var invalidFile in invalidFiles)
        {
            await DeleteTemplateAsync(invalidFile);
        }
    }
    
    public async Task ImportTemplateAsync(string sourceFilePath, string? targetFileName = null)
    {
        if (!File.Exists(sourceFilePath))
            throw new FileNotFoundException($"Source file not found: {sourceFilePath}");
            
        var fileName = targetFileName ?? Path.GetFileNameWithoutExtension(sourceFilePath);
        var targetPath = GetTemplateFilePath(fileName);
        
        File.Copy(sourceFilePath, targetPath, overwrite: true);
        TemplateCreated?.Invoke(fileName);
        await Task.CompletedTask;
    }
    
    public async Task ExportTemplateAsync(string templateFileName, string targetFilePath)
    {
        var sourcePath = GetTemplateFilePath(templateFileName);
        if (!File.Exists(sourcePath))
            throw new FileNotFoundException($"Template file not found: {templateFileName}");
            
        File.Copy(sourcePath, targetFilePath, overwrite: true);
        await Task.CompletedTask;
    }
    
    #endregion
    
    #region Events
    
    public event Action<string>? TemplateCreated;
    public event Action<string>? TemplateDeleted;
    public event Action<string>? TemplateUpdated;
    public event Action<string, string>? TemplateApplied;
    
    #endregion
    
    #region Private Methods
    
    private string GetTemplateFilePath(string templateFileName)
    {
        var fileName = templateFileName.EndsWith(TemplateFileExtension) 
            ? templateFileName 
            : templateFileName + TemplateFileExtension;
        return Path.Combine(_templateDirectory, fileName);
    }
    
    #endregion
}
