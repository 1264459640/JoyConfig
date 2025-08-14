using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using JoyConfig.Application.Abstract.Services;
using JoyConfig.Infrastructure.Models.AttributeDatabase;
using JoyConfig.Infrastructure.Models.DTOs;
using JoyConfig.Infrastructure.Data;

using AttributeSet = JoyConfig.Infrastructure.Models.AttributeDatabase.AttributeSet;
using Attribute = JoyConfig.Infrastructure.Models.AttributeDatabase.Attribute;
using AttributeValue = JoyConfig.Infrastructure.Models.AttributeDatabase.AttributeValue;

namespace JoyConfig.Infrastructure.Services;

/// <summary>
/// 数据访问仓储实现 - 基于Entity Framework Core的数据访问实�?/// 实现IDataRepository接口，提供具体的数据访问功能
/// </summary>
public class DataRepository : IDataRepository
{
    private readonly AttributeDatabaseContext _context;
    
    public DataRepository(AttributeDatabaseContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }
    
    #region AttributeSet Operations
    
    public async Task<List<AttributeSet>> GetAllAttributeSetsAsync()
    {
        return await _context.AttributeSets
            .Include(s => s.AttributeValues)
            .ThenInclude(v => v.Attribute)
            .OrderBy(s => s.Name)
            .ToListAsync();
    }
    
    public async Task<AttributeSet?> GetAttributeSetByIdAsync(string id)
    {
        return await _context.AttributeSets
            .Include(s => s.AttributeValues)
            .ThenInclude(v => v.Attribute)
            .FirstOrDefaultAsync(s => s.Id == id);
    }
    
    public async Task<AttributeSet> CreateAttributeSetAsync(AttributeSet attributeSet)
    {
        _context.AttributeSets.Add(attributeSet);
        await _context.SaveChangesAsync();
        return attributeSet;
    }
    
    public async Task<AttributeSet> AddAttributeSetAsync(AttributeSet attributeSet)
    {
        return await CreateAttributeSetAsync(attributeSet);
    }
    
    public async Task<AttributeSet> UpdateAttributeSetAsync(AttributeSet attributeSet)
    {
        _context.AttributeSets.Update(attributeSet);
        await _context.SaveChangesAsync();
        return attributeSet;
    }
    
    public async Task<bool> DeleteAttributeSetAsync(string id)
    {
        var attributeSet = await _context.AttributeSets.FindAsync(id);
        if (attributeSet == null)
            return false;
            
        _context.AttributeSets.Remove(attributeSet);
        await _context.SaveChangesAsync();
        return true;
    }
    
    #endregion
    
    #region Attribute Operations
    
    public async Task<List<Attribute>> GetAllAttributesAsync()
    {
        return await _context.Attributes
            .OrderBy(a => a.Category)
            .ThenBy(a => a.Id)
            .ToListAsync();
    }
    
    public async Task<List<Attribute>> GetAttributesByCategoryAsync(string category)
    {
        return await _context.Attributes
            .Where(a => a.Category == category)
            .OrderBy(a => a.Id)
            .ToListAsync();
    }
    
    public async Task<Attribute?> GetAttributeByIdAsync(string id)
    {
        return await _context.Attributes
            .FirstOrDefaultAsync(a => a.Id == id);
    }
    
    public async Task<Attribute> CreateAttributeAsync(Attribute attribute)
    {
        _context.Attributes.Add(attribute);
        await _context.SaveChangesAsync();
        return attribute;
    }
    
    public async Task<Attribute> UpdateAttributeAsync(Attribute attribute)
    {
        _context.Attributes.Update(attribute);
        await _context.SaveChangesAsync();
        return attribute;
    }
    
    public async Task<bool> DeleteAttributeAsync(string id)
    {
        var attribute = await _context.Attributes.FindAsync(id);
        if (attribute == null)
            return false;
            
        _context.Attributes.Remove(attribute);
        await _context.SaveChangesAsync();
        return true;
    }
    
    public async Task<List<AttributeSet>> GetReferencingAttributeSetsAsync(string attributeId)
    {
        return await _context.AttributeValues
            .Where(v => v.AttributeId == attributeId)
            .Select(v => v.AttributeSet)
            .Distinct()
            .ToListAsync();
    }
    
    public async Task<List<AttributeSet>> GetReferencingAttributeSetsAsync(List<string> attributeIds)
    {
        return await _context.AttributeValues
            .Where(v => attributeIds.Contains(v.AttributeId))
            .Select(v => v.AttributeSet)
            .Distinct()
            .ToListAsync();
    }
    
    #endregion
    
    #region AttributeValue Operations
    
