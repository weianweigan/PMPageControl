using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;

namespace Du.PMPage.Wpf.Controls;

/// <summary>
/// Abstract base class for all SolidWorks PropertyManagerPage control wrappers.
/// Provides common properties and methods for creating and managing native SW controls.
/// </summary>
public abstract class SldControl : Control
{
    private bool _sldControlVisibility;

    #region Properties

    /// <summary>
    /// Gets the unique identifier assigned by SolidWorks when the control is added to a page,
    /// group, or tab.
    /// </summary>
    public int ID { get; protected set; }

    /// <summary>
    /// Gets or sets whether the property page containing this control has been shown.
    /// When <c>true</c>, certain properties can no longer be modified because the native
    /// control is already displayed.
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

    /// <summary>
    /// Gets or sets the horizontal alignment of the control on the PropertyManager page.
    /// Defaults to <see cref="swPropertyManagerPageControlLeftAlign_e.swControlAlign_LeftEdge"/>.
    /// </summary>
    public swPropertyManagerPageControlLeftAlign_e SldControlAlign { get; set; } =
        swPropertyManagerPageControlLeftAlign_e.swControlAlign_LeftEdge;

    /// <summary>
    /// Gets or sets whether the native SolidWorks control is enabled.
    /// Defaults to <c>true</c>.
    /// </summary>
    public bool SldEnabled { get; set; } = true;

    /// <summary>
    /// Gets or sets whether the control is enabled. Uses a dependency property that supports
    /// animation, styling, and data binding. When the value changes, the native SW control's
    /// <see cref="IPropertyManagerPageControl.Enabled"/> property is updated.
    /// </summary>
    public bool Enabled
    {
        get { return (bool)GetValue(EnabledProperty); }
        set { SetValue(EnabledProperty, value); }
    }

    /// <summary>
    /// Dependency property backing store for <see cref="Enabled"/>.
    /// </summary>
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

    /// <summary>
    /// Called when the <see cref="Enabled"/> property value changes.
    /// Override in derived classes to update the native SW control accordingly.
    /// </summary>
    /// <param name="oldValue">The previous enabled state.</param>
    /// <param name="newValue">The new enabled state.</param>
    protected virtual void OnEnableChanged(bool oldValue, bool newValue) { }

    /// <summary>
    /// Gets or sets whether the native SolidWorks control is visible. Uses a dependency
    /// property that supports animation, styling, and data binding. When the value changes,
    /// the native SW control's <see cref="IPropertyManagerPageControl.Visible"/> property
    /// is updated.
    /// </summary>
    public bool SldVisible
    {
        get { return (bool)GetValue(SldVisibleProperty); }
        set { SetValue(SldVisibleProperty, value); }
    }

    /// <summary>
    /// Dependency property backing store for <see cref="SldVisible"/>.
    /// </summary>
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

    /// <summary>
    /// Called when the <see cref="SldVisible"/> property value changes.
    /// Derived classes must override this to update the native SW control's visibility.
    /// </summary>
    /// <param name="oldValue">The previous visibility state.</param>
    /// <param name="newValue">The new visibility state.</param>
    protected abstract void OnSldVisibleChanged(bool oldValue, bool newValue);

    /// <summary>
    /// Gets or sets whether a small gap is added above this control on the PropertyManager page.
    /// Defaults to <c>false</c>.
    /// </summary>
    public bool SldSmallGapAbove { get; set; } = false;

    /// <summary>
    /// Gets or sets the caption text displayed on the native SolidWorks control.
    /// Uses a dependency property for binding support.
    /// </summary>
    public string SldCaption
    {
        get { return (string)GetValue(SldCaptionProperty); }
        set { SetValue(SldCaptionProperty, value); }
    }

    /// <summary>
    /// Dependency property backing store for <see cref="SldCaption"/>.
    /// </summary>
    public static readonly DependencyProperty SldCaptionProperty = DependencyProperty.Register(
        nameof(SldCaption),
        typeof(string),
        typeof(SldControl),
        new PropertyMetadata("")
    );

    /// <summary>
    /// Gets or sets the tooltip text for the native SolidWorks control.
    /// </summary>
    public string SldTip { get; set; }

    /// <summary>
    /// Whether the native SolidWorks control has been created.
    /// </summary>
    internal bool IsNativeControlCreated { get; set; }

