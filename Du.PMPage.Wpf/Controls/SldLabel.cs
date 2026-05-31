using System.Windows;
using SolidWorks.Interop.sldworks;

namespace Du.PMPage.Wpf.Controls;

/// <summary>
/// the wpf wrapper of <see cref="IPropertyManagerPageLabel"/>
/// </summary>
public class SldLabel : SldControl<IPropertyManagerPageLabel>
{
    static SldLabel()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(SldLabel),
            new FrameworkPropertyMetadata(typeof(SldLabel))
        );
    }

    /// <summary>
    /// Gets or sets the caption for this label.
    /// </summary>
    public string SldText
    {
        get { return (string)GetValue(SldTextProperty); }
        set { SetValue(SldTextProperty, value); }
    }

    // Using a DependencyProperty as the backing store for SldText.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty SldTextProperty = DependencyProperty.Register(
        "SldText",
        typeof(string),
        typeof(SldLabel),
        new PropertyMetadata("", OnSldTextPropertyCallback)
    );

    private static void OnSldTextPropertyCallback(
        DependencyObject d,
        DependencyPropertyChangedEventArgs e
    )
    {
        var sld = (SldLabel)d;
        sld.OnSldTextPropertyChanged((string)e.OldValue, (string)e.NewValue);
    }

    private void OnSldTextPropertyChanged(string oldValue, string newValue)
    {
        if (SControl != null && oldValue != newValue)
        {
            SControl.Caption = newValue;
        }
    }

    protected override void SetSldControl()
    {
        SControl.Caption = SldText;
    }
}
