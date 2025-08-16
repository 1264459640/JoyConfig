using Avalonia;
using Avalonia.Controls;
using System.Windows.Input;

namespace JoyConfig.Views.Components;

public partial class EditorToolbar : UserControl
{
    public static readonly StyledProperty<string> SaveButtonTextProperty =
        AvaloniaProperty.Register<EditorToolbar, string>(nameof(SaveButtonText), "保存");

    public static readonly StyledProperty<ICommand> SaveCommandProperty =
        AvaloniaProperty.Register<EditorToolbar, ICommand>(nameof(SaveCommand));

    public static readonly StyledProperty<bool> IsSaveEnabledProperty =
        AvaloniaProperty.Register<EditorToolbar, bool>(nameof(IsSaveEnabled), true);

    public static readonly StyledProperty<bool> IsModifiedProperty =
        AvaloniaProperty.Register<EditorToolbar, bool>(nameof(IsModified), false);

    public static readonly StyledProperty<object> SecondaryActionsProperty =
        AvaloniaProperty.Register<EditorToolbar, object>(nameof(SecondaryActions));

    public string SaveButtonText
    {
        get => GetValue(SaveButtonTextProperty);
        set => SetValue(SaveButtonTextProperty, value);
    }

    public ICommand SaveCommand
    {
        get => GetValue(SaveCommandProperty);
        set => SetValue(SaveCommandProperty, value);
    }

    public bool IsSaveEnabled
    {
        get => GetValue(IsSaveEnabledProperty);
        set => SetValue(IsSaveEnabledProperty, value);
    }

    public bool IsModified
    {
        get => GetValue(IsModifiedProperty);
        set => SetValue(IsModifiedProperty, value);
    }

    public object SecondaryActions
    {
        get => GetValue(SecondaryActionsProperty);
        set => SetValue(SecondaryActionsProperty, value);
    }

    public EditorToolbar()
    {
        InitializeComponent();
    }
}