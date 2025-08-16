using Avalonia;
using Avalonia.Controls;
using System.Collections;
using System.Windows.Input;

namespace JoyConfig.Views.Components;

public partial class AttributeListCard : UserControl
{
    public static readonly StyledProperty<string> TitleProperty =
        AvaloniaProperty.Register<AttributeListCard, string>(nameof(Title), "属性列表");

    public static readonly StyledProperty<string> CountTextProperty =
        AvaloniaProperty.Register<AttributeListCard, string>(nameof(CountText), "共 0 个属性");

    public static readonly StyledProperty<object> ListContentProperty =
        AvaloniaProperty.Register<AttributeListCard, object>(nameof(ListContent));

    public static readonly StyledProperty<bool> IsEmptyProperty =
        AvaloniaProperty.Register<AttributeListCard, bool>(nameof(IsEmpty), true);

    public static readonly StyledProperty<string> EmptyMessageProperty =
        AvaloniaProperty.Register<AttributeListCard, string>(nameof(EmptyMessage), "点击上方按钮开始添加");

    public string Title
    {
        get => GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public string CountText
    {
        get => GetValue(CountTextProperty);
        set => SetValue(CountTextProperty, value);
    }

    public object ListContent
    {
        get => GetValue(ListContentProperty);
        set => SetValue(ListContentProperty, value);
    }

    public bool IsEmpty
    {
        get => GetValue(IsEmptyProperty);
        set => SetValue(IsEmptyProperty, value);
    }

    public string EmptyMessage
    {
        get => GetValue(EmptyMessageProperty);
        set => SetValue(EmptyMessageProperty, value);
    }

    public AttributeListCard()
    {
        InitializeComponent();
    }
}