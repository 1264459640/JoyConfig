using System;
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

namespace JoyConfig.ViewModels.GameplayEffectDatabase;

/// <summary>
/// 游戏效果数据库工作空间视图模型
/// </summary>
public partial class GameplayEffectDatabaseViewModel : EditorViewModelBase
{
    private readonly MainViewModel _mainViewModel;
    private readonly IDialogService _dialogService;
    private readonly IDbContextFactory _dbContextFactory;
    private readonly IViewModelFactory _viewModelFactory;
    private bool _isProgrammaticSelection;

    /// <summary>
    /// 主视图模型
    /// </summary>
    public MainViewModel MainViewModel => _mainViewModel;

    public GameplayEffectDatabaseViewModel(
        MainViewModel mainViewModel,
        IDialogService dialogService,
        IDbContextFactory dbContextFactory,
        IViewModelFactory viewModelFactory)
    {
        _mainViewModel = mainViewModel;
        _dialogService = dialogService;
        _dbContextFactory = dbContextFactory;
        _viewModelFactory = viewModelFactory;
        
        Title = "游戏效果数据库";
        
        _ = LoadDataAsync();
    }

    [ObservableProperty]
    private ObservableCollection<AttributeEffect> _attributeEffects = new();

    [ObservableProperty]
    private AttributeEffect? _selectedAttributeEffect;

    [ObservableProperty]
    private string? _errorMessage;

    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private string _selectedEffectTypeFilter = "All";

    [ObservableProperty]
    private string _selectedSourceTypeFilter = "All";

    [ObservableProperty]
    private bool _isLoading;

    /// <summary>
    /// 效果类型筛选选项
    /// </summary>
    public string[] EffectTypeFilterOptions { get; } = new[] { "All" }.Concat(EffectTypes.All).ToArray();

    /// <summary>
    /// 来源类型筛选选项
    /// </summary>
    public string[] SourceTypeFilterOptions { get; } = new[] { "All" }.Concat(SourceTypes.All).ToArray();

    /// <summary>
    /// 筛选后的效果列表
    /// </summary>
    public ObservableCollection<AttributeEffect> FilteredAttributeEffects { get; } = new();

    partial void OnSelectedAttributeEffectChanged(AttributeEffect? value)
    {
        if (value is not null && !_isProgrammaticSelection)
        {
            OpenEditorCommand.Execute(value);
        }
    }

    partial void OnSearchTextChanged(string value)
    {
        ApplyFilters();
    }

    partial void OnSelectedEffectTypeFilterChanged(string value)
    {
        ApplyFilters();
    }

    partial void OnSelectedSourceTypeFilterChanged(string value)
    {
        ApplyFilters();
    }

    /// <summary>
    /// 应用筛选条件
    /// </summary>
    private void ApplyFilters()
    {
        var filtered = AttributeEffects.AsEnumerable();

        // 搜索文本筛选
        if (!string.IsNullOrWhiteSpace(SearchText))
        {
            filtered = filtered.Where(e => 
                e.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                e.Id.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                (e.Description?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ?? false));
        }

        // 效果类型筛选
        if (SelectedEffectTypeFilter != "All")
        {
            filtered = filtered.Where(e => e.EffectType == SelectedEffectTypeFilter);
        }

        // 来源类型筛选
        if (SelectedSourceTypeFilter != "All")
        {
            filtered = filtered.Where(e => e.SourceType == SelectedSourceTypeFilter);
        }

        FilteredAttributeEffects.Clear();
        foreach (var effect in filtered.OrderBy(e => e.Name))
        {
            FilteredAttributeEffects.Add(effect);
        }
    }

    /// <summary>
    /// 打开编辑器命令
    /// </summary>
    [RelayCommand]
    public void OpenEditor(AttributeEffect? effect)
    {
        if (effect is null) return;

        _mainViewModel.CurrentEditor = _viewModelFactory.CreateGameplayEffectViewModel(effect, this);
    }

