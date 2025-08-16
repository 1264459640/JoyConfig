using Avalonia;
using Avalonia.Controls;

namespace JoyConfig.Views.Components;

public partial class BasicInfoCard : UserControl
{
    public static readonly StyledProperty<string> TitleProperty =
        AvaloniaProperty.Register<BasicInfoCard, string>(nameof(Title), "基本信息");

    public static readonly StyledProperty<object> FormContentProperty =
        AvaloniaProperty.Register<BasicInfoCard, object>(nameof(FormContent));

    public string Title
    {
        get => GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public object FormContent
    {
        get => GetValue(FormContentProperty);
        set => SetValue(FormContentProperty, value);
    }

    public BasicInfoCard()
    {
        InitializeComponent();
    }
}