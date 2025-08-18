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
    private ObservableCollection<AttributeEffect> _selectedAttributeEffects = new();

    [ObservableProperty]
    private bool _isMultiSelectMode;

    [ObservableProperty]
    private string? _errorMessage;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private string _selectedEffectTypeFilter = "All";

    [ObservableProperty]
    private string _selectedSourceTypeFilter = "All";

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
    /// 切换多选模式
    /// </summary>
    [RelayCommand]
    public void ToggleMultiSelectMode()
    {
        IsMultiSelectMode = !IsMultiSelectMode;
        if (!IsMultiSelectMode)
        {
            SelectedAttributeEffects.Clear();
        }
    }

    /// <summary>
    /// 选择/取消选择效果
    /// </summary>
    [RelayCommand]
    public void ToggleEffectSelection(AttributeEffect effect)
    {
        if (!IsMultiSelectMode) return;

        if (SelectedAttributeEffects.Contains(effect))
        {
            SelectedAttributeEffects.Remove(effect);
        }
        else
        {
            SelectedAttributeEffects.Add(effect);
        }
    }

    /// <summary>
    /// 全选/取消全选
    /// </summary>
    [RelayCommand]
    public void ToggleSelectAll()
    {
        if (!IsMultiSelectMode) return;

        if (SelectedAttributeEffects.Count == FilteredAttributeEffects.Count)
        {
            // 全部已选中，取消全选
            SelectedAttributeEffects.Clear();
        }
        else
        {
            // 全选
            SelectedAttributeEffects.Clear();
            foreach (var effect in FilteredAttributeEffects)
            {
                SelectedAttributeEffects.Add(effect);
            }
        }
    }

    /// <summary>
    /// 批量删除选中的效果
    /// </summary>
    [RelayCommand]
    public async Task BatchDeleteEffectsAsync()
    {
        if (!IsMultiSelectMode || SelectedAttributeEffects.Count == 0)
        {
            await _dialogService.ShowWarningAsync("提示", "请先选择要删除的效果。");
            return;
        }

        var effectCount = SelectedAttributeEffects.Count;
        var effectNames = string.Join(", ", SelectedAttributeEffects.Take(3).Select(e => e.Name));
        if (effectCount > 3)
        {
            effectNames += $" 等 {effectCount} 个效果";
        }

        var confirmed = await _dialogService.ShowConfirmationAsync(
            "确认批量删除",
            $"确定要删除以下效果吗？\n\n{effectNames}\n\n此操作将同时删除这些效果的所有关联修改器，且不可撤销。");

        if (!confirmed) return;

        try
        {
            IsLoading = true;
            _mainViewModel.UpdateStatus($"正在删除 {effectCount} 个游戏效果...", true);

            using var context = _dbContextFactory.CreateGameplayEffectDatabaseContext();

            var effectIds = SelectedAttributeEffects.Select(e => e.Id).ToList();
            var effectsToDelete = await context.AttributeEffects
                .Where(e => effectIds.Contains(e.Id))
                .ToListAsync();

            context.AttributeEffects.RemoveRange(effectsToDelete);
            await context.SaveChangesAsync();

            // 更新UI
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                foreach (var effect in SelectedAttributeEffects.ToList())
                {
                    AttributeEffects.Remove(effect);
                }
                SelectedAttributeEffects.Clear();
                ApplyFilters();
            });

            _mainViewModel.UpdateStatus($"已成功删除 {effectCount} 个游戏效果");
            await _dialogService.ShowInfoAsync("删除成功", $"已成功删除 {effectCount} 个游戏效果及其关联的修改器。");
        }
        catch (Exception ex)
        {
            ErrorMessage = $"批量删除失败: {ex.Message}";
            _mainViewModel.UpdateStatus("批量删除游戏效果失败");
            await _dialogService.ShowErrorAsync("删除失败", ex.Message);
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// 批量修改效果的公共字段
    /// </summary>
    [RelayCommand]
    public async Task BatchEditEffectsAsync()
    {
        if (!IsMultiSelectMode || SelectedAttributeEffects.Count == 0)
        {
            await _dialogService.ShowWarningAsync("提示", "请先选择要编辑的效果。");
            return;
        }

        // 显示批量编辑对话框
        var sourceType = await _dialogService.ShowInputAsync(
            "批量编辑",
            "请输入新的来源类型（留空表示不修改）:",
            "");

        var tags = await _dialogService.ShowInputAsync(
            "批量编辑",
            "请输入新的标签（留空表示不修改）:",
            "");

        if (string.IsNullOrWhiteSpace(sourceType) && string.IsNullOrWhiteSpace(tags))
        {
            await _dialogService.ShowInfoAsync("提示", "没有要修改的字段。");
            return;
        }

        try
        {
            IsLoading = true;
            var effectCount = SelectedAttributeEffects.Count;
            _mainViewModel.UpdateStatus($"正在批量编辑 {effectCount} 个游戏效果...", true);

            using var context = _dbContextFactory.CreateGameplayEffectDatabaseContext();

            var effectIds = SelectedAttributeEffects.Select(e => e.Id).ToList();
            var effectsToUpdate = await context.AttributeEffects
                .Where(e => effectIds.Contains(e.Id))
                .ToListAsync();

            foreach (var effect in effectsToUpdate)
            {
                if (!string.IsNullOrWhiteSpace(sourceType))
                {
                    effect.SourceType = sourceType;
                }

                if (!string.IsNullOrWhiteSpace(tags))
                {
                    effect.Tags = tags;
                }
            }

            await context.SaveChangesAsync();

            // 重新加载数据
            await LoadDataAsync();

            _mainViewModel.UpdateStatus($"已成功批量编辑 {effectCount} 个游戏效果");
            await _dialogService.ShowInfoAsync("编辑成功", $"已成功批量编辑 {effectCount} 个游戏效果。");
        }
        catch (Exception ex)
        {
            ErrorMessage = $"批量编辑失败: {ex.Message}";
            _mainViewModel.UpdateStatus("批量编辑游戏效果失败");
            await _dialogService.ShowErrorAsync("编辑失败", ex.Message);
        }
        finally
        {
            IsLoading = false;
        }
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