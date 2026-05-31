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

    #region Dendpedncy Properties
    /// <summary>
    /// Gets or sets the background color of this PropertyManager group box.
    /// </summary>
    public Color? BackgroundColor
    {
        get { return (Color?)GetValue(BackgroundColorProperty); }
        set { SetValue(BackgroundColorProperty, value); }
    }

    /// <summary>
    /// 用来表示 <see cref="BackgroundColor"/> 的依赖属性
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
    /// 用来标识 <see cref="Caption"/> 的依赖属性
    /// </summary>
    public static readonly DependencyProperty CaptionProperty = DependencyProperty.Register(
        "Caption",
        typeof(string),
        typeof(SldGroupBox),
        new PropertyMetadata("Expander", OnCaptionChanged)
    );

    public bool Checked
    {
        get { return (bool)GetValue(CheckedProperty); }
        set { SetValue(CheckedProperty, value); }
    }

    // Using a DependencyProperty as the backing store for Checked.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty CheckedProperty = DependencyProperty.Register(
        "Checked",
        typeof(bool),
        typeof(SldGroupBox),
        new PropertyMetadata(false, OnCheckedChanged)
    );

    public bool Expanded
    {
        get { return (bool)GetValue(ExpandedProperty); }
        set { SetValue(ExpandedProperty, value); }
    }

    // Using a DependencyProperty as the backing store for Expanded.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty ExpandedProperty = DependencyProperty.Register(
        "Expanded",
        typeof(bool),
        typeof(SldGroupBox),
        new PropertyMetadata(true, OnExpandedChanged)
    );

    public bool Visible
    {
        get { return (bool)GetValue(VisibleProperty); }
        set { SetValue(VisibleProperty, value); }
    }

    // Using a DependencyProperty as the backing store for Visible.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty VisibleProperty = DependencyProperty.Register(
        "Visible",
        typeof(bool),
        typeof(SldGroupBox),
        new PropertyMetadata(true, OnVisibleChanged)
    );

    public bool HasCheckBox
    {
        get { return (bool)GetValue(HasCheckBoxProperty); }
        set { SetValue(HasCheckBoxProperty, value); }
    }

    // Using a DependencyProperty as the backing store for HasCheckBox.  This enables animation, styling, binding, etc...
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
            throw new InvalidOperationException($"创建后不能修改值:{nameof(HasCheckBox)}");
        }
    }

    #endregion

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
    /// 修改 背景色
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
            throw new NullReferenceException($"添加GroupBox错误:{Caption}");
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
                ;
            }
            SControl.Expanded = Expanded;
        }

        //添加子控件
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
        throw new InvalidOperationException($"无法将Group添加到Group中");
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
            throw new NullReferenceException($"添加GroupBox错误:{Caption}");
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

        //添加子控件
        foreach (var child in Children)
        {
            if (child is not SldContentHost)
                AddVisualChild(child);
            id = child.AddToGroup(SControl, id);
        }

        return ++id;
    }

    protected override void OnSldControlChanged(bool value)
    {
        Children.ForEach(p => p.SldControlVisibility = true);
    }

    protected override void SetSldControl() { }

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
