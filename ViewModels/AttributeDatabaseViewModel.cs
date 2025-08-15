using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using JoyConfig.Models.AttributeDatabase;
using CommunityToolkit.Mvvm.ComponentModel;
using Attribute = JoyConfig.Models.AttributeDatabase.Attribute;
using CommunityToolkit.Mvvm.Input;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using JoyConfig.Services;
using Avalonia.Threading;

namespace JoyConfig.ViewModels;

public partial class AttributeDatabaseViewModel : EditorViewModelBase
{
    private bool _isProgrammaticSelection;
    public MainViewModel MainViewModel { get; }
    private readonly IDialogService _dialogService;
    private readonly IAttributeRepository _attributeRepository;
    private readonly IAttributeSetRepository _attributeSetRepository;
    private readonly IViewModelFactory _viewModelFactory;

    public AttributeDatabaseViewModel(
        MainViewModel mainViewModel, 
        IDialogService dialogService,
        IAttributeRepository attributeRepository,
        IAttributeSetRepository attributeSetRepository,
        IViewModelFactory viewModelFactory)
    {
        MainViewModel = mainViewModel;
        _dialogService = dialogService;
        _attributeRepository = attributeRepository;
        _attributeSetRepository = attributeSetRepository;
        _viewModelFactory = viewModelFactory;
        
        _ = LoadDataAsync();
    }
    [ObservableProperty]
    private ObservableCollection<AttributeSet> _attributeSets = new();

    [ObservableProperty]
    private ObservableCollection<AttributeCategoryViewModel> _attributeCategories = new();

    [ObservableProperty]
    private string? _errorMessage;

    [ObservableProperty]
    private AttributeSet? _selectedAttributeSet;

    [ObservableProperty]
    private object? _selectedAttribute; // Can be AttributeCategoryViewModel or Attribute

    partial void OnSelectedAttributeSetChanged(AttributeSet? value)
    {
        if (value is not null)
        {
            SelectedAttribute = null; // Clear other selection
            OpenEditorCommand.Execute(value);
        }
    }

    partial void OnSelectedAttributeChanged(object? value)
    {
        if (value is not null && !_isProgrammaticSelection)
        {
            SelectedAttributeSet = null; // Clear other selection
            OpenEditorCommand.Execute(value);
        }
    }

    [RelayCommand]
    public async Task OpenEditorAsync(object? item)
    {
        if (item is null) return;

        switch (item)
        {
            case Attribute attribute:
                MainViewModel.CurrentEditor = _viewModelFactory.CreateAttributeViewModel(attribute, this);
                break;
            case AttributeSet attributeSet:
                MainViewModel.CurrentEditor = await _viewModelFactory.CreateAttributeSetViewModelAsync(attributeSet.Id, this);
                break;
        }
    }

