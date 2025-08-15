using System;
using JoyConfig.Models.AttributeDatabase;
using CommunityToolkit.Mvvm.ComponentModel;
using Attribute = JoyConfig.Models.AttributeDatabase.Attribute;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Linq;
using JoyConfig.Models.DTOs;
using JoyConfig.Services;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Avalonia.Threading;

namespace JoyConfig.ViewModels;

public partial class AttributeViewModel : EditorViewModelBase
{
    private readonly AttributeDatabaseViewModel _parentViewModel;
    private readonly IDialogService _dialogService;
    private readonly IAttributeRepository _attributeRepository;

    [ObservableProperty]
    private Attribute _attribute;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(DeleteCommand))]
    private bool _isNew;

    [ObservableProperty]
    private string? _errorMessage;
    
    [ObservableProperty]
    private string _idSuffix;

    private string _originalId;

    public string CategoryPrefix => $"{Attribute.Category}.";

    public ObservableCollection<AttributeSet> ReferencingAttributeSets { get; } = new();

    public AttributeViewModel(
        Attribute attribute, 
        AttributeDatabaseViewModel parentViewModel, 
        IDialogService dialogService,
        IAttributeRepository attributeRepository)
    {
        _attribute = attribute;
        _parentViewModel = parentViewModel;
        _dialogService = dialogService;
        _attributeRepository = attributeRepository;
        Title = $"Attribute: {attribute.Id}";

        // Initialize Suffix
        _idSuffix = "";
        if (!string.IsNullOrEmpty(attribute.Id) && attribute.Id.StartsWith(CategoryPrefix))
        {
            _idSuffix = attribute.Id.Substring(CategoryPrefix.Length);
        }
        else if (attribute.Id == "New.Attribute.Id")
        {
            _idSuffix = "NewId";
        }

        // Fire and forget
        _ = InitializeAsync();

        _originalId = attribute.Id;
    }

    private async Task InitializeAsync()
    {
        var existing = await _attributeRepository.GetAttributeByIdAsync(Attribute.Id);
        IsNew = existing == null;
        
        if (!IsNew)
        {
            await LoadReferencingAttributeSetsAsync();
        }
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        ErrorMessage = null;
        var newFullId = CategoryPrefix + IdSuffix;

        if (IsNew)
        {
            // Validation for new attributes
            if (string.IsNullOrWhiteSpace(IdSuffix))
            {
                ErrorMessage = "ID suffix cannot be empty.";
                return;
            }
            
            var existing = await _attributeRepository.GetAttributeByIdAsync(newFullId);
            if (existing != null)
            {
                ErrorMessage = $"ID '{newFullId}' already exists.";
                return;
            }
            
            Attribute.Id = newFullId;
            await _attributeRepository.CreateAttributeAsync(Attribute);
            
            Title = $"Attribute: {Attribute.Id}";
            IsNew = false;
            _originalId = newFullId; // Set original ID to the new ID after creation
            await _parentViewModel.RefreshAttributeListAsync(Attribute);
            ErrorMessage = "Attribute created successfully.";
        }
        else
        {
            // For existing attributes, check if ID has changed.
            if (_originalId != newFullId)
            {
                // --- ID has changed, perform guided update ---
                var preview = await _attributeRepository.PreviewAttributeChangeAsync(_originalId, newFullId, Attribute.Category, Attribute.Category);

                if (!preview.IsValid)
                {
                    ErrorMessage = preview.ErrorMessage;
                    return;
                }

                if (!preview.AffectedAttributes.Any())
                {
                    ErrorMessage = "No changes needed or attribute not found.";
                    return;
                }

                var dialogViewModel = new ConfirmationDialogViewModel
                {
                    Title = "Confirm High-Risk Change",
                    Message = $"You are about to change a critical identifier. This will update {preview.AffectedAttributes.Count} attribute(s) and {preview.AffectedValueCount} value(s) across {preview.AffectedAttributeSets.Count} attribute set(s). This action cannot be undone.",
                    Details = preview.AffectedAttributeSets.Select(s => $"Set: {s.Id} ({s.Name})").ToList()
                };

                var confirmed = await _dialogService.ShowConfirmationDialogAsync(dialogViewModel);

                if (confirmed)
                {
                    try
                    {
                        await _attributeRepository.ExecuteAttributeChangeAsync(preview);

                        ErrorMessage = "Update successful. Reloading editor...";
                        await _parentViewModel.ReloadAndSelectAttributeAsync(newFullId);
                    }
                    catch (Exception ex)
                    {
                        ErrorMessage = $"An error occurred during update: {ex.Message}";
                        // Optionally, revert the ID in the UI
                        IdSuffix = _originalId.Substring(CategoryPrefix.Length);
                    }
                }
                else
                {
                    // Revert the change in the textbox
                    IdSuffix = _originalId.Substring(CategoryPrefix.Length);
                    ErrorMessage = "Update cancelled.";
                }
            }
            else
            {
                // --- ID has not changed, just save other properties ---
                await _attributeRepository.UpdateAttributeAsync(Attribute);
                ErrorMessage = "Attribute saved successfully.";
            }
        }
    }

    private async Task LoadReferencingAttributeSetsAsync()
    {
        var sets = await _attributeRepository.GetReferencingAttributeSetsAsync(Attribute.Id);
        
        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            ReferencingAttributeSets.Clear();
            foreach (var set in sets)
            {
                ReferencingAttributeSets.Add(set);
            }
        });
    }

    public bool CanDelete => !IsNew;

    [RelayCommand(CanExecute = nameof(CanDelete))]
    private async Task DeleteAsync()
    {
        ErrorMessage = null;
        
        var dialogViewModel = new ConfirmationDialogViewModel
        {
            Title = "Confirm Deletion"
        };

        if (ReferencingAttributeSets.Any())
        {
            dialogViewModel.Message = $"This attribute is referenced by {ReferencingAttributeSets.Count} attribute set(s). Deleting it will remove all associated values. This action cannot be undone.";
            dialogViewModel.Details = ReferencingAttributeSets.Select(s => $"Set: {s.Id} ({s.Name})").ToList();
        }
        else
        {
            dialogViewModel.Message = "Are you sure you want to permanently delete this attribute? This action cannot be undone.";
        }

        var confirmed = await _dialogService.ShowConfirmationDialogAsync(dialogViewModel);

        if (confirmed)
        {
            try
            {
                await _attributeRepository.DeleteAttributeAsync(Attribute.Id);
                await _parentViewModel.ReloadAndCloseEditorAsync();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"An error occurred during deletion: {ex.Message}";
            }
        }
        else
        {
            ErrorMessage = "Deletion cancelled.";
        }
    }
}