    public async Task<List<AttributeValue>> GetAttributeValuesBySetIdAsync(string attributeSetId)
    {
        return await _context.AttributeValues
            .Include(v => v.Attribute)
            .Where(v => v.AttributeSetId == attributeSetId)
            .OrderBy(v => v.Attribute.Category)
            .ThenBy(v => v.Attribute.Id)
            .ToListAsync();
    }
    
    public async Task<AttributeValue> SaveAttributeValueAsync(AttributeValue attributeValue)
    {
        var existing = await _context.AttributeValues
            .FirstOrDefaultAsync(v => v.AttributeSetId == attributeValue.AttributeSetId && 
                                    v.AttributeId == attributeValue.AttributeId);
        
        if (existing != null)
        {
            existing.BaseValue = attributeValue.BaseValue;
            existing.MinValue = attributeValue.MinValue;
            existing.MaxValue = attributeValue.MaxValue;
            existing.AttributeCategory = attributeValue.AttributeCategory;
            existing.Comment = attributeValue.Comment;
            _context.AttributeValues.Update(existing);
        }
        else
        {
            _context.AttributeValues.Add(attributeValue);
        }
        
        await _context.SaveChangesAsync();
        return existing ?? attributeValue;
    }
    
    public async Task<bool> DeleteAttributeValueAsync(string attributeSetId, string attributeId)
    {
        var attributeValue = await _context.AttributeValues
            .FirstOrDefaultAsync(v => v.AttributeSetId == attributeSetId && v.AttributeId == attributeId);
            
        if (attributeValue == null)
            return false;
            
        _context.AttributeValues.Remove(attributeValue);
        await _context.SaveChangesAsync();
        return true;
    }
    
    #endregion
    
    #region Advanced Operations
    
    public async Task<AttributeChangePreview> PreviewAttributeChangeAsync(string attributeId, string newName)
    {
        // 需要提供旧分类参数，从现有属性获取
        var existingAttribute = await _context.Attributes.FindAsync(attributeId);
        var oldCategory = existingAttribute?.Category ?? "Unknown";
        return await _context.PreviewAttributeChangeAsync(attributeId, newName, oldCategory, newName);
    }
    
    public async Task<AttributeChangePreview> PreviewAttributeChangeAsync(string oldId, string newId, string category, string newName)
    {
        return await _context.PreviewAttributeChangeAsync(oldId, newId, category, newName);
    }
    
    public async Task<bool> ExecuteAttributeChangeAsync(AttributeChangePreview preview)
    {
        try
        {
            await _context.ExecuteAttributeChangeAsync(preview);
            return true;
        }
        catch
        {
            return false;
        }
    }
    
    public async Task<List<string>> GetAllCategoriesAsync()
    {
        return await _context.Attributes
            .Select(a => a.Category)
            .Distinct()
            .Where(c => !string.IsNullOrEmpty(c))
            .OrderBy(c => c)
            .ToListAsync();
    }
    
    public async Task<List<Attribute>> SearchAttributesAsync(string searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
            return await GetAllAttributesAsync();
            
        var lowerSearchTerm = searchTerm.ToLower();
        return await _context.Attributes
            .Where(a => a.Id.ToLower().Contains(lowerSearchTerm) ||
                       a.Category.ToLower().Contains(lowerSearchTerm) ||
                       (a.Description != null && a.Description.ToLower().Contains(lowerSearchTerm)))
            .OrderBy(a => a.Category)
            .ThenBy(a => a.Id)
            .ToListAsync();
    }
    
    public async Task<bool> AttributeExistsAsync(string attributeId)
    {
        return await _context.Attributes.AnyAsync(a => a.Id == attributeId);
    }
    
    public async Task<bool> AttributeSetExistsAsync(string attributeSetId)
    {
        return await _context.AttributeSets.AnyAsync(s => s.Id == attributeSetId);
    }
    
    public async Task<int> GetAttributeValuesCountAsync(List<string> attributeIds)
    {
        return await _context.AttributeValues
            .Where(v => attributeIds.Contains(v.AttributeId))
            .CountAsync();
    }
    
    public async Task<bool> DeleteCategoryAsync(string categoryName)
    {
        try
        {
            var attributesToDelete = await _context.Attributes
                .Where(a => a.Category == categoryName)
                .ToListAsync();
                
            if (attributesToDelete.Any())
            {
                _context.Attributes.RemoveRange(attributesToDelete);
                await _context.SaveChangesAsync();
            }
            
            return true;
        }
        catch
        {
            return false;
        }
    }
    
    #endregion
    
    #region Transaction Support
    
    public async Task<IDisposable> BeginTransactionAsync()
    {
        return await _context.Database.BeginTransactionAsync();
    }
    
    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }
    
    #endregion
}
