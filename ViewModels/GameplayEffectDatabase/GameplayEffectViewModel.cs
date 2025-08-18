using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using JoyConfig.Models.GameplayEffectDatabase;
using JoyConfig.Services;
using JoyConfig.ViewModels.Base;
using Microsoft.EntityFrameworkCore;
using Avalonia.Threading;
using System.Collections.Specialized;

namespace JoyConfig.ViewModels.GameplayEffectDatabase;

/// <summary>
/// 游戏效果编辑器视图模型
/// </summary>
public partial class GameplayEffectViewModel : EditorViewModelBase
{
    private readonly GameplayEffectDatabaseViewModel _parentViewModel;
    private readonly IDialogService _dialogService;
    private readonly IDbContextFactory _dbContextFactory;
    private readonly IAttributeTypeService _attributeTypeService;
    private readonly IViewModelFactory _viewModelFactory;
    private readonly string _originalId;
    private bool _hasUnsavedChanges;

    public GameplayEffectViewModel(
        AttributeEffect effect,
        GameplayEffectDatabaseViewModel parentViewModel,
        IDialogService dialogService,
        IDbContextFactory dbContextFactory,
        IAttributeTypeService attributeTypeService,
        IViewModelFactory viewModelFactory)
    {
        _parentViewModel = parentViewModel;
        _dialogService = dialogService;
        _dbContextFactory = dbContextFactory;
        _attributeTypeService = attributeTypeService;
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
        
        // 确保属性类型服务已初始化
        _ = _attributeTypeService.InitializeAsync();
        
        System.Diagnostics.Debug.WriteLine($"[DEBUG] GameplayEffectViewModel构造完成，Id: {Id}, OriginalId: {_originalId}");
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
    /// 操作类型选项
    /// </summary>
    public string[] OperationTypeOptions => OperationTypes.All;

    /// <summary>
    /// 可用的属性类型列表（来自全局服务）
    /// </summary>
    public ReadOnlyObservableCollection<string> AvailableAttributeTypes => _attributeTypeService.AvailableAttributeTypes;

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
                
                // 引导式级联更新：检查关联的AttributeModifiers
                var affectedModifiers = await context.AttributeModifiers
                    .Where(m => m.EffectId == _originalId)
                    .ToListAsync();
                    
                if (affectedModifiers.Count > 0)
                {
                    var message = $"更改效果ID将影响 {affectedModifiers.Count} 个关联的属性修改器。\n\n" +
                                 "系统将删除旧效果并创建新效果，同时重新创建所有修改器。\n\n" +
                                 "确定要继续吗？";
                    
                    var confirmed = await _dialogService.ShowConfirmationAsync("确认ID更改", message);
                    if (!confirmed) return;
                }
            }

            if (Id != _originalId)
            {
                // ID发生变化时，需要删除旧实体并创建新实体
                var oldEffect = await context.AttributeEffects
                    .Include(e => e.AttributeModifiers)
                    .FirstOrDefaultAsync(e => e.Id == _originalId);

                if (oldEffect == null)
                {
                    await _dialogService.ShowErrorAsync("保存失败", "找不到要更新的效果。");
                    return;
                }

                // 创建新效果实体
                var newEffect = new AttributeEffect
                {
                    Id = Id,
                    Name = Name,
                    Description = string.IsNullOrWhiteSpace(Description) ? null : Description,
                    EffectType = EffectType,
                    StackingType = StackingType,
                    Tags = string.IsNullOrWhiteSpace(Tags) ? null : Tags,
                    DurationSeconds = ShowDurationFields ? DurationSeconds : null,
                    IsInfinite = IsInfinite,
                    MaxStacks = ShowStackingFields ? MaxStacks : null,
                    IsPassive = IsPassive,
                    Priority = Priority,
                    IsPeriodic = IsPeriodic,
                    IntervalSeconds = ShowPeriodicFields ? IntervalSeconds : 1.0,
                    SourceType = string.IsNullOrWhiteSpace(SourceType) ? null : SourceType
                };

                // 重新创建所有属性修改器
                foreach (var oldModifier in oldEffect.AttributeModifiers)
                {
                    var newModifier = new AttributeModifier
                    {
                        EffectId = Id,
                        AttributeType = oldModifier.AttributeType,
                        OperationType = oldModifier.OperationType,
                        Value = oldModifier.Value,
                        ExecutionOrder = oldModifier.ExecutionOrder
                    };
                    newEffect.AttributeModifiers.Add(newModifier);
                }

                // 删除旧实体（级联删除会自动处理关联的修改器）
                context.AttributeEffects.Remove(oldEffect);
                
                // 添加新实体
                context.AttributeEffects.Add(newEffect);
                
                await context.SaveChangesAsync();
            }
            else
            {
                // ID未变化，直接更新现有实体
                var effect = await context.AttributeEffects
                    .FirstOrDefaultAsync(e => e.Id == _originalId);

                if (effect == null)
                {
                    await _dialogService.ShowErrorAsync("保存失败", "找不到要更新的效果。");
                    return;
                }

                // 更新效果数据（不包括ID）
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
            }

