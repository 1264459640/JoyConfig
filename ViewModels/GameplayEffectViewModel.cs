using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using JoyConfig.Models.GameplayEffectDatabase;
using JoyConfig.Services;
using Avalonia.Threading;
using Microsoft.EntityFrameworkCore;

namespace JoyConfig.ViewModels;

/// <summary>
/// 游戏效果编辑器视图模型
/// </summary>
public partial class GameplayEffectViewModel : EditorViewModelBase
{
    private readonly GameplayEffectDatabaseViewModel _parentViewModel;
    private readonly IDialogService _dialogService;
    private readonly IDbContextFactory _dbContextFactory;
    private readonly IViewModelFactory _viewModelFactory;
    private readonly string _originalId;
    private bool _hasUnsavedChanges;

    public GameplayEffectViewModel(
        AttributeEffect effect,
        GameplayEffectDatabaseViewModel parentViewModel,
        IDialogService dialogService,
        IDbContextFactory dbContextFactory,
        IViewModelFactory viewModelFactory)
    {
        _parentViewModel = parentViewModel;
        _dialogService = dialogService;
        _dbContextFactory = dbContextFactory;
        _viewModelFactory = viewModelFactory;
        _originalId = effect.Id;
        
        // 复制效果数据
        Id = effect.Id;
        Name = effect.Name;
        Description = effect.Description ?? string.Empty;
        EffectType = effect.EffectType;
        StackingType = effect.StackingType;
        Tags = effect.Tags ?? string.Empty;
        DurationSeconds = effect.DurationSeconds ?? 0;
        IsInfinite = effect.IsInfinite ?? false;
        MaxStacks = effect.MaxStacks ?? 1;
        IsPassive = effect.IsPassive ?? false;
        Priority = effect.Priority ?? 0;
        IsPeriodic = effect.IsPeriodic ?? false;
        IntervalSeconds = effect.IntervalSeconds ?? 1.0;
        SourceType = effect.SourceType ?? string.Empty;
        
        Title = $"效果: {effect.Name}";
        
        // 加载属性修改器
        _ = LoadAttributeModifiersAsync();
    }

    [ObservableProperty]
    private string _id = string.Empty;

    [ObservableProperty]
    private string _name = string.Empty;

    [ObservableProperty]
    private string _description = string.Empty;

    [ObservableProperty]
    private string _effectType = EffectTypes.Instant;

    [ObservableProperty]
    private string _stackingType = StackingTypes.NoStack;

    [ObservableProperty]
    private string _tags = string.Empty;

    [ObservableProperty]
    private double _durationSeconds;

    [ObservableProperty]
    private bool _isInfinite;

    [ObservableProperty]
    private int _maxStacks = 1;

    [ObservableProperty]
    private bool _isPassive;

    [ObservableProperty]
    private int _priority;

    [ObservableProperty]
    private bool _isPeriodic;

    [ObservableProperty]
    private double _intervalSeconds = 1.0;

    [ObservableProperty]
    private string _sourceType = string.Empty;

    [ObservableProperty]
    private ObservableCollection<AttributeModifierViewModel> _attributeModifiers = new();

    [ObservableProperty]
    private AttributeModifierViewModel? _selectedAttributeModifier;

    [ObservableProperty]
    private string? _errorMessage;

    [ObservableProperty]
    private bool _isLoading;

    /// <summary>
    /// 效果类型选项
    /// </summary>
    public string[] EffectTypeOptions => EffectTypes.All;

    /// <summary>
    /// 堆叠类型选项
    /// </summary>
    public string[] StackingTypeOptions => StackingTypes.All;

    /// <summary>
    /// 来源类型选项
    /// </summary>
    public string[] SourceTypeOptions => SourceTypes.All;

    /// <summary>
    /// 标签类型选项
    /// </summary>
    public string[] TagTypeOptions => TagTypes.All;

    /// <summary>
    /// 是否显示持续时间字段
    /// </summary>
    public bool ShowDurationFields => EffectType == EffectTypes.Duration;

    /// <summary>
    /// 是否显示周期性字段
    /// </summary>
    public bool ShowPeriodicFields => IsPeriodic;

    /// <summary>
    /// 是否显示堆叠字段
    /// </summary>
    public bool ShowStackingFields => StackingType == StackingTypes.Stack;

