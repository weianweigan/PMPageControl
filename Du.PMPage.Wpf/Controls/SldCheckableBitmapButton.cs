using SolidWorks.Interop.sldworks;
using System.Windows;

namespace Du.PMPage.Wpf.Controls;

/// <summary>
/// WPF wrapper for a SolidWorks Property Manager Page checkable bitmap button control.
/// Extends <see cref="SldBitmapButton"/> with toggle/check behavior via
/// <see cref="IsCheckable"/> and <see cref="Checked"/> properties.
/// </summary>
public class SldCheckableBitmapButton : SldBitmapButton
{
    /// <summary>
    /// True if bitmap button is clickable, false if not
    /// </summary>
    public bool IsCheckable
    {
        get { return (bool)GetValue(IsCheckableProperty); }
        set { SetValue(IsCheckableProperty, value); }
    }

    /// <summary>
    /// Dependency property for <see cref="IsCheckable"/>.
    /// </summary>
    public static readonly DependencyProperty IsCheckableProperty = DependencyProperty.Register(
        "IsCheckable",
        typeof(bool),
        typeof(SldCheckableBitmapButton),
        new PropertyMetadata(true, OnIsCheckableCallback)
    );

    /// <summary>
    /// Gets or sets a value indicating whether the button is in the checked (pressed) state.
    /// Only meaningful when <see cref="IsCheckable"/> is <c>true</c>.
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
        typeof(SldCheckableBitmapButton),
        new PropertyMetadata(false, OnCheckedCallback)
    );

    private static void OnIsCheckableCallback(
        DependencyObject d,
        DependencyPropertyChangedEventArgs e
    )
    {
        var sld = d as SldCheckableBitmapButton;
        sld.OnIsCheckablePropertyCallback((bool)e.OldValue, (bool)e.NewValue);
    }

    private void OnIsCheckablePropertyCallback(bool oldValue, bool newValue)
    {
        if (SControl != null && oldValue != newValue)
        {
            SControl.IsCheckable = newValue;
        }
    }

    private static void OnCheckedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var sld = d as SldCheckableBitmapButton;
        sld.OnCheckedPropertyChanged((bool)e.OldValue, (bool)e.NewValue);
    }

    private void OnCheckedPropertyChanged(bool oldValue, bool newValue)
    {
        if (SControl != null && oldValue != newValue)
        {
            if (IsCheckable)
            {
                SControl.Checked = newValue;
            }
        }
    }

    /// <summary>
    /// Synchronizes the managed properties to the native SolidWorks checkable bitmap button
    /// control after the native control has been created.
    /// </summary>
    protected override void SetSldControl()
    {
        base.SetSldControl();
        SControl.IsCheckable = IsCheckable;
        SControl.Checked = Checked;
    }
}
