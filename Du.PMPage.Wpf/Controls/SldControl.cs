using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;

namespace Du.PMPage.Wpf.Controls;

public abstract class SldControl : Control
{
    private bool _sldControlVisibility;

    #region Properties

    public int ID { get; protected set; }

    /// <summary>
    /// 用于内部表示是不是已经调用 ShowPage 显示此属性页，从来使某些属性不能设置
    /// </summary>
    public bool SldControlVisibility
    {
        get => _sldControlVisibility;
        internal set
        {
            _sldControlVisibility = value;
            OnSldControlChanged(value);
        }
    }

    public swPropertyManagerPageControlLeftAlign_e SldControlAlign { get; set; } =
        swPropertyManagerPageControlLeftAlign_e.swControlAlign_LeftEdge;

    public bool SldEnabled { get; set; } = true;

    public bool Enabled
    {
        get { return (bool)GetValue(EnabledProperty); }
        set { SetValue(EnabledProperty, value); }
    }

    // Using a DependencyProperty as the backing store for Enabled.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty EnabledProperty = DependencyProperty.Register(
        "Enabled",
        typeof(bool),
        typeof(SldControl),
        new FrameworkPropertyMetadata(
            true,
            FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
            OnEnabledPropertyCallback
        )
    );

    private static void OnEnabledPropertyCallback(
        DependencyObject d,
        DependencyPropertyChangedEventArgs e
    )
    {
        var sld = d as SldControl;
        sld.OnEnableChanged((bool)e.OldValue, (bool)e.NewValue);
    }

    protected virtual void OnEnableChanged(bool oldValue, bool newValue) { }

    public bool SldVisible
    {
        get { return (bool)GetValue(SldVisibleProperty); }
        set { SetValue(SldVisibleProperty, value); }
    }

    // Using a DependencyProperty as the backing store for SldVisible.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty SldVisibleProperty = DependencyProperty.Register(
        "SldVisible",
        typeof(bool),
        typeof(SldControl),
        new FrameworkPropertyMetadata(
            true,
            FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
            OnSldVisiblePropertyCallback
        )
    );

    private static void OnSldVisiblePropertyCallback(
        DependencyObject d,
        DependencyPropertyChangedEventArgs e
    )
    {
        var sld = d as SldControl;
        sld.OnSldVisibleChanged((bool)e.OldValue, (bool)e.NewValue);
    }

    protected abstract void OnSldVisibleChanged(bool oldValue, bool newValue);

    public bool SldSmallGapAbove { get; set; } = false;

    /// <summary>
    /// Caption text for the SolidWorks control
    /// </summary>
    public string SldCaption
    {
        get { return (string)GetValue(SldCaptionProperty); }
        set { SetValue(SldCaptionProperty, value); }
    }

    public static readonly DependencyProperty SldCaptionProperty = DependencyProperty.Register(
        nameof(SldCaption),
        typeof(string),
        typeof(SldControl),
        new PropertyMetadata("")
    );

    public string SldTip { get; set; }

    /// <summary>
    /// Whether the native SolidWorks control has been created
    /// </summary>
    internal bool IsNativeControlCreated { get; set; }

    /// <summary>
    /// Content for design-time rendering. Allows templates with ContentPresenter
    /// to display child elements in the designer.
    /// </summary>
    public object Content
    {
        get { return GetValue(ContentProperty); }
        set { SetValue(ContentProperty, value); }
    }

    public static readonly DependencyProperty ContentProperty = DependencyProperty.Register(
        "Content",
        typeof(object),
        typeof(SldControl),
        new FrameworkPropertyMetadata(null)
    );

    /// <summary>
    /// Whether the control is in design mode
    /// </summary>
    internal static bool IsInDesignMode
    {
        get
        {
            if (Application.Current?.MainWindow != null)
            {
                return DesignerProperties.GetIsInDesignMode(Application.Current.MainWindow);
            }
            return DesignerProperties.GetIsInDesignMode(new DependencyObject());
        }
    }

    #endregion

    internal abstract int AddToPage(SldPMPageBase page, int id);

    internal abstract int AddToGroup(IPropertyManagerPageGroup group, int id);

    internal abstract int AddToTab(IPropertyManagerPageTab tab, int id);

    /// <summary>
    /// Create the native SolidWorks control for this wrapper.
    /// </summary>
    internal abstract void CreateNativeControl(SldPMPageBase page);

    /// <summary>
    /// Destroy the native SolidWorks control.
    /// </summary>
    internal abstract void DestroyNativeControl();

    #region Protected Methods

    protected virtual void OnSldControlChanged(bool value) { }

    protected swAddControlOptions_e GetControlOptions()
    {
        if (SldEnabled && SldVisible && SldSmallGapAbove)
        {
            return swAddControlOptions_e.swControlOptions_Enabled
                | swAddControlOptions_e.swControlOptions_Visible
                | swAddControlOptions_e.swControlOptions_SmallGapAbove;
        }
        else if (SldEnabled && SldVisible)
        {
            return swAddControlOptions_e.swControlOptions_Enabled
                | swAddControlOptions_e.swControlOptions_Visible;
        }
        else if (SldEnabled && SldSmallGapAbove)
        {
            return swAddControlOptions_e.swControlOptions_Enabled
                | swAddControlOptions_e.swControlOptions_SmallGapAbove;
        }
        else if (SldVisible && SldSmallGapAbove)
        {
            return swAddControlOptions_e.swControlOptions_Visible
                | swAddControlOptions_e.swControlOptions_SmallGapAbove;
        }
        else if (SldVisible)
        {
            return swAddControlOptions_e.swControlOptions_Visible;
        }
        else if (SldEnabled)
        {
            return swAddControlOptions_e.swControlOptions_Enabled;
        }
        else if (SldSmallGapAbove)
        {
            return swAddControlOptions_e.swControlOptions_SmallGapAbove;
        }

        throw new ArgumentException(
            $"{nameof(SldEnabled)}:{SldEnabled} ,{nameof(SldVisible)}:{SldVisible}, {nameof(SldSmallGapAbove)}:{SldSmallGapAbove} Error,Cannot not be all false"
        );
    }

    #endregion
}