    /// <summary>
    /// 是否有未保存的更改
    /// </summary>
    public bool HasUnsavedChanges
    {
        get => _hasUnsavedChanges;
        set
        {
            if (SetProperty(ref _hasUnsavedChanges, value))
            {
                Title = value ? $"效果: {Name} *" : $"效果: {Name}";
            }
        }
    }

    partial void OnEffectTypeChanged(string value)
    {
        OnPropertyChanged(nameof(ShowDurationFields));
        HasUnsavedChanges = true;
    }

    partial void OnStackingTypeChanged(string value)
    {
        OnPropertyChanged(nameof(ShowStackingFields));
        HasUnsavedChanges = true;
    }

    partial void OnIsPeriodicChanged(bool value)
    {
        OnPropertyChanged(nameof(ShowPeriodicFields));
        HasUnsavedChanges = true;
    }

    partial void OnIdChanged(string value) => HasUnsavedChanges = true;
    partial void OnNameChanged(string value) => HasUnsavedChanges = true;
    partial void OnDescriptionChanged(string value) => HasUnsavedChanges = true;
    partial void OnTagsChanged(string value) => HasUnsavedChanges = true;
    partial void OnDurationSecondsChanged(double value) => HasUnsavedChanges = true;
    partial void OnIsInfiniteChanged(bool value) => HasUnsavedChanges = true;
    partial void OnMaxStacksChanged(int value) => HasUnsavedChanges = true;
    partial void OnIsPassiveChanged(bool value) => HasUnsavedChanges = true;
    partial void OnPriorityChanged(int value) => HasUnsavedChanges = true;
    partial void OnIntervalSecondsChanged(double value) => HasUnsavedChanges = true;
    partial void OnSourceTypeChanged(string value) => HasUnsavedChanges = true;

