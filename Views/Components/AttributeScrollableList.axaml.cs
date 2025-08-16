using Avalonia;
using Avalonia.Controls;

namespace JoyConfig.Views.Components;

public partial class AttributeScrollableList : UserControl
{
    public static readonly StyledProperty<object> ListContentProperty =
        AvaloniaProperty.Register<AttributeScrollableList, object>(nameof(ListContent));

    public static readonly StyledProperty<bool> IsEmptyProperty =
        AvaloniaProperty.Register<AttributeScrollableList, bool>(nameof(IsEmpty), true);

    public static readonly StyledProperty<string> EmptyMessageProperty =
        AvaloniaProperty.Register<AttributeScrollableList, string>(nameof(EmptyMessage), "点击上方按钮开始添加");

    public static readonly StyledProperty<double> ScrollMaxHeightProperty =
        AvaloniaProperty.Register<AttributeScrollableList, double>(nameof(ScrollMaxHeight), 500.0);

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

    public double ScrollMaxHeight
    {
        get => GetValue(ScrollMaxHeightProperty);
        set => SetValue(ScrollMaxHeightProperty, value);
    }

    public AttributeScrollableList()
    {
        InitializeComponent();
    }
}