public abstract class SldControl<TControl> : SldControl
    where TControl : class
{
    #region Properties

    public TControl SControl { get; protected set; }

    public short? SldWidth { get; set; }

    public swControlBitmapLabelType_e? StandardPictureLabel { get; set; }

    #endregion

    #region Methods

    protected void VerifySControlForCreate()
    {
        if (SControl != null)
        {
            throw new InvalidOperationException($"已经创建过{nameof(PropertyManagerPageGroup)},无法再次创建");
        }
    }

    #region Virtual Methods

    internal override int AddToPage(SldPMPageBase page, int id)
    {
        VerifySControlForCreate();

        ID = id;

        short controlType = (short)GetControlType();
        short align = (short)SldControlAlign;
        var options = (int)GetControlOptions();

        SControl =
            page.Page.AddControl2(id, controlType, SldCaption, align, options, SldTip) as TControl;

        if (SControl == null)
        {
            throw new NullReferenceException($"创建{nameof(IPropertyManagerPageSelectionbox)} 错误");
        }

        SetBaseProperties();
        SetSldControl();

        IsNativeControlCreated = true;

        return ++id;
    }

    internal override int AddToGroup(IPropertyManagerPageGroup group, int id)
    {
        VerifySControlForCreate();

        ID = id;

        short controlType = (short)GetControlType();
        short align = (short)SldControlAlign;
        var options = (int)GetControlOptions();

        SControl =
            group.AddControl2(id, controlType, SldCaption, align, options, SldTip) as TControl;

        if (SControl == null)
        {
            throw new NullReferenceException($"创建{nameof(IPropertyManagerPageSelectionbox)} 错误");
        }

        SetBaseProperties();
        SetSldControl();

        IsNativeControlCreated = true;

        return ++id;
    }

    internal override int AddToTab(IPropertyManagerPageTab tab, int id)
    {
        VerifySControlForCreate();

        ID = id;

        short controlType = (short)GetControlType();
        short align = (short)SldControlAlign;
        var options = (int)GetControlOptions();

        SControl = tab.AddControl2(id, controlType, SldCaption, align, options, SldTip) as TControl;

        if (SControl == null)
        {
            throw new NullReferenceException($"创建{typeof(TControl).Name} 错误");
        }

        SetBaseProperties();
        SetSldControl();

        IsNativeControlCreated = true;

        return ++id;
    }

    internal override void CreateNativeControl(SldPMPageBase page)
    {
        // Default implementation delegates to AddToPage
        // Subclasses can override for dual-mode behavior
        AddToPage(page, page.GetNextControlId());
    }

    internal override void DestroyNativeControl()
    {
        SControl = null;
        IsNativeControlCreated = false;
    }

    internal bool SldControlIsVisible()
    {
        if (SControl != null)
        {
            var pageControl = SControl as IPropertyManagerPageControl;
            if (pageControl != null)
                return pageControl.Visible;
            // Control was created but doesn't implement IPropertyManagerPageControl
            // (e.g. WindowFromHandle) — treat as visible.
            return true;
        }
        return false;
    }

    #endregion

    protected override void OnSldVisibleChanged(bool oldValue, bool newValue)
    {
        if (SControl != null && oldValue != newValue)
        {
            var pageControl = SControl as IPropertyManagerPageControl;
            if (pageControl != null)
            {
                pageControl.Visible = newValue;
            }
        }
    }

    protected override void OnEnableChanged(bool oldValue, bool newValue)
    {
        if (SControl != null && oldValue != newValue)
        {
            var pageControl = SControl as IPropertyManagerPageControl;
            if (pageControl != null)
            {
                pageControl.Enabled = newValue;
            }
        }
    }

    private void SetBaseProperties()
    {
        var pageControl = SControl as IPropertyManagerPageControl;
        if (StandardPictureLabel != null)
        {
            pageControl.SetStandardPictureLabel((int)StandardPictureLabel.Value);
        }
        if (SldWidth != null)
        {
            pageControl.Width = SldWidth.Value;
        }
    }

    private swPropertyManagerPageControlType_e GetControlType()
    {
        swPropertyManagerPageControlType_e controlType = default;

        var typeName = typeof(TControl).Name;

        switch (typeName)
        {
            case nameof(IPropertyManagerPageSelectionbox):
                controlType = swPropertyManagerPageControlType_e.swControlType_Selectionbox;
                break;
            case nameof(IPropertyManagerPageCombobox):
                controlType = swPropertyManagerPageControlType_e.swControlType_Combobox;
                break;
            case nameof(IPropertyManagerPageLabel):
                controlType = swPropertyManagerPageControlType_e.swControlType_Label;
                break;
            case nameof(IPropertyManagerPageCheckbox):
                controlType = swPropertyManagerPageControlType_e.swControlType_Checkbox;
                break;
            case nameof(IPropertyManagerPageNumberbox):
                controlType = swPropertyManagerPageControlType_e.swControlType_Numberbox;
                break;
            case nameof(IPropertyManagerPageOption):
                controlType = swPropertyManagerPageControlType_e.swControlType_Option;
                break;
            case nameof(IPropertyManagerPageListbox):
                controlType = swPropertyManagerPageControlType_e.swControlType_Listbox;
                break;
            case nameof(IPropertyManagerPageBitmap):
                controlType = swPropertyManagerPageControlType_e.swControlType_Bitmap;
                break;
            case nameof(IPropertyManagerPageBitmapButton):
                //区分ToggleButton 和 Button
                if (this is SldCheckableBitmapButton)
                {
                    controlType =
                        swPropertyManagerPageControlType_e.swControlType_CheckableBitmapButton;
                }
                else
                {
                    controlType = swPropertyManagerPageControlType_e.swControlType_BitmapButton;
                }
                break;
            case nameof(IPropertyManagerPageButton):
                controlType = swPropertyManagerPageControlType_e.swControlType_Button;
                break;
            case nameof(IPropertyManagerPageGroup):
                throw new NotSupportedException(
                    $"{nameof(IPropertyManagerPageGroup)} Not Support,override the {nameof(AddToPage)} or {nameof(AddToGroup)} Method"
                );
            case nameof(IPropertyManagerPageWindowFromHandle):
                controlType = swPropertyManagerPageControlType_e.swControlType_WindowFromHandle;
                break;
            default:
                throw new InvalidCastException($"{typeName} is not a propertymanagerpage control");
        }

        return controlType;
    }

    #region Abstract Methods

    /// <summary>
    /// 初始化完成后更新控件属性值
    /// </summary>
    protected abstract void SetSldControl();
    #endregion

    #endregion
}
