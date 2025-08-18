using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using JoyConfig.Models.GameplayEffectDatabase;
using JoyConfig.Services;
using Avalonia.Threading;

namespace JoyConfig.ViewModels.GameplayEffectDatabase;

/// <summary>
/// 游戏效果模板管理ViewModel
/// </summary>
public partial class GameplayEffectTemplateViewModel : ObservableObject
{
    private readonly IGameplayEffectTemplateService _templateService;
    private readonly IDialogService _dialogService;
    
    [ObservableProperty]
    private ObservableCollection<GameplayEffectTemplate> _templates = new();
    
    [ObservableProperty]
    private ObservableCollection<GameplayEffectTemplate> _filteredTemplates = new();
    
    [ObservableProperty]
    private GameplayEffectTemplate? _selectedTemplate;
    
    [ObservableProperty]
    private string _searchText = string.Empty;
    
    [ObservableProperty]
    private string _selectedCategory = "All";
    
    [ObservableProperty]
    private ObservableCollection<string> _categories = new();
    
    [ObservableProperty]
    private bool _isLoading;
    
    [ObservableProperty]
    private string? _errorMessage;
    
    public GameplayEffectTemplateViewModel(
        IGameplayEffectTemplateService templateService,
        IDialogService dialogService)
    {
        _templateService = templateService;
        _dialogService = dialogService;
        
        // 初始化分类列表
        Categories.Add("All");
        
        // 加载数据
        _ = LoadTemplatesAsync();
    }
    
