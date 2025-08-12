using Avalonia.Controls;
using Avalonia.Interactivity;
using JoyConfig.Application.ViewModels;

namespace JoyConfig.Views
{
    public partial class SelectTemplateDialog : Window
    {
        public SelectTemplateDialog()
        {
            InitializeComponent();
        }

        private void OkButton_OnClick(object? sender, RoutedEventArgs e)
        {
            var vm = DataContext as SelectTemplateDialogViewModel;
            Close(vm?.SelectedTemplate);
        }

        private void CancelButton_OnClick(object? sender, RoutedEventArgs e)
        {
            Close(null);
        }
    }
}
