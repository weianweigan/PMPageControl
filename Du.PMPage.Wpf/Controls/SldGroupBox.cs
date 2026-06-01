using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;
using Du.PMPage.Wpf.Utils;
using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;

namespace Du.PMPage.Wpf.Controls;

/// <summary>
/// A WPF wrapper for the SolidWorks <see cref="IPropertyManagerPageGroup"/> control.
/// Represents a collapsible group box that can contain child <see cref="SldControl"/> elements.
/// Supports background color, caption, checked state, expand/collapse, visibility,
/// and an optional checkbox via <see cref="HasCheckBox"/>.
/// </summary>
[ContentProperty("Children")]
public class SldGroupBox : SldControl<IPropertyManagerPageGroup>
{
    static SldGroupBox()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(SldGroupBox),
            new FrameworkPropertyMetadata(typeof(SldGroupBox))
        );
    }

    #region Dependency Properties
    /// <summary>
    /// Gets or sets the background color of this PropertyManager group box.
    /// </summary>
    public Color? BackgroundColor
    {
        get { return (Color?)GetValue(BackgroundColorProperty); }
        set { SetValue(BackgroundColorProperty, value); }
    }

    /// <summary>
    /// Identifies the <see cref="BackgroundColor"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty BackgroundColorProperty = DependencyProperty.Register(
        "BackgroundColor",
        typeof(Color?),
        typeof(SldGroupBox),
        new PropertyMetadata(null, OnBackgroundColorChanged)
    );

    /// <summary>
    /// Gets or sets the title for this group box.
    /// </summary>
    public string Caption
    {
        get { return (string)GetValue(CaptionProperty); }
        set { SetValue(CaptionProperty, value); }
    }

    /// <summary>
    /// Identifies the <see cref="Caption"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty CaptionProperty = DependencyProperty.Register(
        "Caption",
        typeof(string),
        typeof(SldGroupBox),
        new PropertyMetadata("Expander", OnCaptionChanged)
    );

    /// <summary>
    /// Gets or sets a value indicating whether the group box checkbox is checked.
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
        typeof(SldGroupBox),
        new PropertyMetadata(false, OnCheckedChanged)
    );

    /// <summary>
    /// Gets or sets a value indicating whether the group box is expanded.
    /// The default is <c>true</c>.
    /// </summary>
    public bool Expanded
    {
        get { return (bool)GetValue(ExpandedProperty); }
        set { SetValue(ExpandedProperty, value); }
    }

    /// <summary>
    /// Identifies the <see cref="Expanded"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty ExpandedProperty = DependencyProperty.Register(
        "Expanded",
        typeof(bool),
        typeof(SldGroupBox),
        new PropertyMetadata(true, OnExpandedChanged)
    );

    /// <summary>
    /// Gets or sets a value indicating whether the group box is visible.
    /// The default is <c>true</c>.
    /// </summary>
    public bool Visible
    {
        get { return (bool)GetValue(VisibleProperty); }
        set { SetValue(VisibleProperty, value); }
    }

    /// <summary>
    /// Identifies the <see cref="Visible"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty VisibleProperty = DependencyProperty.Register(
        "Visible",
        typeof(bool),
        typeof(SldGroupBox),
        new PropertyMetadata(true, OnVisibleChanged)
    );

    /// <summary>
    /// Gets or sets a value indicating whether the group box displays a checkbox.
    /// This property cannot be changed after the native control has been created.
    /// </summary>
    public bool HasCheckBox
    {
        get { return (bool)GetValue(HasCheckBoxProperty); }
        set { SetValue(HasCheckBoxProperty, value); }
    }

    /// <summary>
    /// Identifies the <see cref="HasCheckBox"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty HasCheckBoxProperty = DependencyProperty.Register(
        "HasCheckBox",
        typeof(bool),
        typeof(SldGroupBox),
        new PropertyMetadata(false, OnCheckBoxChanged)
    );

    private static void OnCheckBoxChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var group = d as SldGroupBox;
        group.OnCheckBoxChanged((bool)e.OldValue, (bool)e.NewValue);
    }

    private void OnCheckBoxChanged(bool oldValue, bool newValue)
    {
        if (oldValue != newValue && SControl != null)
        {
            throw new InvalidOperationException($"Cannot change {nameof(HasCheckBox)} after the native control has been created.");
        }
    }

    #endregion

    /// <summary>
    /// Gets the collection of child <see cref="SldControl"/> elements contained within this group box.
    /// </summary>
    public List<SldControl> Children { get; } = new List<SldControl>();

    #region Private Static Methods

    private static void OnBackgroundColorChanged(
        DependencyObject d,
        DependencyPropertyChangedEventArgs e
    )
    {
        var group = d as SldGroupBox;
        group.OnBackgroundColorChanged((Color?)e.OldValue, (Color?)e.NewValue);
    }

    private static void OnCaptionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var group = d as SldGroupBox;
        group.OnCaptionChanged((string)e.OldValue, (string)e.NewValue);
    }

    private static void OnCheckedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var group = d as SldGroupBox;
        group.OnCheckedChanged((bool)e.OldValue, (bool)e.NewValue);
    }

    private static void OnExpandedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var group = d as SldGroupBox;
        group.OnExpandedPropertyChanged((bool)e.OldValue, (bool)e.NewValue);
    }

    private static void OnVisibleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var group = d as SldGroupBox;
        group.OnVisibleChanged((bool)e.OldValue, (bool)e.NewValue);
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// Applies the background color change to the native group box control.
    /// </summary>
    private void OnBackgroundColorChanged(Color? oldValue, Color? newValue)
    {
        if (oldValue != newValue && newValue != null && SControl != null)
        {
            SControl.BackgroundColor = Information.RGB(
                newValue.Value.R,
                newValue.Value.G,
                newValue.Value.B
            );
        }
    }

    private void OnCaptionChanged(string oldValue, string newValue)
    {
        if (oldValue != newValue && SControl != null)
        {
            SControl.Caption = newValue;
        }
    }

    private void OnCheckedChanged(bool oldValue, bool newValue)
    {
        if (oldValue != newValue && SControl != null)
        {
            SControl.Checked = newValue;
        }
    }

    private void OnExpandedPropertyChanged(bool oldValue, bool newValue)
    {
        if (oldValue != newValue && SControl != null)
        {
            SControl.Expanded = newValue;
        }
    }

    private void OnVisibleChanged(bool oldValue, bool newValue)
    {
        if (oldValue != newValue && SControl != null)
        {
            SControl.Visible = newValue;
        }
    }

    #endregion

    #region Public Methods

    internal override int AddToPage(SldPMPageBase page, int id)
    {
        VerifySControlForCreate();

        ID = id;

        var option = (int)(
            swAddGroupBoxOptions_e.swGroupBoxOptions_Expanded
            | swAddGroupBoxOptions_e.swGroupBoxOptions_Visible
        );

        SControl = page.Page.AddGroupBox(id, Caption, option) as IPropertyManagerPageGroup;

        id++;

        if (SControl == null)
        {
            throw new NullReferenceException($"Failed to add GroupBox: {Caption}");
        }
        else
        {
            SControl.Visible = Visible;
            if (HasCheckBox)
                SControl.Checked = Checked;

            if (BackgroundColor != null)
            {
                SControl.BackgroundColor = Information.RGB(
                    BackgroundColor.Value.R,
                    BackgroundColor.Value.G,
                    BackgroundColor.Value.B
                );
            }
            SControl.Expanded = Expanded;
        }

        // Add child controls.
        foreach (var child in Children)
        {
            // SldContentHost will be parented to an ElementHost at runtime,
            // so skip AddVisualChild to avoid "Visual is already a child" error.
            if (child is not SldContentHost)
                AddVisualChild(child);

            id = child.AddToGroup(SControl, id);
        }

        return ++id;
    }

    internal override int AddToGroup(IPropertyManagerPageGroup group, int id)
    {
        throw new InvalidOperationException("Cannot add a GroupBox to another GroupBox. GroupBoxes can only be added to pages or tabs.");
    }

    internal override int AddToTab(IPropertyManagerPageTab tab, int id)
    {
        VerifySControlForCreate();

        ID = id;

        var option = (int)(
            swAddGroupBoxOptions_e.swGroupBoxOptions_Expanded
            | swAddGroupBoxOptions_e.swGroupBoxOptions_Visible
        );

        SControl = tab.AddGroupBox(id, Caption, option) as IPropertyManagerPageGroup;

        id++;

        if (SControl == null)
        {
            throw new NullReferenceException($"Failed to add GroupBox: {Caption}");
        }
        else
        {
            SControl.Visible = Visible;
            if (HasCheckBox)
                SControl.Checked = Checked;

            if (BackgroundColor != null)
            {
                SControl.BackgroundColor = Information.RGB(
                    BackgroundColor.Value.R,
                    BackgroundColor.Value.G,
                    BackgroundColor.Value.B
                );
            }
            SControl.Expanded = Expanded;
        }

        // Add child controls.
        foreach (var child in Children)
        {
            if (child is not SldContentHost)
                AddVisualChild(child);
            id = child.AddToGroup(SControl, id);
        }

        return ++id;
    }

    /// <summary>
    /// Sets the visibility of all child controls to <c>true</c>
    /// when the native group box becomes visible.
    /// </summary>
    /// <param name="value">The new visibility state.</param>
    protected override void OnSldControlChanged(bool value)
    {
        Children.ForEach(p => p.SldControlVisibility = true);
    }

    /// <summary>
    /// No-op. The group box has no standard control properties to initialize.
    /// </summary>
    protected override void SetSldControl() { }

    /// <summary>
    /// Applies the control template and populates design-time content
    /// from the <see cref="Children"/> collection when in a designer.
    /// </summary>
    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
        if (DesignerProperties.GetIsInDesignMode(this))
        {
            EnsureDesignTimeContent();
        }
    }

    /// <summary>
    /// Ensures design-time content is rendered from Children collection.
    /// Called by the parent page when building design-time preview.
    /// </summary>
    internal void EnsureDesignTimeContent()
    {
        if (DesignerProperties.GetIsInDesignMode(this) && Content == null && Children.Count > 0)
        {
            var panel = new StackPanel();
            foreach (var child in Children)
            {
                if (child is UIElement element)
                    panel.Children.Add(element);
            }
            Content = panel;
        }
    }

    #endregion
}
