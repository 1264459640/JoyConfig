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
using JoyConfig.ViewModels.Base;

namespace JoyConfig.ViewModels.Templates;

public partial class TemplateEditorViewModel : EditorViewModelBase
{
    private readonly ITemplateService _templateService;
    private readonly IDialogService _dialogService;
    private readonly IAttributeRepository _attributeRepository;
    private readonly IViewModelFactory _viewModelFactory;
    private readonly MainViewModel _mainViewModel;
    private readonly string _templateId;

    [ObservableProperty]
    private AttributeSetTemplate? _template;

    [ObservableProperty]
    private string _templateName = "";

    [ObservableProperty]
    private string _templateDescription = "";

    [ObservableProperty]
    private string _templateVersion = "1.0.0";

    [ObservableProperty]
    private ObservableCollection<AttributeValueTemplateViewModel> _attributes = new();

    [ObservableProperty]
    private ObservableCollection<JoyConfig.Models.AttributeDatabase.Attribute> _availableAttributes = new();

    [ObservableProperty]
    private string? _errorMessage;

    [ObservableProperty]
    private bool _isModified;

    public TemplateEditorViewModel(
        string templateId,
        ITemplateService templateService,
        IDialogService dialogService,
        IAttributeRepository attributeRepository,
        IViewModelFactory viewModelFactory,
        MainViewModel mainViewModel)
    {
        _templateId = templateId;
        _templateService = templateService;
        _dialogService = dialogService;
        _attributeRepository = attributeRepository;
        _viewModelFactory = viewModelFactory;
        _mainViewModel = mainViewModel;

        Title = "模板编辑器";

        _ = InitializeAsync();
    }

    private async Task InitializeAsync()
    {
        try
        {
            // 加载模板数据
            Template = await _templateService.GetTemplateByIdAsync(_templateId);
            if (Template == null)
            {
                ErrorMessage = $"模板 '{_templateId}' 不存在";
                return;
            }

            // 设置基本信息
            TemplateName = Template.Name;
            TemplateDescription = Template.Description ?? "";
            TemplateVersion = Template.Version;
            Title = $"模板: {Template.Name}";

            // 加载属性数据
            var allAttributes = await _attributeRepository.GetAllAttributesAsync();
            AvailableAttributes = new ObservableCollection<JoyConfig.Models.AttributeDatabase.Attribute>(allAttributes);

            // 转换模板属性为ViewModel
            var attributeViewModels = Template.Attributes.Select(attr =>
                new AttributeValueTemplateViewModel(attr, this)).ToList();
            Attributes = new ObservableCollection<AttributeValueTemplateViewModel>(attributeViewModels);

            IsModified = false;
        }
        catch (Exception ex)
        {
            ErrorMessage = $"加载模板时发生错误: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task Save()
    {
        if (Template == null) return;

        try
        {
            // 更新模板数据
            Template.Name = TemplateName;
            Template.Description = TemplateDescription;
            Template.Version = TemplateVersion;
            Template.Attributes = Attributes.Select(vm => vm.ToAttributeValueTemplate()).ToList();

            await _templateService.UpdateTemplateAsync(Template);

            IsModified = false;
            ErrorMessage = null;

            await _dialogService.ShowMessageBoxAsync("成功", "模板保存成功！");
            _mainViewModel.UpdateStatus("模板保存成功");
        }
        catch (Exception ex)
        {
            ErrorMessage = $"保存模板时发生错误: {ex.Message}";
            await _dialogService.ShowMessageBoxAsync("错误", ErrorMessage);
        }
    }

    [RelayCommand]
    private async Task AddAttribute()
    {
        try
        {
            // 获取未使用的属性
            var usedAttributeIds = Attributes.Select(a => a.AttributeId).ToHashSet();
            var availableForSelection = AvailableAttributes
                .Where(a => !usedAttributeIds.Contains(a.Id))
                .ToList();

            if (!availableForSelection.Any())
            {
                await _dialogService.ShowMessageBoxAsync("提示", "所有属性都已添加到模板中。");
                return;
            }

            // 创建选择对话框
            var selectVm = _viewModelFactory.CreateSelectAttributeViewModel(usedAttributeIds);
            var selectedAttribute = await _dialogService.ShowSelectAttributeDialogAsync(selectVm);

            if (selectedAttribute != null)
            {
                var newAttributeTemplate = new AttributeValueTemplate
                {
                    Id = selectedAttribute.Id,
                    Category = selectedAttribute.Category,
                    BaseValue = 0,
                    MinValue = 0,
                    MaxValue = 100
                };

                var viewModel = new AttributeValueTemplateViewModel(newAttributeTemplate, this);
                Attributes.Add(viewModel);
                MarkAsModified();
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"添加属性时发生错误: {ex.Message}";
        }
    }

    [RelayCommand]
    private void RemoveAttribute(AttributeValueTemplateViewModel attributeViewModel)
    {
        if (attributeViewModel != null)
        {
            Attributes.Remove(attributeViewModel);
            MarkAsModified();
        }
    }

    public void MarkAsModified()
    {
        IsModified = true;
    }

    partial void OnTemplateNameChanged(string value)
    {
        MarkAsModified();
    }

    partial void OnTemplateDescriptionChanged(string value)
    {
        MarkAsModified();
    }

    partial void OnTemplateVersionChanged(string value)
    {
        MarkAsModified();
    }
}

/// <summary>
/// 模板属性值的ViewModel
/// </summary>
public partial class AttributeValueTemplateViewModel : ObservableObject
{
    private readonly TemplateEditorViewModel _parentViewModel;

    [ObservableProperty]
    private string _attributeId = "";

    [ObservableProperty]
    private string _attributeCategory = "";

    [ObservableProperty]
    private double _baseValue;

    [ObservableProperty]
    private double _minValue;

    [ObservableProperty]
    private double _maxValue;

    public AttributeValueTemplateViewModel(AttributeValueTemplate template, TemplateEditorViewModel parentViewModel)
    {
        _parentViewModel = parentViewModel;
        AttributeId = template.Id;
        AttributeCategory = template.Category;
        BaseValue = template.BaseValue;
        MinValue = template.MinValue;
        MaxValue = template.MaxValue;
    }

    public AttributeValueTemplate ToAttributeValueTemplate()
    {
        return new AttributeValueTemplate
        {
            Id = AttributeId,
            Category = AttributeCategory,
            BaseValue = BaseValue,
            MinValue = MinValue,
            MaxValue = MaxValue
        };
    }

    partial void OnBaseValueChanged(double value)
    {
        _parentViewModel.MarkAsModified();
    }

    partial void OnMinValueChanged(double value)
    {
        _parentViewModel.MarkAsModified();
    }

    partial void OnMaxValueChanged(double value)
    {
        _parentViewModel.MarkAsModified();
    }
}