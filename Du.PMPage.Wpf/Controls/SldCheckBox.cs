using System.Windows;
using SolidWorks.Interop.sldworks;

namespace Du.PMPage.Wpf.Controls;

/// <summary>
/// the wpf wrapper of <see cref="IPropertyManagerPageCheckbox"/>
/// </summary>
public class SldCheckBox : SldControl<IPropertyManagerPageCheckbox>
{
    static SldCheckBox()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(SldCheckBox),
            new FrameworkPropertyMetadata(typeof(SldCheckBox))
        );
    }

    /// <summary>
    /// Gets or sets the text displayed next to the check box.
    /// </summary>
    public string Caption
    {
        get { return (string)GetValue(CaptionProperty); }
        set { SetValue(CaptionProperty, value); }
    }

    /// <summary>
    /// Dependency property for <see cref="Caption"/>.
    /// </summary>
    public static readonly DependencyProperty CaptionProperty = DependencyProperty.Register(
        "Caption",
        typeof(string),
        typeof(SldCheckBox),
        new PropertyMetadata("CheckBox", OnCaptionPropertyCallback)
    );

    private static void OnCaptionPropertyCallback(
        DependencyObject d,
        DependencyPropertyChangedEventArgs e
    )
    {
        var sld = d as SldCheckBox;
        sld.OnCaptionChanged(e.OldValue as string, e.NewValue as string);
    }

    private void OnCaptionChanged(string oldValue, string newValue)
    {
        if (SControl != null && oldValue != newValue)
        {
            SControl.Caption = newValue;
        }
    }

    /// <summary>
    /// Gets or sets a value indicating whether the check box is checked.
    /// Binds two-way by default to reflect changes from the native SolidWorks control.
    /// </summary>
    public bool Checked
    {
        get { return (bool)GetValue(CheckedProperty); }
        set { SetValue(CheckedProperty, value); }
    }

    /// <summary>
    /// Dependency property for <see cref="Checked"/>.
    /// </summary>
    public static readonly DependencyProperty CheckedProperty = DependencyProperty.Register(
        "Checked",
        typeof(bool),
        typeof(SldCheckBox),
        new FrameworkPropertyMetadata(
            false,
            FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
            CheckedPropertyChanged
        )
    );

    private static void CheckedPropertyChanged(
        DependencyObject d,
        DependencyPropertyChangedEventArgs e
    )
    {
        var sld = d as SldCheckBox;
        sld.OnCheckedChanged((bool)e.OldValue, (bool)e.NewValue);
    }

    private void OnCheckedChanged(bool oldValue, bool newValue)
    {
        if (SControl != null && oldValue != newValue)
        {
            SControl.Checked = newValue;
        }
    }

    /// <summary>
    /// Synchronizes the managed properties to the native SolidWorks check box control
    /// after the native control has been created.
    /// </summary>
    protected override void SetSldControl()
    {
        SControl.Caption = Caption;
        SControl.Checked = Checked;
    }

    internal void OnUserCheckChanged(bool @checked)
    {
        Checked = @checked;
    }
}
