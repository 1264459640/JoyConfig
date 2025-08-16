using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using JoyConfig.Models.Templates;
using JoyConfig.Services;
using Avalonia.Threading;

namespace JoyConfig.ViewModels;

public partial class TemplateWorkspaceViewModel : EditorViewModelBase
{
    public MainViewModel MainViewModel { get; }
    private readonly IDialogService _dialogService;
    private readonly ITemplateService _templateService;
    private readonly IViewModelFactory _viewModelFactory;

    public TemplateWorkspaceViewModel(
        MainViewModel mainViewModel,
        IDialogService dialogService,
        ITemplateService templateService,
        IViewModelFactory viewModelFactory)
    {
        MainViewModel = mainViewModel;
        _dialogService = dialogService;
        _templateService = templateService;
        _viewModelFactory = viewModelFactory;
        
        Title = "模板管理";
        _ = LoadDataAsync();
    }

    [ObservableProperty]
    private ObservableCollection<AttributeSetTemplate> _templates = new();

    [ObservableProperty]
    private string? _errorMessage;

    [ObservableProperty]
    private AttributeSetTemplate? _selectedTemplate;

    partial void OnSelectedTemplateChanged(AttributeSetTemplate? value)
    {
        if (value is not null)
        {
            OpenEditorCommand.Execute(value);
        }
    }

    [RelayCommand]
    public async Task OpenEditorAsync(object? item)
    {
        if (item is null) return;

        if (item is AttributeSetTemplate template)
        {
            try
            {
                Console.WriteLine($"[TemplateWorkspaceViewModel] 打开模板编辑器：{template.Id} - {template.Name}");
                
                // 创建模板编辑器ViewModel
                var templateEditorVm = _viewModelFactory.CreateTemplateEditorViewModel(template.Id, MainViewModel);
                
                // 在编辑器中打开模板
                MainViewModel.CurrentEditor = templateEditorVm;
                
                Console.WriteLine($"[TemplateWorkspaceViewModel] 模板编辑器已打开：{template.Id}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[TemplateWorkspaceViewModel] 打开模板编辑器时发生错误：{ex.Message}");
                await _dialogService.ShowMessageBoxAsync("错误", $"打开模板编辑器时发生错误: {ex.Message}");
            }
        }
    }

    [RelayCommand]
    public async Task LoadDataAsync()
    {
        await Dispatcher.UIThread.InvokeAsync(() => ErrorMessage = "加载中...");
        try
        {
            Console.WriteLine("[TemplateWorkspaceViewModel] 开始加载模板数据");
            var templates = await _templateService.GetAllTemplatesAsync();
            
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                Templates.Clear();
                foreach (var template in templates)
                {
                    Templates.Add(template);
                }
                ErrorMessage = null;
                Console.WriteLine($"[TemplateWorkspaceViewModel] 成功加载 {templates.Count} 个模板");
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[TemplateWorkspaceViewModel] 加载模板数据时发生错误: {ex.Message}");
            await Dispatcher.UIThread.InvokeAsync(() => ErrorMessage = $"加载模板时发生错误: {ex.Message}");
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
                return; // 取消操作
            }
            
            // 输入模板描述（可选）
            var descInputVm = _viewModelFactory.CreateInputDialogViewModel(
                "模板描述",
                "请输入模板描述（可选）：",
                "手动创建的模板");
            
            var templateDescription = await _dialogService.ShowInputDialogAsync(descInputVm);
            
            Console.WriteLine("[CreateTemplate] 开始创建空模板");
            MainViewModel.SetLoading(true, "正在创建模板...");
            
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
            
            await _dialogService.ShowMessageBoxAsync("成功", $"模板 '{templateName}' 创建成功！");
            
            await LoadDataAsync();
            MainViewModel.UpdateStatus("模板创建成功");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[CreateTemplate] 创建模板时发生异常：{ex.Message}");
            MainViewModel.UpdateStatus("模板创建失败");
            await _dialogService.ShowMessageBoxAsync("错误", $"创建模板时发生错误: {ex.Message}");
        }
        finally
        {
            MainViewModel.SetLoading(false);
        }
    }

    [RelayCommand]
    private async Task EditTemplate(AttributeSetTemplate? template)
    {
        if (template == null) return;
        
        try
        {
            Console.WriteLine($"[EditTemplate] 开始编辑模板：{template.Id} - {template.Name}");
            
            // 创建模板编辑器ViewModel
            var templateEditorVm = _viewModelFactory.CreateTemplateEditorViewModel(template.Id, MainViewModel);
            
            // 在编辑器中打开模板
            MainViewModel.CurrentEditor = templateEditorVm;
            
            Console.WriteLine($"[EditTemplate] 模板编辑器已打开：{template.Id}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[EditTemplate] 打开模板编辑器时发生错误：{ex.Message}");
            await _dialogService.ShowMessageBoxAsync("错误", $"打开模板编辑器时发生错误: {ex.Message}");
        }
    }

    [RelayCommand]
    private async Task UseTemplate(AttributeSetTemplate? template)
    {
        if (template == null) return;
        
        try
        {
            // 输入新属性集的名称
            var nameInputVm = _viewModelFactory.CreateInputDialogViewModel(
                "创建属性集",
                $"基于模板 '{template.Name}' 创建新属性集，请输入名称：",
                template.Name.Replace("Template", "").Replace("_", "").Trim());
            
            var newName = await _dialogService.ShowInputDialogAsync(nameInputVm);
            
            if (string.IsNullOrWhiteSpace(newName))
            {
                return; // 取消操作
            }
            
            // 输入新属性集的ID
            var idInputVm = _viewModelFactory.CreateInputDialogViewModel(
                "属性集ID",
                "请输入新属性集的ID：",
                newName.ToLower().Replace(" ", "_"));
            
            var newId = await _dialogService.ShowInputDialogAsync(idInputVm);
            
            if (string.IsNullOrWhiteSpace(newId))
            {
                return; // 取消操作
            }
            
            MainViewModel.SetLoading(true, "正在从模板创建属性集...");
            
            await _templateService.CreateAttributeSetFromTemplateAsync(template.Id, newId, newName);
            
            await _dialogService.ShowMessageBoxAsync("成功", $"属性集 '{newName}' 创建成功！");
            MainViewModel.UpdateStatus("属性集创建成功");
            
            // 切换到属性数据库视图
            MainViewModel.OpenAttributeDatabase();
        }
        catch (Exception ex)
        {
            MainViewModel.UpdateStatus("属性集创建失败");
            await _dialogService.ShowMessageBoxAsync("错误", $"使用模板创建属性集时发生错误: {ex.Message}");
        }
        finally
        {
            MainViewModel.SetLoading(false);
        }
    }

    [RelayCommand]
    private async Task DeleteTemplate(AttributeSetTemplate? template)
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
                MainViewModel.SetLoading(true, "正在删除模板...");
                await _templateService.DeleteTemplateAsync(template.Id);
                await _dialogService.ShowMessageBoxAsync("成功", $"模板 '{template.Name}' 删除成功。");
                await LoadDataAsync();
                MainViewModel.UpdateStatus("模板删除成功");
            }
            catch (Exception ex)
            {
                MainViewModel.UpdateStatus("模板删除失败");
                await _dialogService.ShowMessageBoxAsync("错误", $"删除模板时发生错误: {ex.Message}");
            }
            finally
            {
                MainViewModel.SetLoading(false);
            }
        }
    }

    [RelayCommand]
    private async Task Refresh()
    {
        await LoadDataAsync();
    }
}