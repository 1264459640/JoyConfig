using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;

namespace JoyConfig.Application.ViewModels;

public partial class SelectTemplateDialogViewModel : ObservableObject
{
    [ObservableProperty]
    private ObservableCollection<string> _templateNames = new();

    [ObservableProperty]
    private string? _selectedTemplate;
    
    public ObservableCollection<string> Templates => TemplateNames;

    public SelectTemplateDialogViewModel()
    {
        LoadTemplateNames();
    }

    private void LoadTemplateNames()
    {
        var templatesPath = Path.Combine(System.AppContext.BaseDirectory, "Templates");
        if (Directory.Exists(templatesPath))
        {
            var files = Directory.GetFiles(templatesPath, "*.json");
            TemplateNames = new ObservableCollection<string>(files.Select(Path.GetFileNameWithoutExtension));
        }
    }
}
