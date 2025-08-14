using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.VisualTree;

namespace JoyConfig.Views;

public partial class SelectAttributeView : UserControl
{
    public SelectAttributeView()
    {
        InitializeComponent();
    }

    private void OkButton_OnClick(object? sender, RoutedEventArgs e)
    {
        var window = this.FindAncestorOfType<Window>();
        window?.Close(true);
    }

    private void CancelButton_OnClick(object? sender, RoutedEventArgs e)
    {
        var window = this.FindAncestorOfType<Window>();
        window?.Close(false);
    }
}