    /// <summary>
    /// Gets or sets the content used for design-time rendering. Allows templates with
    /// <see cref="ContentPresenter"/> to display child elements in the WPF designer.
    /// </summary>
    public object Content
    {
        get { return GetValue(ContentProperty); }
        set { SetValue(ContentProperty, value); }
    }

    /// <summary>
    /// Dependency property backing store for <see cref="Content"/>.
    /// </summary>
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

    /// <summary>
    /// Adds this control to the specified PropertyManager page.
    /// </summary>
    /// <param name="page">The page to add the control to.</param>
    /// <param name="id">The control identifier to use.</param>
    /// <returns>The next available control identifier.</returns>
    internal abstract int AddToPage(SldPMPageBase page, int id);

    /// <summary>
    /// Adds this control to the specified group on a PropertyManager page.
    /// </summary>
    /// <param name="group">The group to add the control to.</param>
    /// <param name="id">The control identifier to use.</param>
    /// <returns>The next available control identifier.</returns>
    internal abstract int AddToGroup(IPropertyManagerPageGroup group, int id);

    /// <summary>
    /// Adds this control to the specified tab on a PropertyManager page.
    /// </summary>
    /// <param name="tab">The tab to add the control to.</param>
    /// <param name="id">The control identifier to use.</param>
    /// <returns>The next available control identifier.</returns>
    internal abstract int AddToTab(IPropertyManagerPageTab tab, int id);

    /// <summary>
    /// Creates the native SolidWorks control for this wrapper.
    /// </summary>
    /// <param name="page">The page that owns this control.</param>
    internal abstract void CreateNativeControl(SldPMPageBase page);

    /// <summary>
    /// Destroys the native SolidWorks control and releases the COM reference.
    /// </summary>
    internal abstract void DestroyNativeControl();

    #region Protected Methods

    /// <summary>
    /// Called when <see cref="SldControlVisibility"/> changes (i.e., the property page
    /// has been shown or hidden). Override in derived classes to react to this event.
    /// </summary>
    /// <param name="value">The new visibility state.</param>
    protected virtual void OnSldControlChanged(bool value) { }

    /// <summary>
    /// Computes the combined <see cref="swAddControlOptions_e"/> flags based on the current
    /// values of <see cref="SldEnabled"/>, <see cref="SldVisible"/>, and
    /// <see cref="SldSmallGapAbove"/>.
    /// </summary>
    /// <returns>A bitwise combination of control option flags.</returns>
    /// <exception cref="ArgumentException">
    /// Thrown when none of the three option properties are <c>true</c>.
    /// </exception>
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
            $"{nameof(SldEnabled)}:{SldEnabled}, {nameof(SldVisible)}:{SldVisible}, {nameof(SldSmallGapAbove)}:{SldSmallGapAbove} — at least one option must be enabled.",
            nameof(SldEnabled));
    }

    #endregion
}

