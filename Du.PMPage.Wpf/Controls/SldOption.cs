using System;
using System.Windows;
using SolidWorks.Interop.sldworks;

namespace Du.PMPage.Wpf.Controls;

/// <summary>
/// A WPF control that wraps a native SolidWorks <see cref="IPropertyManagerPageOption"/> (radio button).
/// Supports two-way binding for the <see cref="Checked"/> state.
/// </summary>
public class SldOption : SldControl<IPropertyManagerPageOption>
{
    static SldOption()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(SldOption),
            new FrameworkPropertyMetadata(typeof(SldOption))
        );
    }

    /// <summary>
    /// Gets or sets whether this option is checked (selected).
    /// Binds two-way by default to the native <see cref="IPropertyManagerPageOption.Checked"/> property.
    /// </summary>
    public bool Checked
    {
        get { return (bool)GetValue(CheckedProperty); }
        set { SetValue(CheckedProperty, value); }
    }

    /// <summary>
    /// Identifies the <see cref="Checked"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty CheckedProperty = DependencyProperty.Register(
        "Checked",
        typeof(bool),
        typeof(SldOption),
        new FrameworkPropertyMetadata(
            false,
            FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
            OnCheckedPropertyCallback
        )
    );

    private static void OnCheckedPropertyCallback(
        DependencyObject d,
        DependencyPropertyChangedEventArgs e
    )
    {
        var sld = d as SldOption;
        sld.OnCheckedChanged((bool)e.OldValue, (bool)e.NewValue);
    }

    private void OnCheckedChanged(bool oldValue, bool newValue)
    {
        if (oldValue != newValue && SControl != null)
        {
            SControl.Checked = newValue;
        }
    }

    /// <inheritdoc/>
    protected override void SetSldControl()
    {
        SControl.Caption = SldCaption;
        SControl.Checked = Checked;
    }

    internal void OnCheckedUpdate()
    {
        Checked = SControl.Checked;
    }
}