    /// <summary>
    /// 加载所有模板
    /// </summary>
    [RelayCommand]
    public async Task LoadTemplatesAsync()
    {
        try
        {
            IsLoading = true;
            ErrorMessage = null;
            
            var templates = await _templateService.GetAllTemplatesAsync();
            var categories = await _templateService.GetAllCategoriesAsync();
            
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                Templates.Clear();
                foreach (var template in templates)
                {
                    Templates.Add(template);
                }
                
                Categories.Clear();
                Categories.Add("All");
                foreach (var category in categories)
                {
                    Categories.Add(category);
                }
                
                ApplyFilters();
            });
        }
        catch (Exception ex)
        {
            ErrorMessage = $"加载模板失败: {ex.Message}";
            await _dialogService.ShowErrorAsync("加载失败", ex.Message);
        }
        finally
        {
            IsLoading = false;
        }
    }
    
    /// <summary>
    /// 搜索模板
    /// </summary>
    [RelayCommand]
    public async Task SearchTemplatesAsync()
    {
        try
        {
            IsLoading = true;
            
            var searchResults = await _templateService.SearchTemplatesAsync(SearchText);
            
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                Templates.Clear();
                foreach (var template in searchResults)
                {
                    Templates.Add(template);
                }
                
                ApplyFilters();
            });
        }
        catch (Exception ex)
        {
            ErrorMessage = $"搜索模板失败: {ex.Message}";
            await _dialogService.ShowErrorAsync("搜索失败", ex.Message);
        }
        finally
        {
            IsLoading = false;
        }
    }
    
    /// <summary>
    /// 创建模板
    /// </summary>
    [RelayCommand]
    public async Task CreateTemplateAsync(string sourceEffectId)
    {
        try
        {
            // 显示创建模板对话框
            var templateName = await _dialogService.ShowInputAsync(
                "创建模板", 
                "请输入模板名称:", 
                "新模板");
                
            if (string.IsNullOrWhiteSpace(templateName))
                return;
            
            var description = await _dialogService.ShowInputAsync(
                "创建模板", 
                "请输入模板描述（可选）:", 
                "");
            
            var category = await _dialogService.ShowInputAsync(
                "创建模板", 
                "请输入模板分类:", 
                "Default");
            
            IsLoading = true;
            
            var template = await _templateService.CreateTemplateFromEffectAsync(
                sourceEffectId, 
                templateName, 
                description, 
                category ?? "Default");
            
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                Templates.Add(template);
                ApplyFilters();
            });
            
            await _dialogService.ShowInfoAsync("创建成功", $"模板 '{templateName}' 已成功创建。");
        }
        catch (Exception ex)
        {
            ErrorMessage = $"创建模板失败: {ex.Message}";
            await _dialogService.ShowErrorAsync("创建失败", ex.Message);
        }
        finally
        {
            IsLoading = false;
        }
    }
    
    /// <summary>
    /// 从模板创建效果
    /// </summary>
    [RelayCommand]
    public async Task<AttributeEffect?> CreateEffectFromTemplateAsync(GameplayEffectTemplate? template = null)
    {
        try
        {
            template ??= SelectedTemplate;
            if (template == null)
            {
                await _dialogService.ShowWarningAsync("提示", "请先选择一个模板。");
                return null;
            }
            
            var effectId = await _dialogService.ShowInputAsync(
                "从模板创建效果", 
                "请输入新效果ID:", 
                $"effect_{DateTime.Now:yyyyMMdd_HHmmss}");
                
            if (string.IsNullOrWhiteSpace(effectId))
                return null;
            
            var effectName = await _dialogService.ShowInputAsync(
                "从模板创建效果", 
                "请输入新效果名称:", 
                template.Name);
                
            if (string.IsNullOrWhiteSpace(effectName))
                return null;
            
            IsLoading = true;
            
            var newEffect = await _templateService.CreateEffectFromTemplateAsync(
                template.Id, 
                effectId, 
                effectName);
            
            await _dialogService.ShowInfoAsync("创建成功", $"效果 '{effectName}' 已从模板创建成功。");
            
            return newEffect;
        }
        catch (Exception ex)
        {
            ErrorMessage = $"从模板创建效果失败: {ex.Message}";
            await _dialogService.ShowErrorAsync("创建失败", ex.Message);
            return null;
        }
        finally
        {
            IsLoading = false;
        }
    }
    
    /// <summary>
    /// 删除模板
    /// </summary>
    [RelayCommand]
    public async Task DeleteTemplateAsync(GameplayEffectTemplate? template = null)
    {
        try
        {
            template ??= SelectedTemplate;
            if (template == null)
            {
                await _dialogService.ShowWarningAsync("提示", "请先选择一个模板。");
                return;
            }
            
            if (template.IsBuiltIn)
            {
                await _dialogService.ShowWarningAsync("提示", "不能删除系统内置模板。");
                return;
            }
            
            var confirmed = await _dialogService.ShowConfirmationAsync(
                "确认删除", 
                $"确定要删除模板 '{template.Name}' 吗？\n\n此操作不可撤销。");
                
            if (!confirmed)
                return;
            
            IsLoading = true;
            
            var success = await _templateService.DeleteTemplateAsync(template.Id);
            
            if (success)
            {
                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    Templates.Remove(template);
                    ApplyFilters();
                    
                    if (SelectedTemplate == template)
                        SelectedTemplate = null;
                });
                
                await _dialogService.ShowInfoAsync("删除成功", $"模板 '{template.Name}' 已成功删除。");
            }
            else
            {
                await _dialogService.ShowErrorAsync("删除失败", "模板删除失败，请重试。");
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"删除模板失败: {ex.Message}";
            await _dialogService.ShowErrorAsync("删除失败", ex.Message);
        }
        finally
        {
            IsLoading = false;
        }
    }
    
    /// <summary>
    /// 应用筛选条件
    /// </summary>
    private void ApplyFilters()
    {
        var filtered = Templates.AsEnumerable();
        
        // 按分类筛选
        if (SelectedCategory != "All")
        {
            filtered = filtered.Where(t => t.Category == SelectedCategory);
        }
        
        // 按搜索文本筛选
        if (!string.IsNullOrWhiteSpace(SearchText))
        {
            var searchTerm = SearchText.ToLower();
            filtered = filtered.Where(t => 
                t.Name.ToLower().Contains(searchTerm) ||
                (t.Description?.ToLower().Contains(searchTerm) ?? false) ||
                (t.Tags?.ToLower().Contains(searchTerm) ?? false));
        }
        
        FilteredTemplates.Clear();
        foreach (var template in filtered.OrderBy(t => t.Category).ThenBy(t => t.Name))
        {
            FilteredTemplates.Add(template);
        }
    }
    
    /// <summary>
    /// 当搜索文本改变时
    /// </summary>
    partial void OnSearchTextChanged(string value)
    {
        ApplyFilters();
    }
    
    /// <summary>
    /// 当选择的分类改变时
    /// </summary>
    partial void OnSelectedCategoryChanged(string value)
    {
        ApplyFilters();
    }
}