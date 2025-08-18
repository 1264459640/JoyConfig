using JoyConfig.ViewModels;
using JoyConfig.ViewModels.AttributeDatabase;
using JoyConfig.ViewModels.GameplayEffectDatabase;
using JoyConfig.ViewModels.Dialogs;
using JoyConfig.ViewModels.Templates;
using JoyConfig.ViewModels.Settings;
using JoyConfig.Models.AttributeDatabase;
using JoyConfig.Models.GameplayEffectDatabase;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace JoyConfig.Services;

/// <summary>
/// ViewModel工厂接口
/// </summary>
public interface IViewModelFactory
{
    /// <summary>
    /// 创建主ViewModel
    /// </summary>
    MainViewModel CreateMainViewModel();

    /// <summary>
    /// 创建属性数据库ViewModel
    /// </summary>
    AttributeDatabaseViewModel CreateAttributeDatabaseViewModel(MainViewModel mainViewModel);

    /// <summary>
    /// 创建游戏效果数据库ViewModel
    /// </summary>
    GameplayEffectDatabaseViewModel CreateGameplayEffectDatabaseViewModel(MainViewModel mainViewModel);

    /// <summary>
    /// 创建游戏效果ViewModel
    /// </summary>
    GameplayEffectViewModel CreateGameplayEffectViewModel(AttributeEffect effect, GameplayEffectDatabaseViewModel parentViewModel);

    /// <summary>
    /// 创建属性ViewModel
    /// </summary>
    AttributeViewModel CreateAttributeViewModel(Attribute attribute, AttributeDatabaseViewModel parentViewModel);

    /// <summary>
    /// 创建属性集ViewModel
    /// </summary>
    Task<AttributeSetViewModel> CreateAttributeSetViewModelAsync(string attributeSetId, AttributeDatabaseViewModel parentViewModel);

    /// <summary>
    /// 创建设置ViewModel
    /// </summary>
    SettingsViewModel CreateSettingsViewModel(MainViewModel mainViewModel);

    /// <summary>
    /// 创建模板管理ViewModel
    /// </summary>
    TemplateManagerViewModel CreateTemplateManagerViewModel(MainViewModel mainViewModel);

    /// <summary>
    /// 创建模板编辑器ViewModel
    /// </summary>
    TemplateEditorViewModel CreateTemplateEditorViewModel(string templateId, MainViewModel mainViewModel);

    /// <summary>
    /// 创建模板工作区ViewModel
    /// </summary>
    TemplateWorkspaceViewModel CreateTemplateWorkspaceViewModel(MainViewModel mainViewModel);

    /// <summary>
    /// 创建分类ViewModel
    /// </summary>
    CategoryViewModel CreateCategoryViewModel(AttributeDatabaseViewModel parentViewModel);

    /// <summary>
    /// 创建选择属性ViewModel
    /// </summary>
    SelectAttributeViewModel CreateSelectAttributeViewModel(IEnumerable<string> excludedAttributeIds);

    /// <summary>
    /// 创建确认对话框ViewModel
    /// </summary>
    ConfirmationDialogViewModel CreateConfirmationDialogViewModel(string title, string message);

    /// <summary>
    /// 创建确认对话框ViewModel（带详细信息）
    /// </summary>
    ConfirmationDialogViewModel CreateConfirmationDialogViewModel(string title, string message, List<string> details);

    /// <summary>
    /// 创建输入对话框ViewModel
    /// </summary>
    InputDialogViewModel CreateInputDialogViewModel(string title, string prompt, string defaultValue = "");
}