    [RelayCommand]
    public async Task LoadDataAsync()
    {
        await Dispatcher.UIThread.InvokeAsync(() => ErrorMessage = "Loading...");
        try
        {
            var sets = await _attributeSetRepository.GetAllAttributeSetsAsync();
            var attributes = await _attributeRepository.GetAllAttributesAsync();
            
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                AttributeSets = new ObservableCollection<AttributeSet>(sets);
                var grouped = attributes
                    .GroupBy(a => a.Category)
                    .Select(g => new AttributeCategoryViewModel 
                    { 
                        CategoryName = g.Key, 
                        Attributes = new ObservableCollection<Attribute>(g) 
                    });
                AttributeCategories = new ObservableCollection<AttributeCategoryViewModel>(grouped);
                ErrorMessage = null;
            });
        }
        catch (Exception ex)
        {
            await Dispatcher.UIThread.InvokeAsync(() => ErrorMessage = $"An error occurred: {ex.Message}");
        }
    }

    [RelayCommand]
    private void CreateNewCategory()
    {
        var vm = new CategoryViewModel(this);
        MainViewModel.OpenEditor(vm);
    }

    public void AddNewCategory(string name)
    {
        if (AttributeCategories.Any(c => c.CategoryName == name))
        {
            // In a real app, the CategoryViewModel would handle this and show an error.
            return;
        }
        
        var newCategory = new AttributeCategoryViewModel
        {
            CategoryName = name,
            Attributes = new ObservableCollection<Attribute>()
        };
        AttributeCategories.Add(newCategory);
    }

    [RelayCommand]
    private void CreateNewAttributeInCategory(string? category)
    {
        category ??= "Uncategorized";

        var newAttribute = new Attribute
        {
            Id = "New.Attribute.Id", 
            Category = category,
            Description = "A new attribute definition."
        };
        
        MainViewModel.CurrentEditor = _viewModelFactory.CreateAttributeViewModel(newAttribute, this);
    }

    [RelayCommand]
    private async Task CreateFromTemplate()
    {
        try
        {
            Console.WriteLine("[CreateFromTemplate] 开始从模板创建属性集");
            
            // 使用新的TemplateService获取YAML模板
            var templateService = new Services.TemplateService(_attributeSetRepository);
            var templates = await templateService.GetAllTemplatesAsync();
            
            Console.WriteLine($"[CreateFromTemplate] 找到 {templates.Count} 个YAML模板");
            
            if (!templates.Any())
            {
                await _dialogService.ShowMessageBoxAsync("提示", "当前没有可用的模板。请先在模板管理中创建一些模板。");
                return;
            }
            
            // 创建模板选择对话框
            var templateOptions = templates.Select(t => $"{t.Name} (v{t.Version})").ToList();
            var selectionMessage = "可用模板：\n" + string.Join("\n", templateOptions.Select((opt, i) => $"{i + 1}. {opt}"));
            
            var inputVm = _viewModelFactory.CreateInputDialogViewModel(
                "选择模板",
                $"{selectionMessage}\n\n请输入模板编号（1-{templates.Count}）：",
                "1");
            
            var selection = await _dialogService.ShowInputDialogAsync(inputVm);
            Console.WriteLine($"[CreateFromTemplate] 用户选择：'{selection}'");
            
            if (string.IsNullOrWhiteSpace(selection) || !int.TryParse(selection, out var index) || 
                index < 1 || index > templates.Count)
            {
                Console.WriteLine("[CreateFromTemplate] 用户取消或选择无效");
                return; // 取消或无效选择
            }
            
            var selectedTemplate = templates[index - 1];
            Console.WriteLine($"[CreateFromTemplate] 选中模板：{selectedTemplate.Id} - {selectedTemplate.Name}");
            
            // 输入新属性集的名称
            var nameInputVm = _viewModelFactory.CreateInputDialogViewModel(
                "创建属性集",
                $"基于模板 '{selectedTemplate.Name}' 创建新属性集，请输入名称：",
                selectedTemplate.Name.Replace("Template", "").Replace("_", "").Trim());
            
            var newName = await _dialogService.ShowInputDialogAsync(nameInputVm);
            Console.WriteLine($"[CreateFromTemplate] 新属性集名称：'{newName}'");
            
            if (string.IsNullOrWhiteSpace(newName))
            {
                Console.WriteLine("[CreateFromTemplate] 用户取消操作");
                return; // 取消操作
            }
            
            // 输入新属性集的ID
            var idInputVm = _viewModelFactory.CreateInputDialogViewModel(
                "创建属性集",
                "请输入新属性集的ID：",
                newName.Replace(" ", "_").ToLower());
            
            var newId = await _dialogService.ShowInputDialogAsync(idInputVm);
            Console.WriteLine($"[CreateFromTemplate] 新属性集ID：'{newId}'");
            
            if (string.IsNullOrWhiteSpace(newId))
            {
                Console.WriteLine("[CreateFromTemplate] 用户取消操作");
                return; // 取消操作
            }
            
            // 使用TemplateService创建属性集
            Console.WriteLine("[CreateFromTemplate] 开始使用模板服务创建属性集");
            await templateService.CreateAttributeSetFromTemplateAsync(selectedTemplate.Id, newId, newName);
            
            await _dialogService.ShowMessageBoxAsync("成功", $"属性集 '{newName}' 创建成功！");
            Console.WriteLine($"[CreateFromTemplate] 属性集创建成功：{newId}");
            
            // 刷新属性集列表
            Console.WriteLine("[CreateFromTemplate] 刷新属性集列表");
            await RefreshAttributeSetsAsync();
            
            // 打开新创建的属性集编辑器
            Console.WriteLine("[CreateFromTemplate] 打开新创建的属性集编辑器");
            var vm = await _viewModelFactory.CreateAttributeSetViewModelAsync(newId, this);
            MainViewModel.CurrentEditor = vm;
            
            Console.WriteLine("[CreateFromTemplate] 从模板创建属性集完成");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[CreateFromTemplate] 从模板创建属性集时发生异常：{ex.Message}");
            Console.WriteLine($"[CreateFromTemplate] 异常堆栈：{ex.StackTrace}");
            await _dialogService.ShowMessageBoxAsync("错误", $"从模板创建属性集时发生错误: {ex.Message}\n\n详细信息请查看控制台日志。");
        }
    }

    [RelayCommand]
    private async Task CreateNewAttributeSet()
    {
        var inputVm = _viewModelFactory.CreateInputDialogViewModel(
            "Create New Attribute Set",
            "Enter a unique ID for the new attribute set:",
            "New.AttributeSet.Id");

        var newId = await _dialogService.ShowInputDialogAsync(inputVm);

        if (string.IsNullOrWhiteSpace(newId))
        {
            ErrorMessage = "Creation cancelled.";
            return;
        }

        // Check if the ID already exists
        if (await _attributeSetRepository.AttributeSetExistsAsync(newId))
        {
            ErrorMessage = $"Error: Attribute Set with ID '{newId}' already exists.";
            return;
        }

        var newSet = new AttributeSet
        {
            Id = newId,
            Name = "New Attribute Set", // Default name, can be changed in the editor
            Description = "A new attribute set created from the editor."
        };
        
        await _attributeSetRepository.CreateAttributeSetAsync(newSet);
        
        // Refresh the list in the UI
        AttributeSets.Add(newSet);

        // Select and open the new set for editing
        SelectedAttributeSet = newSet;
        MainViewModel.CurrentEditor = await _viewModelFactory.CreateAttributeSetViewModelAsync(newSet.Id, this);
    }

    [RelayCommand]
    private async Task DeleteAttributeSetAsync(AttributeSet? attributeSet)
    {
        if (attributeSet is null) return;

        var confirmVm = _viewModelFactory.CreateConfirmationDialogViewModel(
            "Confirm Deletion",
            $"Are you sure you want to delete the attribute set '{attributeSet.Name}' ({attributeSet.Id})? This will also remove all associated attribute values. This action cannot be undone.");

        var confirmed = await _dialogService.ShowConfirmationDialogAsync(confirmVm);

        if (confirmed)
        {
            try
            {
                await _attributeSetRepository.DeleteAttributeSetAsync(attributeSet.Id);

                // If the deleted set was being edited, close the editor
                if (MainViewModel.CurrentEditor is AttributeSetViewModel vm && vm.AttributeSet.Id == attributeSet.Id)
                {
                    MainViewModel.CurrentEditor = null;
                }
                
                await RefreshAttributeSetsAsync();
                ErrorMessage = $"Attribute set '{attributeSet.Name}' deleted successfully.";
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error deleting attribute set: {ex.Message}";
            }
        }
        else
        {
            ErrorMessage = "Deletion cancelled.";
        }
    }

    [RelayCommand]
    private async Task RenameCategoryAsync(AttributeCategoryViewModel? categoryVm)
    {
        if (categoryVm?.CategoryName is null) return;
        Console.WriteLine("[ViewModel] RenameCategoryAsync started.");

        var inputVm = _viewModelFactory.CreateInputDialogViewModel(
            "Rename Category",
            $"Enter a new name for the '{categoryVm.CategoryName}' category:",
            categoryVm.CategoryName);

        var newName = await _dialogService.ShowInputDialogAsync(inputVm);
        Console.WriteLine($"[ViewModel] Input dialog returned: '{newName}'");

        if (string.IsNullOrWhiteSpace(newName) || newName == categoryVm.CategoryName)
        {
            ErrorMessage = "Rename cancelled or name unchanged.";
            Console.WriteLine("[ViewModel] Rename cancelled or name unchanged.");
            return; // Cancelled or no change
        }
        
        await Dispatcher.UIThread.InvokeAsync(() => ErrorMessage = "New name received. Previewing changes...");

        // We need a dummy old/new ID to pass to the preview service. 
        // The service primarily uses the category names for this type of change.
        var dummyOldId = $"{categoryVm.CategoryName}.Dummy";
        var dummyNewId = $"{newName}.Dummy";

        Console.WriteLine("[ViewModel] Calling PreviewAttributeChangeAsync...");
        var preview = await _attributeRepository.PreviewAttributeChangeAsync(dummyOldId, dummyNewId, categoryVm.CategoryName, newName);
        Console.WriteLine($"[ViewModel] PreviewAttributeChangeAsync returned. IsValid: {preview.IsValid}, Affected Attributes: {preview.AffectedAttributes.Count}");

        if (!preview.IsValid)
        {
            ErrorMessage = preview.ErrorMessage;
            return;
        }
        
        ErrorMessage = "Preview successful. Awaiting confirmation...";

        if (!preview.AffectedAttributes.Any())
        {
            // This case is for renaming an empty category.
            // We can just update the view model directly.
            categoryVm.CategoryName = newName;
            return;
        }

        var confirmVm = _viewModelFactory.CreateConfirmationDialogViewModel(
            "Confirm Category Rename",
            $"Renaming '{categoryVm.CategoryName}' to '{newName}' will update {preview.AffectedAttributes.Count} attribute(s) and {preview.AffectedValueCount} value(s) across {preview.AffectedAttributeSets.Count} attribute set(s). This action cannot be undone.",
            preview.AffectedAttributeSets.Select(s => $"Set: {s.Id} ({s.Name})").ToList());

        var confirmed = await _dialogService.ShowConfirmationDialogAsync(confirmVm);
        Console.WriteLine($"[ViewModel] Confirmation dialog returned: {confirmed}");

        if (confirmed)
        {
            await Dispatcher.UIThread.InvokeAsync(() => ErrorMessage = "Confirmation received. Executing changes...");
            
            Console.WriteLine("[ViewModel] Calling ExecuteAttributeChangeAsync...");
            await _attributeRepository.ExecuteAttributeChangeAsync(preview);
            Console.WriteLine("[ViewModel] ExecuteAttributeChangeAsync finished.");
            
            Console.WriteLine("[ViewModel] Reloading data...");
            await LoadDataAsync(); // Reload the entire tree
            Console.WriteLine("[ViewModel] Data reloaded.");
            await Dispatcher.UIThread.InvokeAsync(() => ErrorMessage = "Rename successful.");
        }
        else
        {
            ErrorMessage = "Rename cancelled by user.";
        }
    }

    public async Task RefreshAttributeListAsync(Attribute attribute)
    {
        var categoryVm = AttributeCategories.FirstOrDefault(c => c.CategoryName == attribute.Category);
        if (categoryVm != null)
        {
            // Category exists, check if the attribute is already in the list
            var attrInList = categoryVm.Attributes.FirstOrDefault(a => a.Id == attribute.Id);
            if (attrInList == null)
            {
                // Add new attribute to existing category
                categoryVm.Attributes.Add(attribute);
            }
            // If it exists, it's an update, and the data binding will handle UI changes.
        }
        else
        {
            // Category does not exist, create a new one
            categoryVm = new AttributeCategoryViewModel
            {
                CategoryName = attribute.Category,
                Attributes = new ObservableCollection<Attribute> { attribute }
            };
            AttributeCategories.Add(categoryVm);
        }

        // Ensure the category is expanded and the item is selected
        categoryVm.IsExpanded = true;
        await Task.Delay(100); // Give UI time to render the new item
        SelectedAttribute = attribute;
    }
    
    public async Task ReloadAndSelectAttributeAsync(string attributeId)
    {
        await LoadDataAsync();
        
        // Find the attribute in the reloaded data
        var category = AttributeCategories.FirstOrDefault(c => attributeId.StartsWith(c.CategoryName + "."));
        if (category != null)
        {
            var attribute = category.Attributes.FirstOrDefault(a => a.Id == attributeId);
            if (attribute != null)
            {
                _isProgrammaticSelection = true;
                
                // Open a new editor for this attribute
                MainViewModel.CurrentEditor = _viewModelFactory.CreateAttributeViewModel(attribute, this);
                
                // Also update the selection in the list
                SelectedAttribute = attribute;
                category.IsExpanded = true;
                
                _isProgrammaticSelection = false;
            }
        }
    }

    public async Task ReloadAndCloseEditorAsync()
    {
        await LoadDataAsync();
        MainViewModel.CurrentEditor = new WelcomeViewModel();
    }

    public async Task RefreshAttributeSetsAsync()
    {
        try
        {
            var sets = await _attributeSetRepository.GetAllAttributeSetsAsync();
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                AttributeSets = new ObservableCollection<AttributeSet>(sets);
            });
        }
        catch (Exception ex)
        {
            await Dispatcher.UIThread.InvokeAsync(() => ErrorMessage = $"Failed to refresh sets: {ex.Message}");
        }
    }

    [RelayCommand]
    private async Task DeleteCategoryAsync(AttributeCategoryViewModel? categoryVm)
    {
        if (categoryVm?.CategoryName is null) return;

        ErrorMessage = null;
        
        // Safety Check
        var attributeIds = categoryVm.Attributes.Select(a => a.Id).ToList();
        var referencingSets = await _attributeSetRepository.GetReferencingAttributeSetsAsync(attributeIds);
        var affectedValuesCount = 0;
        
        if (attributeIds.Any())
        {
            affectedValuesCount = await _attributeRepository.GetAttributeValueCountAsync(attributeIds);
        }

        var dialogViewModel = _viewModelFactory.CreateConfirmationDialogViewModel(
            "Confirm Category Deletion",
            $"You are about to delete the entire '{categoryVm.CategoryName}' category. " +
            $"This will permanently delete {categoryVm.Attributes.Count} attribute(s) and {affectedValuesCount} value(s) across {referencingSets.Count} attribute set(s). " +
            "This action cannot be undone.",
            referencingSets.Select(s => $"Set: {s.Id} ({s.Name})").ToList());
        
        var confirmed = await _dialogService.ShowConfirmationDialogAsync(dialogViewModel);

        if (confirmed)
        {
            try
            {
                await _attributeRepository.DeleteCategoryAsync(categoryVm.CategoryName);
                await ReloadAndCloseEditorAsync();
                ErrorMessage = $"Category '{categoryVm.CategoryName}' deleted successfully.";
            }
            catch (Exception ex)
            {
                ErrorMessage = $"An error occurred during deletion: {ex.Message}";
            }
        }
        else
        {
            ErrorMessage = "Deletion cancelled.";
        }
    }
}

public partial class AttributeCategoryViewModel : ObservableObject
{
    [ObservableProperty]
    private string? _categoryName;

    [ObservableProperty]
    private ObservableCollection<Attribute> _attributes = new();

    [ObservableProperty]
    private bool _isExpanded = false; // Default to collapsed
}
