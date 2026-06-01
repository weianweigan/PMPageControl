using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms.Integration;
using System.Windows.Markup;
using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;

namespace Du.PMPage.Wpf.Controls;

/// <summary>
/// A WPF control that wraps an <see cref="IPropertyManagerPageWindowFromHandle"/>
/// in the SolidWorks PMPage. Extends <see cref="SldControl{TControl}"/> so it
/// participates in the standard <c>AddToPage</c> / <c>AddToGroup</c> / <c>AddToTab</c>
/// lifecycle alongside all other <see cref="SldControl"/> types.
///
/// XAML content syntax:
/// <code>
/// &lt;controls:SldContentHost PageHeight="200"&gt;
///     &lt;StackPanel&gt;...&lt;/StackPanel&gt;
/// &lt;/controls:SldContentHost&gt;
/// </code>
/// The <see cref="SldControl.Content"/> property (inherited from <see cref="SldControl"/>)
/// receives the child WPF element.
/// </summary>
[ContentProperty(nameof(Content))]
public class SldContentHost : SldControl<IPropertyManagerPageWindowFromHandle>
{
    #region Fields

    private bool _hasRetry;

    internal ElementHost ElementHost { get; set; }

    #endregion

    #region Static Constructor

    static SldContentHost()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(SldContentHost),
            new FrameworkPropertyMetadata(typeof(SldContentHost))
        );
    }

    #endregion

    #region Dependency Properties

    /// <summary>
    /// The height of this content area in the SW PMPage (in SW dialog units).
    /// Changing this after the native handle is created updates it reactively.
    /// </summary>
    public int PageHeight
    {
        get { return (int)GetValue(PageHeightProperty); }
        set { SetValue(PageHeightProperty, value); }
    }

    public static readonly DependencyProperty PageHeightProperty = DependencyProperty.Register(
        nameof(PageHeight),
        typeof(int),
        typeof(SldContentHost),
        new FrameworkPropertyMetadata(200, OnPageHeightChanged)
    );

    private static void OnPageHeightChanged(
        DependencyObject d,
        DependencyPropertyChangedEventArgs e
    )
    {
        var host = (SldContentHost)d;
        if (host.SControl != null)
            host.SControl.Height = (int)e.NewValue;
    }

    #endregion

    public bool UseSystemFont { get; set; } = true;

    #region Overrides (SldControl integration)

    /// <summary>
    /// Empty — <see cref="IPropertyManagerPageWindowFromHandle"/> has no standard
    /// properties to initialize like Caption/Checked/etc.
    /// </summary>
    protected override void SetSldControl() { }

    /// <summary>
    /// No-op — <see cref="IPropertyManagerPageWindowFromHandle"/> does not implement
    /// <see cref="IPropertyManagerPageControl"/>, so Visible cannot be toggled.
    /// </summary>
    protected override void OnSldVisibleChanged(bool oldValue, bool newValue) { }

    /// <summary>
    /// No-op — same reason as <see cref="OnSldVisibleChanged"/>.
    /// </summary>
    protected override void OnEnableChanged(bool oldValue, bool newValue) { }

    /// <summary>
    /// Returns true if the native handle has been created. WindowFromHandle controls
    /// are always "visible" once created.
    /// </summary>
    internal new bool SldControlIsVisible()
    {
        return SControl != null;
    }

    #endregion

    #region Binding (ElementHost + SetWindowHandlex64)

    /// <summary>
    /// Creates the <see cref="ElementHost"/>, sets this control as its child,
    /// and binds the WPF window handle to the native SW WindowFromHandle control
    /// via <see cref="IPropertyManagerPageWindowFromHandle.SetWindowHandlex64(long)"/>.
    /// Call this during <see cref="SldPMPageBase.OnShowPagePreview"/>.
    /// </summary>
    internal void BindWindowHandle()
    {
        if (DesignerProperties.GetIsInDesignMode(this))
            return;

        if (SControl == null)
            return;

        if (ElementHost != null)
            return;

        // Detach from current WPF parent (ItemsControl) before becoming
        // an ElementHost child, otherwise WPF throws "Visual is already a child".
        if (Parent is ItemsControl itemsControl)
            itemsControl.Items.Remove(this);

        if (Content is UIElement ui)
        {
            ElementHost = new ElementHost
            {
                Child = ui,
            };
            if (UseSystemFont)
            {
                ElementHost.Font = System.Drawing.SystemFonts.MessageBoxFont;
            }
        }

        SControl.SetWindowHandlex64(ElementHost.Handle.ToInt64());
        SControl.Height = PageHeight;
    }

    #endregion

    #region Callback (retry logic)

    /// <summary>
    /// Handles the SW callback for WindowFromHandle control creation status.
    /// Implements a single-retry mechanism.
    /// </summary>
    internal int HandleWindowFromHandleCreatedCallback(int controlId, bool status)
    {
        if (controlId != ID)
            return -1;

        if (status)
        {
            _hasRetry = false;
            return -1;
        }

        if (!_hasRetry)
        {
            _hasRetry = true;
            return (int)
                swHandleWindowFromHandleCreationFailure_e.swHandleWindowFromHandleCreationFailure_Retry;
        }

        return (int)
            swHandleWindowFromHandleCreationFailure_e.swHandleWindowFromHandleCreationFailure_Cancel;
    }

    #endregion

    #region Cleanup

    /// <summary>
    /// Releases all native resources: disposes the ElementHost and releases
    /// the COM reference to the native WindowFromHandle.
    /// </summary>
    internal void CleanupNativeHandle()
    {
        if (ElementHost != null)
        {
            ElementHost.Parent = null;
            ElementHost.Child = null;
            ElementHost.Dispose();
            ElementHost = null;
        }

        if (SControl != null)
        {
            Marshal.FinalReleaseComObject(SControl);
            SControl = null;
        }

        IsNativeControlCreated = false;
        _hasRetry = false;
    }

    #endregion
}