    /// <summary>
    /// 加载属性修改器
    /// </summary>
    [RelayCommand]
    public async Task LoadAttributeModifiersAsync()
    {
        try
        {
            IsLoading = true;
            ErrorMessage = null;

            using var context = _dbContextFactory.CreateGameplayEffectDatabaseContext();
            var modifiers = await context.AttributeModifiers
                .Where(m => m.EffectId == _originalId)
                .OrderBy(m => m.ExecutionOrder)
                .ThenBy(m => m.Id)
                .ToListAsync();

            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                AttributeModifiers.Clear();
                foreach (var modifier in modifiers)
                {
                    var viewModel = new AttributeModifierViewModel(modifier, this, _dialogService, _dbContextFactory);
                    AttributeModifiers.Add(viewModel);
                }
            });
        }
        catch (Exception ex)
        {
            ErrorMessage = $"加载属性修改器失败: {ex.Message}";
            await _dialogService.ShowErrorAsync("加载失败", ex.Message);
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// 保存效果
    /// </summary>
    [RelayCommand]
    public async Task SaveAsync()
    {
        try
        {
            // 验证数据
            if (string.IsNullOrWhiteSpace(Id))
            {
                await _dialogService.ShowErrorAsync("验证失败", "效果ID不能为空。");
                return;
            }

            if (string.IsNullOrWhiteSpace(Name))
            {
                await _dialogService.ShowErrorAsync("验证失败", "效果名称不能为空。");
                return;
            }

            IsLoading = true;
            ErrorMessage = null;

            using var context = _dbContextFactory.CreateGameplayEffectDatabaseContext();
            
            // 检查ID是否已被其他效果使用
            if (Id != _originalId)
            {
                var existingEffect = await context.AttributeEffects
                    .FirstOrDefaultAsync(e => e.Id == Id);
                if (existingEffect != null)
                {
                    await _dialogService.ShowErrorAsync("保存失败", "该ID已被其他效果使用，请使用不同的ID。");
                    return;
                }
            }

            var effect = await context.AttributeEffects
                .FirstOrDefaultAsync(e => e.Id == _originalId);

            if (effect == null)
            {
                await _dialogService.ShowErrorAsync("保存失败", "找不到要更新的效果。");
                return;
            }

            // 更新效果数据
            effect.Id = Id;
            effect.Name = Name;
            effect.Description = string.IsNullOrWhiteSpace(Description) ? null : Description;
            effect.EffectType = EffectType;
            effect.StackingType = StackingType;
            effect.Tags = string.IsNullOrWhiteSpace(Tags) ? null : Tags;
            effect.DurationSeconds = ShowDurationFields ? DurationSeconds : null;
            effect.IsInfinite = IsInfinite;
            effect.MaxStacks = ShowStackingFields ? MaxStacks : null;
            effect.IsPassive = IsPassive;
            effect.Priority = Priority;
            effect.IsPeriodic = IsPeriodic;
            effect.IntervalSeconds = ShowPeriodicFields ? IntervalSeconds : 1.0;
            effect.SourceType = string.IsNullOrWhiteSpace(SourceType) ? null : SourceType;

            await context.SaveChangesAsync();

            HasUnsavedChanges = false;
            await _dialogService.ShowInfoAsync("保存成功", "效果已成功保存。");
            
            // 刷新父视图模型的数据
            await _parentViewModel.LoadDataAsync();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"保存失败: {ex.Message}";
            await _dialogService.ShowErrorAsync("保存失败", ex.Message);
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// 删除效果
    /// </summary>
    [RelayCommand]
    public async Task DeleteAsync()
    {
        try
        {
            var modifierCount = AttributeModifiers.Count;
            var message = modifierCount > 0 
                ? $"确定要删除效果 '{Name}' 吗？\n\n此操作将同时删除 {modifierCount} 个关联的属性修改器，且无法撤销。"
                : $"确定要删除效果 '{Name}' 吗？\n\n此操作无法撤销。";

            var confirmed = await _dialogService.ShowConfirmationAsync("确认删除", message);
            if (!confirmed) return;

            IsLoading = true;
            ErrorMessage = null;

            using var context = _dbContextFactory.CreateGameplayEffectDatabaseContext();
            var effect = await context.AttributeEffects
                .Include(e => e.AttributeModifiers)
                .FirstOrDefaultAsync(e => e.Id == _originalId);

            if (effect != null)
            {
                context.AttributeEffects.Remove(effect);
                await context.SaveChangesAsync();
            }

            await _dialogService.ShowInfoAsync("删除成功", "效果已成功删除。");
            
            // 刷新父视图模型的数据
            await _parentViewModel.LoadDataAsync();
            
            // 关闭当前编辑器
            _parentViewModel.MainViewModel.CurrentEditor = new WelcomeViewModel();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"删除失败: {ex.Message}";
            await _dialogService.ShowErrorAsync("删除失败", ex.Message);
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// 添加属性修改器
    /// </summary>
    [RelayCommand]
    public async Task AddAttributeModifierAsync()
    {
        try
        {
            // TODO: 实现选择属性类型的对话框
            var attributeType = await _dialogService.ShowInputAsync("添加修改器", "请输入属性类型:", "");
            if (string.IsNullOrWhiteSpace(attributeType))
                return;

            var newModifier = new AttributeModifier
            {
                EffectId = _originalId,
                AttributeType = attributeType,
                OperationType = OperationTypes.Add,
                Value = 0,
                ExecutionOrder = AttributeModifiers.Count
            };

            using var context = _dbContextFactory.CreateGameplayEffectDatabaseContext();
            context.AttributeModifiers.Add(newModifier);
            await context.SaveChangesAsync();

            // 刷新修改器列表
            await LoadAttributeModifiersAsync();
        }
        catch (Exception ex)
        {
            await _dialogService.ShowErrorAsync("添加失败", ex.Message);
        }
    }

    /// <summary>
    /// 删除属性修改器
    /// </summary>
    [RelayCommand]
    public async Task DeleteAttributeModifierAsync(AttributeModifierViewModel? modifier)
    {
        if (modifier == null) return;

        try
        {
            var confirmed = await _dialogService.ShowConfirmationAsync(
                "确认删除", 
                $"确定要删除属性修改器 '{modifier.AttributeType}' 吗？\n\n此操作无法撤销。");
            if (!confirmed) return;

            using var context = _dbContextFactory.CreateGameplayEffectDatabaseContext();
            var modifierToDelete = await context.AttributeModifiers
                .FirstOrDefaultAsync(m => m.Id == modifier.Id);

            if (modifierToDelete != null)
            {
                context.AttributeModifiers.Remove(modifierToDelete);
                await context.SaveChangesAsync();
            }

            // 刷新修改器列表
            await LoadAttributeModifiersAsync();
        }
        catch (Exception ex)
        {
            await _dialogService.ShowErrorAsync("删除失败", ex.Message);
        }
    }
}