/// <summary>
/// Generic abstract base class for SolidWorks PropertyManagerPage control wrappers.
/// Provides the strongly-typed native SW COM control reference via <see cref="SControl"/>
/// and implements the core logic for creating and destroying native controls.
/// </summary>
/// <typeparam name="TControl">
/// The SolidWorks COM interface type for the native control (e.g.,
/// <see cref="IPropertyManagerPageTextbox"/>, <see cref="IPropertyManagerPageCombobox"/>).
/// </typeparam>
public abstract class SldControl<TControl> : SldControl
    where TControl : class
{
    #region Properties

    /// <summary>
    /// Gets the native SolidWorks COM control instance. This is <c>null</c> until
    /// the control is added to a page, group, or tab.
    /// </summary>
    public TControl SControl { get; protected set; }

    /// <summary>
    /// Gets or sets the width of the native SolidWorks control, in pixels.
    /// If <c>null</c>, the default width is used.
    /// </summary>
    public short? SldWidth { get; set; }

    /// <summary>
    /// Gets or sets the standard picture label for bitmap-type controls.
    /// If <c>null</c>, no standard picture label is applied.
    /// </summary>
    public swControlBitmapLabelType_e? StandardPictureLabel { get; set; }

    #endregion

    #region Methods

    /// <summary>
    /// Verifies that the native SolidWorks control (<see cref="SControl"/>) has not
    /// already been created. Throws <see cref="InvalidOperationException"/> if it has.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// Thrown when <see cref="SControl"/> is not <c>null</c>, indicating the native
    /// control has already been created.
    /// </exception>
    protected void VerifySControlForCreate()
    {
        if (SControl != null)
        {
            throw new InvalidOperationException(
                $"The native {typeof(TControl).Name} control has already been created and cannot be created again.");
        }
    }

    #region Virtual Methods

    /// <inheritdoc/>
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
            throw new NullReferenceException($"Failed to create native control of type {typeof(TControl).Name} on the page.");
        }

        SetBaseProperties();
        SetSldControl();

        IsNativeControlCreated = true;

        return ++id;
    }

    /// <inheritdoc/>
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
            throw new NullReferenceException($"Failed to create native control of type {typeof(TControl).Name} in the group.");
        }

        SetBaseProperties();
        SetSldControl();

        IsNativeControlCreated = true;

        return ++id;
    }

    /// <inheritdoc/>
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
            throw new NullReferenceException($"Failed to create native control of type {typeof(TControl).Name} in the tab.");
        }

        SetBaseProperties();
        SetSldControl();

        IsNativeControlCreated = true;

        return ++id;
    }

    /// <inheritdoc/>
    internal override void CreateNativeControl(SldPMPageBase page)
    {
        // Default implementation delegates to AddToPage.
        // Subclasses can override for dual-mode behavior.
        AddToPage(page, page.GetNextControlId());
    }

    /// <inheritdoc/>
    internal override void DestroyNativeControl()
    {
        if (SControl != null)
        {
            Marshal.FinalReleaseComObject(SControl);
            SControl = null;
        }
        IsNativeControlCreated = false;
    }

    /// <summary>
    /// Returns whether the native SolidWorks control is currently visible.
    /// If <see cref="SControl"/> is <c>null</c>, returns <c>false</c>.
    /// If the control does not implement <see cref="IPropertyManagerPageControl"/>
    /// (e.g., WindowFromHandle), it is treated as always visible.
    /// </summary>
    /// <returns><c>true</c> if the native control is visible; otherwise, <c>false</c>.</returns>
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

    /// <inheritdoc/>
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

    /// <inheritdoc/>
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

    /// <summary>
    /// Applies <see cref="StandardPictureLabel"/> and <see cref="SldWidth"/> to the
    /// native control via <see cref="IPropertyManagerPageControl"/>.
    /// Called after the native control is created.
    /// </summary>
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

    /// <summary>
    /// Resolves the native SolidWorks control type enum value from the generic type
    /// parameter <typeparamref name="TControl"/>.
    /// </summary>
    /// <returns>The <see cref="swPropertyManagerPageControlType_e"/> value for this control.</returns>
    /// <exception cref="NotSupportedException">
    /// Thrown for <see cref="IPropertyManagerPageGroup"/> and <see cref="IPropertyManagerPageTab"/>,
    /// which are not directly supported as controls.
    /// </exception>
    /// <exception cref="InvalidCastException">
    /// Thrown when <typeparamref name="TControl"/> does not map to a known control type.
    /// </exception>
    private swPropertyManagerPageControlType_e GetControlType()
    {
        swPropertyManagerPageControlType_e controlType = default;

        var typeName = typeof(TControl).Name;

        switch (typeName)
        {
            case nameof(IPropertyManagerPageTextbox):
                controlType = swPropertyManagerPageControlType_e.swControlType_Textbox;
                break;
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
                // Distinguish between ToggleButton and Button based on the wrapper type.
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
                    $"{nameof(IPropertyManagerPageGroup)} is not directly supported. Override the {nameof(AddToPage)} or {nameof(AddToGroup)} method instead."
                );
            case nameof(IPropertyManagerPageWindowFromHandle):
                controlType = swPropertyManagerPageControlType_e.swControlType_WindowFromHandle;
                break;
            case nameof(IPropertyManagerPageTab):
                throw new NotSupportedException(
                    $"{nameof(IPropertyManagerPageTab)} is not supported. Override the {nameof(AddToPage)} or {nameof(AddToGroup)} method instead."
                );
            default:
                throw new InvalidCastException($"{typeName} is not a propertymanagerpage control");
        }

        return controlType;
    }

    #region Abstract Methods

    /// <summary>
    /// Called after the native SolidWorks control has been created and base properties
    /// have been applied. Derived classes must override this to configure additional
    /// control-specific properties on the native <see cref="SControl"/> instance.
    /// </summary>
    protected abstract void SetSldControl();
    #endregion

    #endregion
}
