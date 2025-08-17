using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using JoyConfig.Models.AttributeDatabase;
using JoyConfig.Services;
using JoyConfig.ViewModels.Base;
using JoyConfig.ViewModels.Dialogs;
using Microsoft.EntityFrameworkCore;

namespace JoyConfig.ViewModels.AttributeDatabase;

public partial class AttributeSetViewModel : EditorViewModelBase
{
    private readonly string _attributeSetId;
    private readonly AttributeDatabaseViewModel _parentViewModel;
    private readonly IDialogService _dialogService;
    private readonly IAttributeSetRepository _attributeSetRepository;
    private readonly IViewModelFactory _viewModelFactory;
    private string _originalId = string.Empty;

    [ObservableProperty]
    private AttributeSet? _attributeSet;

    [ObservableProperty]
    private ObservableCollection<AttributeValueViewModel> _attributeValues = new();

    [ObservableProperty]
    private AttributeValueViewModel? _selectedAttributeValue;

    [ObservableProperty]
    private string? _errorMessage;

    public AttributeSetViewModel(
        string attributeSetId, 
        AttributeDatabaseViewModel parentViewModel, 
        IDialogService dialogService,
        IAttributeSetRepository attributeSetRepository,
        IViewModelFactory viewModelFactory)
    {
        _attributeSetId = attributeSetId;
        _parentViewModel = parentViewModel;
        _dialogService = dialogService;
        _attributeSetRepository = attributeSetRepository;
        _viewModelFactory = viewModelFactory;
        Title = $"Attribute Set: {attributeSetId}";
    }

    public async Task InitializeAsync()
    {
        try
        {
            AttributeSet = await _attributeSetRepository.GetAttributeSetByIdAsync(_attributeSetId);
            if (AttributeSet == null)
            {
                ErrorMessage = $"Attribute set with ID '{_attributeSetId}' not found.";
                return;
            }

            // 保存原始ID用于后续比较
            _originalId = AttributeSet.Id;
            Title = $"Set: {AttributeSet.Name}";

            // Load attribute values with attribute information
            var attributeValueViewModels = await _attributeSetRepository.GetAttributeValueViewModelsAsync(_attributeSetId, RemoveAttributeValueCommand);
            AttributeValues = new ObservableCollection<AttributeValueViewModel>(attributeValueViewModels);
        }
        catch (Exception)
        {
            ErrorMessage = "Failed to load attribute set data.";
            // Optionally, log the exception to a file or analytics service
        }
    }

    [RelayCommand]
    private async Task ChangeId()
    {
        if (AttributeSet == null)
        {
            await _dialogService.ShowMessageBoxAsync("Error", "AttributeSet is not loaded.");
            return;
        }

        var inputVm = _viewModelFactory.CreateInputDialogViewModel(
            "修改属性集ID",
            $"请输入属性集 '{AttributeSet.Name}' 的新ID：",
            AttributeSet.Id);

        var newId = await _dialogService.ShowInputDialogAsync(inputVm);

        if (string.IsNullOrWhiteSpace(newId) || newId == AttributeSet.Id)
        {
            return; // 取消或无变化
        }

        try
        {
            // 检查新ID是否已存在
            var exists = await _attributeSetRepository.AttributeSetExistsAsync(newId);
            if (exists)
            {
                await _dialogService.ShowMessageBoxAsync("Error", $"属性集ID '{newId}' 已存在，请选择其他ID。");
                return;
            }

            // 获取当前属性集的属性值数量用于确认对话框
            var valueCount = AttributeValues.Count;
            
            var confirmVm = _viewModelFactory.CreateConfirmationDialogViewModel(
                "确认修改属性集ID",
                $"修改属性集ID从 '{AttributeSet.Id}' 到 '{newId}' 将会更新 {valueCount} 个属性值的关联。此操作不可撤销。",
                new List<string> { $"属性集: {AttributeSet.Id} ({AttributeSet.Name})" });

            var confirmed = await _dialogService.ShowConfirmationDialogAsync(confirmVm);

            if (confirmed)
            {
                _parentViewModel.MainViewModel.SetLoading(true, "正在修改ID...");
                
                try
                {
                    // 执行ID更新
                    await _attributeSetRepository.UpdateAttributeSetIdAsync(_originalId, newId);
                    
                    // 重新加载AttributeSet以确保UI更新
                    var updatedAttributeSet = await _attributeSetRepository.GetAttributeSetByIdAsync(newId);
                    if (updatedAttributeSet != null)
                    {
                        AttributeSet = updatedAttributeSet;
                        _originalId = newId;
                        Title = $"Set: {AttributeSet.Name}";
                    }
                    
                    await _dialogService.ShowMessageBoxAsync("Success", "属性集ID修改成功。");
                    
                    // 刷新父视图
                    await _parentViewModel.RefreshAttributeSetsAsync();
                    
                    _parentViewModel.MainViewModel.UpdateStatus("ID修改成功");
                }
                catch (Exception idEx)
                {
                    _parentViewModel.MainViewModel.UpdateStatus("ID修改失败");
                    await _dialogService.ShowMessageBoxAsync("Error", $"修改ID时发生错误: {idEx.Message}");
                }
                finally
                {
                    _parentViewModel.MainViewModel.SetLoading(false);
                }
            }
        }
        catch (Exception ex)
        {
            await _dialogService.ShowMessageBoxAsync("Error", $"修改ID时发生错误: {ex.Message}");
        }
    }