            // 更新原始ID
            var originalIdField = typeof(GameplayEffectViewModel).GetField("_originalId", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            originalIdField?.SetValue(this, Id);

            HasUnsavedChanges = false;
            await _dialogService.ShowInfoAsync("保存成功", "效果已成功保存。");
            
            // 刷新父视图模型的数据
            await _parentViewModel.LoadDataAsync();
            
            // 重新加载属性修改器以反映ID变更
            await LoadAttributeModifiersAsync();
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
            // 创建属性选择对话框
            var excludedIds = new List<string>(); // 暂时不排除任何属性
            var selectAttributeVm = _viewModelFactory.CreateSelectAttributeViewModel(excludedIds);
            var selectedAttribute = await _dialogService.ShowSelectAttributeDialogAsync(selectAttributeVm);
            
            if (selectedAttribute == null)
                return;

            var newModifier = new AttributeModifier
            {
                EffectId = _originalId,
                AttributeType = selectedAttribute.Id,
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

    /// <summary>
    /// 上移修改器
    /// </summary>
    [RelayCommand]
    public async Task MoveModifierUpAsync(AttributeModifierViewModel? modifier)
    {
        if (modifier == null) return;

        try
        {
            var currentIndex = AttributeModifiers.IndexOf(modifier);
            if (currentIndex <= 0) return; // 已经在最上面

            // 交换执行顺序
            var previousModifier = AttributeModifiers[currentIndex - 1];
            var tempOrder = modifier.ExecutionOrder;
            modifier.ExecutionOrder = previousModifier.ExecutionOrder;
            previousModifier.ExecutionOrder = tempOrder;

            // 保存到数据库
            using var context = _dbContextFactory.CreateGameplayEffectDatabaseContext();
            
            var modifierToUpdate = await context.AttributeModifiers.FirstOrDefaultAsync(m => m.Id == modifier.Id);
            var previousToUpdate = await context.AttributeModifiers.FirstOrDefaultAsync(m => m.Id == previousModifier.Id);
            
            if (modifierToUpdate != null && previousToUpdate != null)
            {
                modifierToUpdate.ExecutionOrder = modifier.ExecutionOrder;
                previousToUpdate.ExecutionOrder = previousModifier.ExecutionOrder;
                await context.SaveChangesAsync();
            }

            // 重新加载列表以反映排序变化
            await LoadAttributeModifiersAsync();
        }
        catch (Exception ex)
        {
            await _dialogService.ShowErrorAsync("移动失败", ex.Message);
        }
    }

    /// <summary>
    /// 下移修改器
    /// </summary>
    [RelayCommand]
    public async Task MoveModifierDownAsync(AttributeModifierViewModel? modifier)
    {
        if (modifier == null) return;

        try
        {
            var currentIndex = AttributeModifiers.IndexOf(modifier);
            if (currentIndex >= AttributeModifiers.Count - 1) return; // 已经在最下面

            // 交换执行顺序
            var nextModifier = AttributeModifiers[currentIndex + 1];
            var tempOrder = modifier.ExecutionOrder;
            modifier.ExecutionOrder = nextModifier.ExecutionOrder;
            nextModifier.ExecutionOrder = tempOrder;

            // 保存到数据库
            using var context = _dbContextFactory.CreateGameplayEffectDatabaseContext();
            
            var modifierToUpdate = await context.AttributeModifiers.FirstOrDefaultAsync(m => m.Id == modifier.Id);
            var nextToUpdate = await context.AttributeModifiers.FirstOrDefaultAsync(m => m.Id == nextModifier.Id);
            
            if (modifierToUpdate != null && nextToUpdate != null)
            {
                modifierToUpdate.ExecutionOrder = modifier.ExecutionOrder;
                nextToUpdate.ExecutionOrder = nextModifier.ExecutionOrder;
                await context.SaveChangesAsync();
            }

            // 重新加载列表以反映排序变化
            await LoadAttributeModifiersAsync();
        }
        catch (Exception ex)
        {
            await _dialogService.ShowErrorAsync("移动失败", ex.Message);
        }
    }

    /// <summary>
    /// 添加测试修改器数据（用于UI测试）
    /// </summary>
    private async void AddTestModifiers()
    {
        try
        {
            System.Diagnostics.Debug.WriteLine($"[DEBUG] 开始添加测试修改器，当前集合数量: {AttributeModifiers.Count}");
            
            // 创建测试用的AttributeModifier对象
            var testModifier1 = new AttributeModifier
            {
                Id = 1,
                EffectId = _originalId,
                AttributeType = "Health",
                OperationType = OperationTypes.Add,
                Value = 100.0,
                ExecutionOrder = 0
            };

            var testModifier2 = new AttributeModifier
            {
                Id = 2,
                EffectId = _originalId,
                AttributeType = "Mana",
                OperationType = OperationTypes.Multiply,
                Value = 1.5,
                ExecutionOrder = 1
            };

            var testModifier3 = new AttributeModifier
            {
                Id = 3,
                EffectId = _originalId,
                AttributeType = "Damage",
                OperationType = OperationTypes.Percentage,
                Value = 25.0,
                ExecutionOrder = 2
            };

            System.Diagnostics.Debug.WriteLine($"[DEBUG] 创建了3个测试修改器，EffectId: {_originalId}");

            // 在UI线程中添加数据
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                // 创建对应的ViewModel并添加到集合中
                var viewModel1 = new AttributeModifierViewModel(testModifier1, this, _dialogService, _dbContextFactory);
                var viewModel2 = new AttributeModifierViewModel(testModifier2, this, _dialogService, _dbContextFactory);
                var viewModel3 = new AttributeModifierViewModel(testModifier3, this, _dialogService, _dbContextFactory);

                AttributeModifiers.Clear(); // 先清空
                 AttributeModifiers.Add(viewModel1);
                 AttributeModifiers.Add(viewModel2);
                 AttributeModifiers.Add(viewModel3);
                 
                 System.Diagnostics.Debug.WriteLine($"[DEBUG] 测试修改器添加完成，当前集合数量: {AttributeModifiers.Count}");
                 
                 // 强制触发UI更新
                 OnPropertyChanged(nameof(AttributeModifiers));
                 System.Diagnostics.Debug.WriteLine($"[DEBUG] 已触发PropertyChanged通知");
            });
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ERROR] 添加测试修改器失败: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"[ERROR] 堆栈跟踪: {ex.StackTrace}");
        }
    }
}