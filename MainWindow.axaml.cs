using Avalonia.Controls;
using Avalonia.Input;
using JoyConfig.ViewModels;

namespace JoyConfig;

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

        // 设置分割器拖拽事件
        SetupSplitterEvents();
    }

    private void SetupSplitterEvents()
    {
        // 这里可以添加分割器拖拽事件处理逻辑
        // 由于Avalonia的GridSplitter会自动处理拖拽，
        // 我们主要通过绑定和转换器来实现自动隐藏功能
    }
}