    /// <summary>
    /// 加载数据命令
    /// </summary>
    [RelayCommand]
    public async Task LoadDataAsync()
    {
        try
        {
            IsLoading = true;
            ErrorMessage = null;
            _mainViewModel.UpdateStatus("正在加载游戏效果数据...", true);

            using var context = _dbContextFactory.CreateGameplayEffectDatabaseContext();
            var effects = await context.AttributeEffects
                .Include(e => e.AttributeModifiers)
                .OrderBy(e => e.Name)
                .ToListAsync();

            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                AttributeEffects.Clear();
                foreach (var effect in effects)
                {
                    AttributeEffects.Add(effect);
                }
                ApplyFilters();
            });

            _mainViewModel.UpdateStatus($"已加载 {effects.Count} 个游戏效果");
        }
        catch (Exception ex)
        {
            ErrorMessage = $"加载数据失败: {ex.Message}";
            _mainViewModel.UpdateStatus("加载游戏效果数据失败");
            await _dialogService.ShowErrorAsync("加载失败", ex.Message);
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// 新建效果命令
    /// </summary>
    [RelayCommand]
    public async Task CreateNewEffectAsync()
    {
        try
        {
            var newId = await _dialogService.ShowInputAsync("新建效果", "请输入效果ID:", "");
            if (string.IsNullOrWhiteSpace(newId))
                return;

            // 检查ID是否已存在
            if (AttributeEffects.Any(e => e.Id == newId))
            {
                await _dialogService.ShowErrorAsync("创建失败", "该ID已存在，请使用其他ID。");
                return;
            }

            var newName = await _dialogService.ShowInputAsync("新建效果", "请输入效果名称:", "");
            if (string.IsNullOrWhiteSpace(newName))
                return;

            var newEffect = new AttributeEffect
            {
                Id = newId,
                Name = newName,
                EffectType = EffectTypes.Instant,
                StackingType = StackingTypes.NoStack,
                Priority = 0,
                IsPassive = false,
                IsInfinite = false,
                IsPeriodic = false,
                IntervalSeconds = 1.0
            };

            using var context = _dbContextFactory.CreateGameplayEffectDatabaseContext();
            context.AttributeEffects.Add(newEffect);
            await context.SaveChangesAsync();

            // 刷新数据
            await LoadDataAsync();

            // 选中新创建的效果
            _isProgrammaticSelection = true;
            SelectedAttributeEffect = AttributeEffects.FirstOrDefault(e => e.Id == newId);
            _isProgrammaticSelection = false;

            _mainViewModel.UpdateStatus($"已创建新效果: {newName}");
        }
        catch (Exception ex)
        {
            await _dialogService.ShowErrorAsync("创建失败", ex.Message);
        }
    }

    /// <summary>
    /// 删除效果命令
    /// </summary>
    [RelayCommand]
    public async Task DeleteEffectAsync(AttributeEffect? effect)
    {
        if (effect is null) return;

        try
        {
            var modifierCount = effect.AttributeModifiers?.Count ?? 0;
            var message = modifierCount > 0 
                ? $"确定要删除效果 '{effect.Name}' 吗？\n\n此操作将同时删除 {modifierCount} 个关联的属性修改器，且无法撤销。"
                : $"确定要删除效果 '{effect.Name}' 吗？\n\n此操作无法撤销。";

            var confirmed = await _dialogService.ShowConfirmationAsync("确认删除", message);
            if (!confirmed) return;

            using var context = _dbContextFactory.CreateGameplayEffectDatabaseContext();
            var effectToDelete = await context.AttributeEffects
                .Include(e => e.AttributeModifiers)
                .FirstOrDefaultAsync(e => e.Id == effect.Id);

            if (effectToDelete != null)
            {
                context.AttributeEffects.Remove(effectToDelete);
                await context.SaveChangesAsync();
            }

            // 刷新数据
            await LoadDataAsync();

            _mainViewModel.UpdateStatus($"已删除效果: {effect.Name}");
        }
        catch (Exception ex)
        {
            await _dialogService.ShowErrorAsync("删除失败", ex.Message);
        }
    }

    /// <summary>
    /// 刷新数据命令
    /// </summary>
    [RelayCommand]
    public async Task RefreshAsync()
    {
        await LoadDataAsync();
    }

    /// <summary>
    /// 清除筛选命令
    /// </summary>
    [RelayCommand]
    public void ClearFilters()
    {
        SearchText = string.Empty;
        SelectedEffectTypeFilter = "All";
        SelectedSourceTypeFilter = "All";
    }
}