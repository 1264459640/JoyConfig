using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using JoyConfig.Models.AttributeDatabase;
using JoyConfig.Models.Templates;
using JoyConfig.Services;
using JoyConfig.ViewModels.AttributeDatabase;

namespace JoyConfig.ViewModels.Templates;

public partial class TemplateManagerViewModel : ObservableObject
{
    private readonly MainViewModel _mainViewModel;
    private readonly IDialogService _dialogService;
    private readonly ITemplateService _templateService;
    private readonly IViewModelFactory _viewModelFactory;

    [ObservableProperty]
    private ObservableCollection<AttributeSetTemplate> _templates = new();

    [ObservableProperty]
    private string? _errorMessage;

    public TemplateManagerViewModel(
        MainViewModel mainViewModel,
        IDialogService dialogService,
        ITemplateService templateService,
        IViewModelFactory viewModelFactory)
    {
        _mainViewModel = mainViewModel;
        _dialogService = dialogService;
        _templateService = templateService;
        _viewModelFactory = viewModelFactory;
        
        _ = LoadTemplatesAsync();
    }

    private async Task LoadTemplatesAsync()
    {
        try
        {
            Console.WriteLine("[TemplateManagerViewModel] 开始加载模板");
            var templates = await _templateService.GetAllTemplatesAsync();
            Templates = new ObservableCollection<AttributeSetTemplate>(templates);
            ErrorMessage = null;
            Console.WriteLine($"[TemplateManagerViewModel] 成功加载 {templates.Count} 个模板");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[TemplateManagerViewModel] 加载模板时发生错误: {ex.Message}");
            ErrorMessage = $"加载模板时发生错误: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task CreateTemplate()
    {
        Console.WriteLine("[CreateTemplate] 开始执行创建模板操作");
        
        try
        {
            // 输入模板名称
            var nameInputVm = _viewModelFactory.CreateInputDialogViewModel(
                "创建模板",
                "请输入模板名称：",
                "新模板");
            
            var templateName = await _dialogService.ShowInputDialogAsync(nameInputVm);
            Console.WriteLine($"[CreateTemplate] 用户输入的模板名称：'{templateName}'");
            
            if (string.IsNullOrWhiteSpace(templateName))
            {
                Console.WriteLine("[CreateTemplate] 用户取消操作或模板名称为空");
                return; // 取消操作
            }
            
            // 输入模板描述
            var descInputVm = _viewModelFactory.CreateInputDialogViewModel(
                "模板描述",
                "请输入模板描述（可选）：",
                "");
            
            var templateDescription = await _dialogService.ShowInputDialogAsync(descInputVm);
            
            Console.WriteLine("[CreateTemplate] 开始创建空模板");
            _mainViewModel.SetLoading(true, "正在创建模板...");
            
            // 创建空模板
            var template = new AttributeSetTemplate
            {
                Id = $"template_{DateTime.Now:yyyyMMdd_HHmmss}_{Guid.NewGuid().ToString("N")[..8]}",
                Name = templateName,
                Description = string.IsNullOrWhiteSpace(templateDescription) ? "手动创建的模板" : templateDescription,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now,
                Version = "1.0.0",
                Tags = new List<string> { "manual" },
                Attributes = new List<AttributeValueTemplate>()
            };
            
            await _templateService.CreateTemplateAsync(template);
            Console.WriteLine($"[CreateTemplate] 模板创建成功：{template.Id}");
            
            await _dialogService.ShowMessageBoxAsync("Success", $"模板 '{templateName}' 创建成功！");
            
            await LoadTemplatesAsync();
            _mainViewModel.UpdateStatus("模板创建成功");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[CreateTemplate] 创建模板时发生异常：{ex.Message}");
            _mainViewModel.UpdateStatus("模板创建失败");
            await _dialogService.ShowMessageBoxAsync("Error", $"创建模板时发生错误: {ex.Message}");
        }
        finally
        {
            _mainViewModel.SetLoading(false);
        }
    }

    [RelayCommand]
    private async Task CreateFromTemplate()
    {
        if (!Templates.Any())
        {
            await _dialogService.ShowMessageBoxAsync("提示", "当前没有可用的模板。");
            return;
        }
        
        // 创建模板选择对话框
        var templateOptions = Templates.Select(t => $"{t.Name} ({t.Id})").ToList();
        var selectionMessage = "可用模板：\n" + string.Join("\n", templateOptions.Select((opt, i) => $"{i + 1}. {opt}"));
        
        var inputVm = _viewModelFactory.CreateInputDialogViewModel(
            "选择模板",
            $"{selectionMessage}\n\n请输入模板编号（1-{Templates.Count}）：",
            "1");
        
        var selection = await _dialogService.ShowInputDialogAsync(inputVm);
        
        if (string.IsNullOrWhiteSpace(selection) || !int.TryParse(selection, out var templateIndex) || 
            templateIndex < 1 || templateIndex > Templates.Count)
        {
            return; // 取消或无效选择
        }
        
        var selectedTemplate = Templates[templateIndex - 1];
        
        // 输入新属性集的名称
        var nameInputVm = _viewModelFactory.CreateInputDialogViewModel(
            "创建属性集",
            $"基于模板 '{selectedTemplate.Name}' 创建新属性集，请输入名称：",
            selectedTemplate.Name.Replace("Template", "").Replace("_", "").Trim());
        
        var newName = await _dialogService.ShowInputDialogAsync(nameInputVm);
        
        if (string.IsNullOrWhiteSpace(newName))
        {
            return; // 取消操作
        }
        
        // 输入新属性集的ID
        var idInputVm = _viewModelFactory.CreateInputDialogViewModel(
            "创建属性集",
            "请输入新属性集的ID：",
            newName.Replace(" ", "_").ToLower());
        
        var newId = await _dialogService.ShowInputDialogAsync(idInputVm);
        
        if (string.IsNullOrWhiteSpace(newId))
        {
            return; // 取消操作
        }
        
        try
        {
            _mainViewModel.SetLoading(true, "正在创建属性集...");
            
            await _templateService.CreateAttributeSetFromTemplateAsync(selectedTemplate.Id, newId, newName);
            
            await _dialogService.ShowMessageBoxAsync("成功", $"属性集 '{newName}' 创建成功！");
            _mainViewModel.UpdateStatus("属性集创建成功");
            
            // 切换到属性数据库视图并打开新创建的属性集
            _mainViewModel.OpenAttributeDatabase();
            var attributeDatabaseVm = _mainViewModel.CurrentWorkspace as AttributeDatabaseViewModel;
            if (attributeDatabaseVm != null)
            {
                var vm = await _viewModelFactory.CreateAttributeSetViewModelAsync(newId, attributeDatabaseVm);
                _mainViewModel.CurrentEditor = vm;
            }
        }
        catch (Exception ex)
        {
            _mainViewModel.UpdateStatus("属性集创建失败");
            await _dialogService.ShowMessageBoxAsync("错误", $"从模板创建属性集时发生错误: {ex.Message}");
        }
        finally
        {
            _mainViewModel.SetLoading(false);
        }
    }

    [RelayCommand]
    private async Task UseTemplate(AttributeSetTemplate template)
    {
        if (template == null) return;
        
        // 输入新属性集的名称
        var nameInputVm = _viewModelFactory.CreateInputDialogViewModel(
            "使用模板",
            $"使用模板 '{template.Name}' 创建新属性集，请输入名称：",
            template.Name.Replace("Template", "").Replace("_", "").Trim());
        
        var newName = await _dialogService.ShowInputDialogAsync(nameInputVm);
        
        if (string.IsNullOrWhiteSpace(newName))
        {
            return; // 取消操作
        }
        
        // 输入新属性集的ID
        var idInputVm = _viewModelFactory.CreateInputDialogViewModel(
            "使用模板",
            "请输入新属性集的ID：",
            newName.Replace(" ", "_").ToLower());
        
        var newId = await _dialogService.ShowInputDialogAsync(idInputVm);
        
        if (string.IsNullOrWhiteSpace(newId))
        {
            return; // 取消操作
        }
        
        try
        {
            _mainViewModel.SetLoading(true, "正在创建属性集...");
            
            await _templateService.CreateAttributeSetFromTemplateAsync(template.Id, newId, newName);
            
            await _dialogService.ShowMessageBoxAsync("成功", $"属性集 '{newName}' 创建成功！");
            _mainViewModel.UpdateStatus("属性集创建成功");
            
            // 切换到属性数据库视图并打开新创建的属性集
            _mainViewModel.OpenAttributeDatabase();
            var attributeDatabaseVm = _mainViewModel.CurrentWorkspace as AttributeDatabaseViewModel;
            if (attributeDatabaseVm != null)
            {
                var vm = await _viewModelFactory.CreateAttributeSetViewModelAsync(newId, attributeDatabaseVm);
                _mainViewModel.CurrentEditor = vm;
            }
        }
        catch (Exception ex)
        {
            _mainViewModel.UpdateStatus("属性集创建失败");
            await _dialogService.ShowMessageBoxAsync("错误", $"使用模板创建属性集时发生错误: {ex.Message}");
        }
        finally
        {
            _mainViewModel.SetLoading(false);
        }
    }

    [RelayCommand]
    private async Task EditTemplate(AttributeSetTemplate template)
    {
        if (template == null) return;
        
        try
        {
            Console.WriteLine($"[EditTemplate] 开始编辑模板：{template.Id} - {template.Name}");
            
            // 创建模板编辑器ViewModel
            var templateEditorVm = _viewModelFactory.CreateTemplateEditorViewModel(template.Id, _mainViewModel);
            
            // 在编辑器中打开模板
            _mainViewModel.CurrentEditor = templateEditorVm;
            
            Console.WriteLine($"[EditTemplate] 模板编辑器已打开：{template.Id}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[EditTemplate] 打开模板编辑器时发生错误：{ex.Message}");
            await _dialogService.ShowMessageBoxAsync("错误", $"打开模板编辑器时发生错误: {ex.Message}");
        }
    }

    [RelayCommand]
    private async Task DeleteTemplate(AttributeSetTemplate template)
    {
        if (template == null) return;
        
        var confirmVm = _viewModelFactory.CreateConfirmationDialogViewModel(
            "确认删除模板",
            $"确定要删除模板 '{template.Name}' 吗？此操作不可撤销。",
            new List<string> { $"模板: {template.Id} ({template.Name})" });

        var confirmed = await _dialogService.ShowConfirmationDialogAsync(confirmVm);

        if (confirmed)
        {
            try
            {
                _mainViewModel.SetLoading(true, "正在删除模板...");
                await _templateService.DeleteTemplateAsync(template.Id);
                await _dialogService.ShowMessageBoxAsync("成功", $"模板 '{template.Name}' 删除成功。");
                await LoadTemplatesAsync();
                _mainViewModel.UpdateStatus("模板删除成功");
            }
            catch (Exception ex)
            {
                _mainViewModel.UpdateStatus("模板删除失败");
                await _dialogService.ShowMessageBoxAsync("错误", $"删除模板时发生错误: {ex.Message}");
            }
            finally
            {
                _mainViewModel.SetLoading(false);
            }
        }
    }

    [RelayCommand]
    private async Task Refresh()
    {
        await LoadTemplatesAsync();
    }


}