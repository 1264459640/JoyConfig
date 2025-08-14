using Avalonia.Controls;
using Avalonia.Input;

namespace JoyConfig.UI;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        var minimizeButton = this.FindControl<Button>("MinimizeButton");
        var maximizeButton = this.FindControl<Button>("MaximizeButton");
        var closeButton = this.FindControl<Button>("CloseButton");

        if (minimizeButton != null)
            minimizeButton.Click += (sender, e) => WindowState = WindowState.Minimized;

        if (maximizeButton != null)
            maximizeButton.Click += (sender, e) =>
                WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;

        if (closeButton != null)
            closeButton.Click += (sender, e) => Close();
        
        var titleBar = this.FindControl<Border>("TitleBar");
        if (titleBar != null)
        {
            titleBar.PointerPressed += (sender, e) =>
            {
                if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
                {
                    BeginMoveDrag(e);
                }
            };
        }
    }
}