    [RelayCommand]
    private async Task SaveAsTemplate()
    {
        Console.WriteLine("[SaveAsTemplate] 开始执行另存为模板操作");
        
        if (AttributeSet == null)
        {
            Console.WriteLine("[SaveAsTemplate] 错误：AttributeSet为null");
            await _dialogService.ShowMessageBoxAsync("Error", "AttributeSet is not loaded.");
            return;
        }
        
        Console.WriteLine($"[SaveAsTemplate] 当前属性集：{AttributeSet.Id} - {AttributeSet.Name}");
        Console.WriteLine($"[SaveAsTemplate] 属性值数量：{AttributeValues.Count}");

        var inputVm = _viewModelFactory.CreateInputDialogViewModel(
            "另存为模板",
            $"请输入模板名称（基于属性集 '{AttributeSet.Name}'）：",
            $"{AttributeSet.Name}_Template");

        var templateName = await _dialogService.ShowInputDialogAsync(inputVm);
        Console.WriteLine($"[SaveAsTemplate] 用户输入的模板名称：'{templateName}'");

        if (string.IsNullOrWhiteSpace(templateName))
        {
            Console.WriteLine("[SaveAsTemplate] 用户取消操作或输入为空");
            return; // 取消操作
        }

        try
        {
            Console.WriteLine("[SaveAsTemplate] 开始创建模板流程");
            _parentViewModel.MainViewModel.SetLoading(true, "正在创建模板...");
            
            // 创建TemplateService实例
            var templateService = new Services.TemplateService(_attributeSetRepository);
            
            // 使用TemplateService创建模板
            var template = await templateService.CreateTemplateFromAttributeSetAsync(
                AttributeSet.Id, 
                templateName, 
                $"基于属性集 '{AttributeSet.Name}' 创建的模板");
            
            Console.WriteLine($"[SaveAsTemplate] 模板创建成功：{template.Id}");
            
            await _dialogService.ShowMessageBoxAsync("Success", $"模板 '{templateName}' 创建成功！\n\n模板文件已保存到：{templateService.GetTemplateFilePath(template.Id)}");
            
            // 刷新父视图以显示新模板
            Console.WriteLine("[SaveAsTemplate] 开始刷新父视图");
            await _parentViewModel.RefreshAttributeSetsAsync();
            Console.WriteLine("[SaveAsTemplate] 父视图刷新完成");
            
            _parentViewModel.MainViewModel.UpdateStatus("模板创建成功");
            Console.WriteLine("[SaveAsTemplate] 模板创建流程完全成功");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[SaveAsTemplate] 创建模板时发生异常：{ex.Message}");
            Console.WriteLine($"[SaveAsTemplate] 异常类型：{ex.GetType().Name}");
            Console.WriteLine($"[SaveAsTemplate] 异常堆栈：{ex.StackTrace}");
            
            if (ex.InnerException != null)
            {
                Console.WriteLine($"[SaveAsTemplate] 内部异常：{ex.InnerException.Message}");
                Console.WriteLine($"[SaveAsTemplate] 内部异常堆栈：{ex.InnerException.StackTrace}");
            }
            
            _parentViewModel.MainViewModel.UpdateStatus("模板创建失败");
            await _dialogService.ShowMessageBoxAsync("Error", $"创建模板时发生错误: {ex.Message}\n\n详细信息请查看控制台日志。");
        }
        finally
        {
            Console.WriteLine("[SaveAsTemplate] 清理资源，关闭加载状态");
            _parentViewModel.MainViewModel.SetLoading(false);
        }
    }

