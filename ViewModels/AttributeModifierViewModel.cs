using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using JoyConfig.Models.GameplayEffectDatabase;
using JoyConfig.Services;
using Microsoft.EntityFrameworkCore;

namespace JoyConfig.ViewModels;

/// <summary>
/// 属性修改器视图模型
/// </summary>
public partial class AttributeModifierViewModel : ObservableObject
{
    private readonly GameplayEffectViewModel _parentViewModel;
    private readonly IDialogService _dialogService;
    private readonly IDbContextFactory _dbContextFactory;
    private bool _hasUnsavedChanges;

    public AttributeModifierViewModel(
        AttributeModifier modifier,
        GameplayEffectViewModel parentViewModel,
        IDialogService dialogService,
        IDbContextFactory dbContextFactory)
    {
        _parentViewModel = parentViewModel;
        _dialogService = dialogService;
        _dbContextFactory = dbContextFactory;
        
        // 复制修改器数据
        Id = modifier.Id;
        EffectId = modifier.EffectId;
        AttributeType = modifier.AttributeType;
        OperationType = modifier.OperationType;
        Value = modifier.Value;
        ExecutionOrder = modifier.ExecutionOrder ?? 0;
    }

    [ObservableProperty]
    private int _id;

    [ObservableProperty]
    private string _effectId = string.Empty;

    [ObservableProperty]
    private string _attributeType = string.Empty;

    [ObservableProperty]
    private string _operationType = OperationTypes.Add;

    [ObservableProperty]
    private double _value;

    [ObservableProperty]
    private int _executionOrder;

    [ObservableProperty]
    private string? _errorMessage;

    [ObservableProperty]
    private bool _isLoading;

    /// <summary>
    /// 操作类型选项
    /// </summary>
    public string[] OperationTypeOptions => OperationTypes.All;

    /// <summary>
    /// 是否有未保存的更改
    /// </summary>
    public bool HasUnsavedChanges
    {
        get => _hasUnsavedChanges;
        set => SetProperty(ref _hasUnsavedChanges, value);
    }

    partial void OnAttributeTypeChanged(string value) => HasUnsavedChanges = true;
    partial void OnOperationTypeChanged(string value) => HasUnsavedChanges = true;
    partial void OnValueChanged(double value) => HasUnsavedChanges = true;
    partial void OnExecutionOrderChanged(int value) => HasUnsavedChanges = true;

    /// <summary>
    /// 保存修改器
    /// </summary>
    [RelayCommand]
    public async Task SaveAsync()
    {
        try
        {
            // 验证数据
            if (string.IsNullOrWhiteSpace(AttributeType))
            {
                await _dialogService.ShowErrorAsync("验证失败", "属性类型不能为空。");
                return;
            }

            if (string.IsNullOrWhiteSpace(OperationType))
            {
                await _dialogService.ShowErrorAsync("验证失败", "操作类型不能为空。");
                return;
            }

            IsLoading = true;
            ErrorMessage = null;

            using var context = _dbContextFactory.CreateGameplayEffectDatabaseContext();
            var modifier = await context.AttributeModifiers
                .FirstOrDefaultAsync(m => m.Id == Id);

            if (modifier == null)
            {
                await _dialogService.ShowErrorAsync("保存失败", "找不到要更新的修改器。");
                return;
            }

            // 更新修改器数据
            modifier.AttributeType = AttributeType;
            modifier.OperationType = OperationType;
            modifier.Value = Value;
            modifier.ExecutionOrder = ExecutionOrder;

            await context.SaveChangesAsync();

            HasUnsavedChanges = false;
            await _dialogService.ShowInfoAsync("保存成功", "修改器已成功保存。");
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
    /// 删除修改器
    /// </summary>
    [RelayCommand]
    public async Task DeleteAsync()
    {
        try
        {
            var confirmed = await _dialogService.ShowConfirmationAsync(
                "确认删除", 
                $"确定要删除属性修改器 '{AttributeType}' 吗？\n\n此操作无法撤销。");
            if (!confirmed) return;

            IsLoading = true;
            ErrorMessage = null;

            using var context = _dbContextFactory.CreateGameplayEffectDatabaseContext();
            var modifier = await context.AttributeModifiers
                .FirstOrDefaultAsync(m => m.Id == Id);

            if (modifier != null)
            {
                context.AttributeModifiers.Remove(modifier);
                await context.SaveChangesAsync();
            }

            await _dialogService.ShowInfoAsync("删除成功", "修改器已成功删除。");
            
            // 刷新父视图模型的修改器列表
            await _parentViewModel.LoadAttributeModifiersAsync();
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
    /// 获取操作类型的显示名称
    /// </summary>
    public string GetOperationTypeDisplayName(string operationType)
    {
        return operationType switch
        {
            OperationTypes.Add => "加法 (+)",
            OperationTypes.Subtract => "减法 (-)",
            OperationTypes.Multiply => "乘法 (×)",
            OperationTypes.Override => "覆盖 (=)",
            OperationTypes.Percentage => "百分比 (%)",
            _ => operationType
        };
    }

    /// <summary>
    /// 获取操作类型的颜色
    /// </summary>
    public string GetOperationTypeColor(string operationType)
    {
        return operationType switch
        {
            OperationTypes.Add => "#4CAF50",      // 绿色
            OperationTypes.Subtract => "#F44336", // 红色
            OperationTypes.Multiply => "#FF9800", // 橙色
            OperationTypes.Override => "#9C27B0", // 紫色
            OperationTypes.Percentage => "#2196F3", // 蓝色
            _ => "#757575" // 灰色
        };
    }
}