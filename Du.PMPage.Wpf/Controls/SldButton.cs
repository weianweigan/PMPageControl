using System.Windows;
using System.Windows.Input;
using SolidWorks.Interop.sldworks;

namespace Du.PMPage.Wpf.Controls;

/// <summary>
/// WPF wrapper for a SolidWorks Property Manager Page button control
/// (<see cref="IPropertyManagerPageButton"/>).
/// </summary>
public class SldButton : SldControl<IPropertyManagerPageButton>, ISldBtnCommand
{
    static SldButton()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(SldButton),
            new FrameworkPropertyMetadata(typeof(SldButton))
        );
    }

    /// <summary>
    /// Gets or sets the text displayed on the button.
    /// </summary>
    public string Caption
    {
        get { return (string)GetValue(CaptionProperty); }
        set { SetValue(CaptionProperty, value); }
    }

    /// <summary>
    /// Gets or sets the command to invoke when the button is clicked.
    /// </summary>
    public ICommand Command
    {
        get { return (ICommand)GetValue(CommandProperty); }
        set { SetValue(CommandProperty, value); }
    }

    /// <summary>
    /// Dependency property for <see cref="Command"/>.
    /// </summary>
    public static readonly DependencyProperty CommandProperty = DependencyProperty.Register(
        "Command",
        typeof(ICommand),
        typeof(SldButton),
        new PropertyMetadata(null)
    );

    /// <summary>
    /// Dependency property for <see cref="Caption"/>.
    /// </summary>
    public static readonly DependencyProperty CaptionProperty = DependencyProperty.Register(
        "Caption",
        typeof(string),
        typeof(SldButton),
        new PropertyMetadata(null, OnCaptionCallback)
    );

    private static void OnCaptionCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var sld = d as SldButton;
        sld.OnCaptionPropertyChanged((string)e.OldValue, (string)e.NewValue);
    }

    private void OnCaptionPropertyChanged(string oldValue, string newValue)
    {
        if (SControl != null && oldValue != newValue)
        {
            SControl.Caption = newValue;
        }
    }

    /// <summary>
    /// Synchronizes the managed properties to the native SolidWorks button control
    /// after the native control has been created.
    /// </summary>
    protected override void SetSldControl()
    {
        if (SControl != null && !string.IsNullOrWhiteSpace(Caption))
        {
            SControl.Caption = Caption;
        }
    }
}
