using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using SolidWorks.Interop.sldworks;

namespace Du.PMPage.Wpf.Controls;

/// <summary>
/// A WPF control that wraps a native SolidWorks <see cref="IPropertyManagerPageTab"/>.
/// Declare <see cref="SldControl"/> children inside this tab in XAML; they will be
/// added to the native tab via <see cref="IPropertyManagerPageTab.AddControl2"/>.
///
/// Tabs are created via <see cref="IPropertyManagerPage2.AddTab(int, string, string, int)"/>
/// and tab clicks are handled by the <c>IPropertyManagerPage2Handler9.OnTabClicked</c> callback.
/// </summary>
[ContentProperty("Children")]
public class SldTabControl : SldControl<IPropertyManagerPageTab>
{
    #region Static Constructor

    static SldTabControl()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(SldTabControl),
            new FrameworkPropertyMetadata(typeof(SldTabControl))
        );
    }

    #endregion

    #region Dependency Properties

    /// <summary>
    /// The caption text displayed on this tab.
    /// </summary>
    public string Caption
    {
        get { return (string)GetValue(CaptionProperty); }
        set { SetValue(CaptionProperty, value); }
    }

    public static readonly DependencyProperty CaptionProperty = DependencyProperty.Register(
        nameof(Caption),
        typeof(string),
        typeof(SldTabControl),
        new PropertyMetadata("Tab", OnCaptionChanged)
    );

    private static void OnCaptionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var tab = d as SldTabControl;
        tab.OnCaptionChanged((string)e.OldValue, (string)e.NewValue);
    }

    private void OnCaptionChanged(string oldValue, string newValue)
    {
        if (oldValue != newValue && SControl != null)
        {
            SControl.Caption = newValue;
        }
    }

    #endregion

    #region Properties

    /// <summary>
    /// Child controls that will be added to this tab.
    /// </summary>
    public List<SldControl> Children { get; } = [];

    #endregion

    #region AddToPage / AddToGroup / AddToTab

    internal override int AddToPage(SldPMPageBase page, int id)
    {
        VerifySControlForCreate();

        ID = id;

        // Create native tab via Page.AddTab(ID, Caption, Bitmap, Options)
        SControl = page.Page.AddTab(id, Caption, "", 0) as IPropertyManagerPageTab;

        if (SControl == null)
        {
            throw new NullReferenceException($"添加Tab错误:{Caption}");
        }

        id++;

        IsNativeControlCreated = true;

        // Add child controls to this tab
        foreach (var child in Children)
        {
            // SldContentHost will be parented to an ElementHost at runtime,
            // so skip AddVisualChild to avoid "Visual is already a child" error.
            if (child is not SldContentHost)
                AddVisualChild(child);
            id = child.AddToTab(SControl, id);
        }

        return id;
    }

    internal override int AddToGroup(IPropertyManagerPageGroup group, int id)
    {
        throw new InvalidOperationException("无法将Tab添加到Group中");
    }

    internal override int AddToTab(IPropertyManagerPageTab tab, int id)
    {
        throw new InvalidOperationException("无法将Tab添加到Tab中");
    }

    #endregion

    #region Overrides

    protected override void SetSldControl() { }

    protected override void OnSldControlChanged(bool value)
    {
        Children.ForEach(p => p.SldControlVisibility = value);
    }

    #endregion

    #region Design-Time

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
    /// Called when building design-time preview.
    /// </summary>
    internal void EnsureDesignTimeContent()
    {
        if (DesignerProperties.GetIsInDesignMode(this) && Content == null && Children.Count > 0)
        {
            var panel = new StackPanel();
            foreach (var child in Children)
            {
                if (child is UIElement element)
                {
                    if (child is SldGroupBox groupBox)
                        groupBox.EnsureDesignTimeContent();
                    panel.Children.Add(element);
                }
            }
            Content = panel;
        }
    }

    #endregion
}
