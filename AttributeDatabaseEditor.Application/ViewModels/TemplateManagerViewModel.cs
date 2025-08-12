using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using JoyConfig.Core.Models.DTOs;
using JoyConfig.Core.Services;
using JoyConfig.Infrastructure.Services;

namespace JoyConfig.Application.ViewModels;

public partial class TemplateManagerViewModel : EditorViewModelBase
{
    private readonly IDialogService _dialogService;

    [ObservableProperty]
    private ObservableCollection<string> _templateFiles = new();

    [ObservableProperty]
    private string? _selectedTemplateFile;

    [ObservableProperty]
    private AttributeSetTemplate? _currentTemplate;

    [ObservableProperty]
    private bool _isDialogMode;

    public LocalizationManager LocalizationManager { get; }

    public TemplateManagerViewModel(IDialogService dialogService)
    {
        _dialogService = dialogService;
        LocalizationManager = LocalizationManager.Instance;
        Title = "Template Manager";
        LoadTemplateFiles();
    }

    private void LoadTemplateFiles()
    {
        var templatesPath = Path.Combine(AppContext.BaseDirectory, "Templates");
        if (!Directory.Exists(templatesPath))
        {
            Directory.CreateDirectory(templatesPath);
        }

        var files = Directory.GetFiles(templatesPath, "*.json")
            .Select(Path.GetFileNameWithoutExtension)
            .Where(f => f != null)
            .Select(f => f!);
        TemplateFiles = new ObservableCollection<string>(files);
    }

    partial void OnSelectedTemplateFileChanged(string? value)
    {
        if (value != null)
        {
            LoadTemplate(value);
        }
        else
        {
            CurrentTemplate = null;
        }
    }

    private void LoadTemplate(string templateName)
    {
        try
        {
            var templatesPath = Path.Combine(AppContext.BaseDirectory, "Templates");
            var filePath = Path.Combine(templatesPath, $"{templateName}.json");
            var json = File.ReadAllText(filePath);
            CurrentTemplate = JsonSerializer.Deserialize<AttributeSetTemplate>(json);
        }
        catch (Exception)
        {
            // Handle error
            CurrentTemplate = null;
        }
    }

    [RelayCommand]
    private void CreateNewTemplate()
    {
        CurrentTemplate = new AttributeSetTemplate
        {
            Name = "New Template",
            Description = "A new template."
        };
        SelectedTemplateFile = null;
    }

    [RelayCommand]
    private async Task SaveTemplateAsync()
    {
        if (CurrentTemplate == null || string.IsNullOrWhiteSpace(CurrentTemplate.Name))
        {
            return;
        }

        try
        {
            var templatesPath = Path.Combine(AppContext.BaseDirectory, "Templates");
            var fileName = SelectedTemplateFile ?? CurrentTemplate.Name;
            var filePath = Path.Combine(templatesPath, $"{fileName}.json");
            var json = JsonSerializer.Serialize(CurrentTemplate, new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(filePath, json);
            LoadTemplateFiles();
            SelectedTemplateFile = fileName;
        }
        catch (Exception)
        {
            // Handle error
        }
    }

    [RelayCommand]
    private async Task DeleteTemplateAsync()
    {
        if (SelectedTemplateFile == null) return;

        // Confirmation dialog would be good here
        
        try
        {
            var templatesPath = Path.Combine(AppContext.BaseDirectory, "Templates");
            var filePath = Path.Combine(templatesPath, $"{SelectedTemplateFile}.json");
            File.Delete(filePath);
            LoadTemplateFiles();
            SelectedTemplateFile = null;
        }
        catch (Exception)
        {
            // Handle error
        }
    }
}
