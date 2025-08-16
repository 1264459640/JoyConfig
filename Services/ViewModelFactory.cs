using System.Collections.Generic;
using System.Threading.Tasks;
using JoyConfig.ViewModels;
using JoyConfig.Models.AttributeDatabase;

namespace JoyConfig.Services;

/// <summary>
/// ViewModel工厂实现
/// </summary>
public class ViewModelFactory : IViewModelFactory
{
    private readonly IDialogService _dialogService;
    private readonly IAttributeRepository _attributeRepository;
    private readonly IAttributeSetRepository _attributeSetRepository;
    
    public ViewModelFactory(
        IDialogService dialogService,
        IAttributeRepository attributeRepository,
        IAttributeSetRepository attributeSetRepository)
    {
        _dialogService = dialogService;
        _attributeRepository = attributeRepository;
        _attributeSetRepository = attributeSetRepository;
    }
    
    public MainViewModel CreateMainViewModel()
    {
        return new MainViewModel(_dialogService, this);
    }
    
    public AttributeDatabaseViewModel CreateAttributeDatabaseViewModel(MainViewModel mainViewModel)
    {
        return new AttributeDatabaseViewModel(mainViewModel, _dialogService, _attributeRepository, _attributeSetRepository, this);
    }
    
    public AttributeViewModel CreateAttributeViewModel(Attribute attribute, AttributeDatabaseViewModel parentViewModel)
    {
        return new AttributeViewModel(attribute, parentViewModel, _dialogService, _attributeRepository);
    }
    
    public async Task<AttributeSetViewModel> CreateAttributeSetViewModelAsync(string attributeSetId, AttributeDatabaseViewModel parentViewModel)
    {
        var viewModel = new AttributeSetViewModel(attributeSetId, parentViewModel, _dialogService, _attributeSetRepository, this);
        await viewModel.InitializeAsync();
        return viewModel;
    }
    
    public SettingsViewModel CreateSettingsViewModel(MainViewModel mainViewModel)
    {
        return new SettingsViewModel(_dialogService, mainViewModel);
    }
    
    public TemplateManagerViewModel CreateTemplateManagerViewModel(MainViewModel mainViewModel)
    {
        var templateService = new TemplateService(_attributeSetRepository);
        return new TemplateManagerViewModel(mainViewModel, _dialogService, templateService, this);
    }
    
    public TemplateEditorViewModel CreateTemplateEditorViewModel(string templateId, MainViewModel mainViewModel)
    {
        var templateService = new TemplateService(_attributeSetRepository);
        return new TemplateEditorViewModel(templateId, templateService, _dialogService, _attributeRepository, this, mainViewModel);
    }
    
    public TemplateWorkspaceViewModel CreateTemplateWorkspaceViewModel(MainViewModel mainViewModel)
    {
        var templateService = new TemplateService(_attributeSetRepository);
        return new TemplateWorkspaceViewModel(mainViewModel, _dialogService, templateService, this);
    }
    
    public CategoryViewModel CreateCategoryViewModel(AttributeDatabaseViewModel parentViewModel)
    {
        return new CategoryViewModel(parentViewModel);
    }
    
    public SelectAttributeViewModel CreateSelectAttributeViewModel(IEnumerable<string> excludedAttributeIds)
    {
        return new SelectAttributeViewModel(_attributeRepository, excludedAttributeIds);
    }
    
    public ConfirmationDialogViewModel CreateConfirmationDialogViewModel(string title, string message)
    {
        return new ConfirmationDialogViewModel
        {
            Title = title,
            Message = message
        };
    }
    
    public ConfirmationDialogViewModel CreateConfirmationDialogViewModel(string title, string message, List<string> details)
    {
        return new ConfirmationDialogViewModel
        {
            Title = title,
            Message = message,
            Details = details
        };
    }
    
    public InputDialogViewModel CreateInputDialogViewModel(string title, string prompt, string defaultValue = "")
    {
        return new InputDialogViewModel
        {
            Title = title,
            Message = prompt,
            InputText = defaultValue
        };
    }
}