    [RelayCommand]
    private async Task Save()
    {
        if (AttributeSet == null)
        {
            await _dialogService.ShowMessageBoxAsync("Error", "AttributeSet is not loaded.");
            return;
        }

        try
        {
            // 只保存名称和描述，ID通过专门的ChangeId命令修改
            await _attributeSetRepository.SaveAttributeSetChangesAsync(AttributeSet.Id, AttributeSet.Name, AttributeSet.Description);
            await _dialogService.ShowMessageBoxAsync("Success", "Successfully saved changes.");

            // Refresh parent view
            await _parentViewModel.RefreshAttributeSetsAsync();
        }
        catch (Exception ex)
        {
            await _dialogService.ShowMessageBoxAsync("Error", $"An error occurred while saving: {ex.Message}");
        }
    }

    [RelayCommand]
    private async Task AddAttributeValue()
    {
        if (AttributeSet == null) return;

        var excludedIds = AttributeValues.Select(v => v.AttributeValue.AttributeId).ToList();
        var dialogViewModel = _viewModelFactory.CreateSelectAttributeViewModel(excludedIds);

        var selectedAttribute = await _dialogService.ShowSelectAttributeDialogAsync(dialogViewModel);

        if (selectedAttribute != null)
        {
            try
             {
                 if (excludedIds.Contains(selectedAttribute.Id))
                 {
                     await _dialogService.ShowMessageBoxAsync("Error", "This attribute is already in the set.");
                     return;
                 }

                 await _attributeSetRepository.AddAttributeValueAsync(_attributeSetId, selectedAttribute.Id);
                
                // Refresh the list
                await InitializeAsync();
            }
            catch (Exception ex)
            {
                // Improved error logging to show inner exception details
                var errorMessage = $"An error occurred while adding the attribute: {ex.Message}";
                if (ex.InnerException != null)
                {
                    errorMessage += $"\n\nInner Exception:\n{ex.InnerException.Message}";
                }
                await _dialogService.ShowMessageBoxAsync("Error", errorMessage);
            }
        }
    }

    [RelayCommand]
    private void EditAttributeValue(AttributeValueViewModel? attributeValueVm)
    {
        if (attributeValueVm == null) return;
        
        // 设置选中的属性值，这样用户可以在详细视图中编辑
        SelectedAttributeValue = attributeValueVm;
        
        // 可以在这里添加打开详细编辑对话框的逻辑
        // 或者在右侧面板显示编辑表单
    }

    [RelayCommand]
    private async Task RemoveAttributeValue(AttributeValueViewModel? attributeValueVm)
    {
        if (attributeValueVm == null) return;

        var confirmed = await _dialogService.ShowConfirmationDialogAsync(new ConfirmationDialogViewModel
        {
            Title = "Remove Attribute",
            Message = $"Are you sure you want to remove '{attributeValueVm.AttributeName}' from this attribute set?"
        });

        if (!confirmed) return;

        try
        {
            await _attributeSetRepository.RemoveAttributeValueAsync(_attributeSetId, attributeValueVm.AttributeValue.AttributeId);

            // Remove from the UI collection
            AttributeValues.Remove(attributeValueVm);
        }
        catch (Exception ex)
        {
            await _dialogService.ShowMessageBoxAsync("Error", $"An error occurred while removing the attribute: {ex.Message}");
        }
    }
}
