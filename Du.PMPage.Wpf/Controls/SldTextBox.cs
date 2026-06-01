using System;
using System.Windows;
using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;

namespace Du.PMPage.Wpf.Controls;

/// <summary>
/// A WPF control that wraps a native SolidWorks <see cref="IPropertyManagerPageTextbox"/>.
/// Supports configurable height, text content with two-way binding, and text box style selection.
/// </summary>
public class SldTextBox : SldControl<IPropertyManagerPageTextbox>
{
    static SldTextBox()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(SldTextBox),
            new FrameworkPropertyMetadata(typeof(SldTextBox))
        );
    }

    /// <summary>
    /// Gets or sets the height of the native SolidWorks text box in pixels.
    /// When <c>null</c>, the default height is used.
    /// </summary>
    public int? SldHeight
    {
        get { return (int?)GetValue(SldHeightProperty); }
        set { SetValue(SldHeightProperty, value); }
    }

    /// <summary>
    /// Identifies the <see cref="SldHeight"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty SldHeightProperty = DependencyProperty.Register(
        "SldHeight",
        typeof(int?),
        typeof(SldTextBox),
        new FrameworkPropertyMetadata(
            null,
            FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
            OnSldHeightPropertyChangedCallback
        )
    );

    private static void OnSldHeightPropertyChangedCallback(
        DependencyObject d,
        DependencyPropertyChangedEventArgs e
    )
    {
        var sld = d as SldTextBox;
        sld.OnHeightChanged((int?)e.OldValue, (int?)e.NewValue);
    }

    private void OnHeightChanged(int? oldValue, int? newValue)
    {
        if (SControl != null && newValue != null && oldValue != newValue)
        {
            SControl.Height = (short)newValue.Value;
        }
    }

    /// <summary>
    /// Gets or sets the text content of the native SolidWorks text box.
    /// Binds two-way by default to the native <see cref="IPropertyManagerPageTextbox.Text"/> property.
    /// </summary>
    public string Text
    {
        get { return (string)GetValue(TextProperty); }
        set { SetValue(TextProperty, value); }
    }

    /// <summary>
    /// Identifies the <see cref="Text"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty TextProperty = DependencyProperty.Register(
        "Text",
        typeof(string),
        typeof(SldTextBox),
        new FrameworkPropertyMetadata(
            null,
            FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
            OnTextPropertyCallback
        )
    );

    private static void OnTextPropertyCallback(
        DependencyObject d,
        DependencyPropertyChangedEventArgs e
    )
    {
        var sld = d as SldTextBox;
        sld.OnTextChanged((string)e.OldValue, (string)e.NewValue);
    }

    private void OnTextChanged(string oldValue, string newValue)
    {
        if (SControl == null)
        {
            return;
        }
        if (oldValue == newValue)
        {
            return;
        }
        if (SControl.Text == newValue)
        {
            return;
        }

        SControl.Text = newValue;
    }

    /// <summary>
    /// Gets or sets the style of the native SolidWorks text box.
    /// Defaults to <see cref="swPropMgrPageTextBoxStyle_e.swPropMgrPageTextBoxStyle_NoBorder"/>.
    /// </summary>
    public swPropMgrPageTextBoxStyle_e SldStyle
    {
        get { return (swPropMgrPageTextBoxStyle_e)GetValue(SldStyleProperty); }
        set { SetValue(SldStyleProperty, value); }
    }

    /// <summary>
    /// Identifies the <see cref="SldStyle"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty SldStyleProperty = DependencyProperty.Register(
        "SldStyle",
        typeof(swPropMgrPageTextBoxStyle_e),
        typeof(SldTextBox),
        new PropertyMetadata(
            swPropMgrPageTextBoxStyle_e.swPropMgrPageTextBoxStyle_NoBorder,
            OnStylePropertyCallback
        )
    );

    private static void OnStylePropertyCallback(
        DependencyObject d,
        DependencyPropertyChangedEventArgs e
    )
    {
        var sld = d as SldTextBox;
        sld.OnSldStyleChanged(
            (swPropMgrPageTextBoxStyle_e)e.OldValue,
            (swPropMgrPageTextBoxStyle_e)e.NewValue
        );
    }

    private void OnSldStyleChanged(
        swPropMgrPageTextBoxStyle_e oldValue,
        swPropMgrPageTextBoxStyle_e newValue
    )
    {
        if (SControl != null && oldValue != newValue)
        {
            SControl.Style = (int)newValue;
        }
    }

    ///<inheritdoc/>
    protected override void SetSldControl()
    {
        if (SldHeight != null)
        {
            SControl.Height = (short)SldHeight.Value;
        }
        if (!string.IsNullOrEmpty(Text))
        {
            SControl.Text = Text;
        }
    }
}
