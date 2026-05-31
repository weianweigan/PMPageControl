using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;
using SolidWorks.Interop.swpublished;

namespace Du.PMPage.Wpf;

/// <summary>
/// Abstract base class for SolidWorks PropertyManagerPage WPF hosts.
/// Provides shared PMPage infrastructure: page creation, ElementHost,
/// IPropertyManagerPage2Handler9 lifecycle, common properties and events.
/// </summary>
[ComVisible(true)]
public abstract class SldPMPageBase : ItemsControl, IPropertyManagerPage2Handler9, IDisposable
{
    #region Constants
    internal const int S_FALSE = 1;
    #endregion

    #region Fields
    private IModelDoc2 _doc;
    private IPropertyManagerPage2 _page;

    /// <summary>Used by derived classes for disposal tracking.</summary>
    protected bool _disposed;
    #endregion

    #region Constructors

    static SldPMPageBase()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(SldPMPageBase),
            new FrameworkPropertyMetadata(typeof(SldPMPageBase))
        );
    }

    /// <summary>
    /// Parameterless constructor for XAML support.
    /// In design mode, sets a fallback template for designer rendering.
    /// </summary>
    public SldPMPageBase()
    {
        if (DesignerProperties.GetIsInDesignMode(this))
        {
            if (Template == null)
                Template = CreateDesignTimeTemplate();
        }
    }

    /// <summary>
    /// Convenience constructor that sets the SolidWorks application object.
    /// </summary>
    public SldPMPageBase(ISldWorks app)
        : this()
    {
        App = app;
    }

    #endregion

    #region Design-Time Helpers

    private static ControlTemplate CreateDesignTimeTemplate()
    {
        var template = new ControlTemplate(typeof(SldPMPageBase));
        var borderFactory = new FrameworkElementFactory(typeof(Border));
        borderFactory.SetValue(
            Border.BackgroundProperty,
            new SolidColorBrush(Color.FromRgb(0xF7, 0xF7, 0xF7))
        );

        var itemsFactory = new FrameworkElementFactory(typeof(ItemsPresenter));

        borderFactory.AppendChild(itemsFactory);
        template.VisualTree = borderFactory;
        return template;
    }

    #endregion

    #region Public Properties

    /// <summary>
    /// SolidWorks application object.
    /// </summary>
    public ISldWorks App
    {
        get { return (ISldWorks)GetValue(AppProperty); }
        set { SetValue(AppProperty, value); }
    }

    public static readonly DependencyProperty AppProperty = DependencyProperty.Register(
        nameof(App),
        typeof(ISldWorks),
        typeof(SldPMPageBase),
        new PropertyMetadata(default(ISldWorks))
    );

    /// <summary>
    /// Title displayed in the SolidWorks PropertyManagerPage header.
    /// </summary>
    public string PageTitle
    {
        get { return (string)GetValue(PageTitleProperty); }
        set { SetValue(PageTitleProperty, value); }
    }

    public static readonly DependencyProperty PageTitleProperty = DependencyProperty.Register(
        nameof(PageTitle),
        typeof(string),
        typeof(SldPMPageBase),
        new PropertyMetadata("PMPage")
    );

    /// <summary>
    /// Height of the WPF content area in dialog units.
    /// </summary>
    public int PageHeight
    {
        get { return (int)GetValue(PageHeightProperty); }
        set { SetValue(PageHeightProperty, value); }
    }

    public static readonly DependencyProperty PageHeightProperty = DependencyProperty.Register(
        nameof(PageHeight),
        typeof(int),
        typeof(SldPMPageBase),
        new PropertyMetadata(500)
    );

    /// <summary>
    /// Whether the page is pinned open.
    /// Getter syncs with native page state.
    /// </summary>
    public bool Pinned
    {
        get
        {
            var pinned = (bool)GetValue(PinnedProperty);
            if (_page != null)
            {
                if (pinned != _page.Pinned)
                {
                    SetValue(PinnedProperty, _page.Pinned);
                    pinned = _page.Pinned;
                }
            }
            return pinned;
        }
        set
        {
            SetValue(PinnedProperty, value);
            if (_page != null)
                _page.Pinned = value;
        }
    }

    public static readonly DependencyProperty PinnedProperty = DependencyProperty.Register(
        nameof(Pinned),
        typeof(bool),
        typeof(SldPMPageBase),
        new PropertyMetadata(false)
    );

    /// <summary>
    /// Command executed when the page is closed.
    /// Receives null on OK, -1 on Cancel.
    /// </summary>
    public ICommand CloseCommand
    {
        get { return (ICommand)GetValue(CloseCommandProperty); }
        set { SetValue(CloseCommandProperty, value); }
    }

    public static readonly DependencyProperty CloseCommandProperty = DependencyProperty.Register(
        "CloseCommand",
        typeof(ICommand),
        typeof(SldPMPageBase),
        new PropertyMetadata(null)
    );

    /// <summary>
    /// Whether page creation is deferred to <see cref="ShowPage()"/>.
    /// Default: true.
    /// </summary>
    public bool PageCreatedWhenShow
    {
        get { return (bool)GetValue(PageCreatedWhenShowProperty); }
        set { SetValue(PageCreatedWhenShowProperty, value); }
    }

    public static readonly DependencyProperty PageCreatedWhenShowProperty =
        DependencyProperty.Register(
            nameof(PageCreatedWhenShow),
            typeof(bool),
            typeof(SldPMPageBase),
            new PropertyMetadata(true)
        );

    /// <summary>
    /// Gets the close reason after the page is closed.
    /// </summary>
    public swPropertyManagerPageCloseReasons_e CloseReason { get; private set; }

    /// <summary>
    /// Gets the native SolidWorks PropertyManagerPage2 object.
    /// </summary>
    public IPropertyManagerPage2 Page => _page;

    /// <summary>
    /// Gets the active SolidWorks document.
    /// </summary>
    internal IModelDoc2 ActiveDoc => _doc;

    #endregion

    #region Events

    /// <summary>
    /// Fired when the PropertyManagerPage is activated.
    /// </summary>
    public event Action AfterActivationEvent;

    /// <summary>
    /// Fired when the user clicks OK.
    /// </summary>
    public event Action OkClicked;

    /// <summary>
    /// Fired when the Help button is clicked.
    /// </summary>
    public event Action HelpRequested;

    /// <summary>
    /// Fired when the "What's New" button is clicked.
    /// </summary>
    public event Action WhatsNewRequested;

    /// <summary>
    /// Fired when the page is closing. Set <see cref="ClosingArg.Cancel"/> = true to prevent close.
    /// </summary>
    public event PropertyManagerPageClosingDelegate Closing;

    /// <summary>
    /// Fired after the page has closed.
    /// </summary>
    public event PropertyManagerPageClosedDelegate Closed;

    #endregion

    #region Delegates

    public delegate void PropertyManagerPageClosedDelegate(
        swPropertyManagerPageCloseReasons_e reason
    );
    public delegate void PropertyManagerPageClosingDelegate(
        swPropertyManagerPageCloseReasons_e reason,
        ClosingArg arg
    );

    #endregion

    #region Show / Close

    /// <summary>
    /// Creates the SW PropertyManagerPage (if not already created), sets up
    /// WPF hosting, and shows the page.
    /// </summary>
    public void ShowPage()
    {
        // Use STA apartment to prevent SolidWorks crashes
        Thread.CurrentThread.TrySetApartmentState(ApartmentState.STA);

        _doc = App?.IActiveDoc2;
        if (_doc == null)
            throw new InvalidOperationException(
                "No active document found. A document must be open to show the PropertyManagerPage."
            );

        if (_page == null)
        {
            CreatePage();
        }

        swPropertyManagerPageStatus_e result;
        try
        {
            Debug.Print("PMPage.Show");
            BeforeShow();
            OnShowPagePreview();
            result = (swPropertyManagerPageStatus_e)_page.Show();
            Debug.Print("PMPage.Showed");
        }
        catch (Exception)
        {
            throw;
        }

        if (result != swPropertyManagerPageStatus_e.swPropertyManagerPage_Okay)
            throw new CreatePMPageErrorException($"ShowPage Error: {result}");

        OnShowPagePost();
    }

    /// <summary>
    /// Pre-create the PMPage without showing it.
    /// Call <see cref="ShowPage()"/> afterward.
    /// </summary>
    public void CreatePage()
    {
        OnCreatePagePreview();

        int errors = 0;
        _page = (PropertyManagerPage2)
            App.CreatePropertyManagerPage(
                PageTitle,
                GetOptions(),
                new PropertyManagerPage2Handler9Wrapper(this),
                ref errors
            );

        var status = (swPropertyManagerPageStatus_e)errors;
        if (status != swPropertyManagerPageStatus_e.swPropertyManagerPage_Okay)
            throw new CreatePMPageErrorException($"CreatePropertyManagerPage Error: {status}");
    }

    /// <summary>
    /// Called after <see cref="CreatePage()"/> and before <see cref="ShowPage()"/>'s ElementHost setup.
    /// Override to create native controls or WindowFromHandle instances.
    /// </summary>
    protected virtual void BeforeShow() { }

    /// <summary>
    /// Called just before <see cref="Page.Show()"/>.
    /// </summary>
    protected virtual void OnShowPagePreview() { }

    /// <summary>
    /// Called just after successful <see cref="Page.Show()"/>.
    /// </summary>
    protected virtual void OnShowPagePost() { }

    /// <summary>
    /// Called before <see cref="CreatePage()"/> creates the native SW page.
    /// </summary>
    protected virtual void OnCreatePagePreview() { }

    /// <summary>
    /// Show a bubble tooltip next to the PropertyManagerPage.
    /// </summary>
    public void ShowBubbleTooltipAt2(
        string title,
        string msg,
        swArrowPosition postion = swArrowPosition.swArrowLeftTop
    )
    {
        App?.ShowBubbleTooltipAt2(
            0,
            0,
            (int)postion,
            title,
            msg,
            (int)swBitMaps.swBitMapTreeError,
            "",
            "",
            0,
            (int)swLinkString.swLinkStringNone,
            "",
            ""
        );
    }

    #endregion

    #region Control ID Management

    /// <summary>
    /// Gets and increments the next available control ID.
    /// Override in derived classes to customize ID allocation.
    /// </summary>
    internal virtual int GetNextControlId() => 0;

    /// <summary>
    /// Sets the Win32 window handle of a WPF ElementHost on a native
    /// <see cref="IPropertyManagerPageWindowFromHandle"/> control (64-bit).
    /// </summary>
    protected static void SetWindowHandle(
        long handle,
        IPropertyManagerPageWindowFromHandle nativeHandle
    )
    {
        if (nativeHandle != null)
            nativeHandle.SetWindowHandlex64(handle);
    }

    #endregion

    #region Options

    /// <summary>
    /// Gets the option flags for the PropertyManagerPage.
    /// Override to customize options. Default: OkayButton | CancelButton.
    /// </summary>
    protected virtual int GetOptions()
    {
        return (int)swPropertyManagerPageOptions_e.swPropertyManagerOptions_OkayButton
            | (int)swPropertyManagerPageOptions_e.swPropertyManagerOptions_CancelButton;
    }

    #endregion

    #region IPropertyManagerPage2Handler9

    public void AfterActivation()
    {
        AfterActivationEvent?.Invoke();
    }

    public void OnWhatsNew()
    {
        WhatsNewRequested?.Invoke();
    }

    public void OnClose(int Reason)
    {
        CloseReason = (swPropertyManagerPageCloseReasons_e)Reason;

        var arg = new ClosingArg();
        Closing?.Invoke(CloseReason, arg);

        if (arg.Cancel)
        {
            if (!string.IsNullOrEmpty(arg.ErrorTitle) || !string.IsNullOrEmpty(arg.ErrorMessage))
            {
                var title = !string.IsNullOrEmpty(arg.ErrorTitle) ? arg.ErrorTitle : "Error";
                ShowBubbleTooltipAt2(title, arg.ErrorMessage);
            }
            throw new COMException(arg.ErrorMessage ?? "Cancelled", S_FALSE);
        }

        if (CloseReason == swPropertyManagerPageCloseReasons_e.swPropertyManagerPageClose_Okay)
        {
            bool canExecute = CloseCommand?.CanExecute(null) ?? true;
            if (!canExecute)
            {
                if (CloseCommand is CloseCommand clsCmd)
                    ShowBubbleTooltipAt2(clsCmd.ErrorTitle, clsCmd.BubbleTooltip);
                else
                    ShowBubbleTooltipAt2("Cannot execute", "Unknown error");
                throw new COMException("Command cannot execute", S_FALSE);
            }
        }
    }

    public void AfterClose()
    {
        if (CloseReason == swPropertyManagerPageCloseReasons_e.swPropertyManagerPageClose_Okay)
        {
            OkClicked?.Invoke();
            CloseCommand?.Execute(null);
        }
        else
        {
            CloseCommand?.Execute(-1);
        }
        Closed?.Invoke(CloseReason);
        Dispose();
    }

    public virtual bool OnHelp()
    {
        HelpRequested?.Invoke();
        return true;
    }

    public virtual bool OnPreviousPage()
    {
        return true;
    }

    public virtual bool OnNextPage()
    {
        return true;
    }

    public bool OnPreview() => true;

    public void OnUndo() { }

    public void OnRedo() { }

    public bool OnTabClicked(int Id) => true;

    public void OnGroupExpand(int Id, bool Expanded) { }

    public void OnGroupCheck(int Id, bool Checked) { }

    public virtual void OnCheckboxCheck(int Id, bool Checked) { }

    public virtual void OnOptionCheck(int Id) { }

    public virtual void OnButtonPress(int Id) { }

    public virtual void OnTextboxChanged(int Id, string Text) { }

    public virtual void OnNumberboxChanged(int Id, double Value) { }

    public virtual void OnComboboxEditChanged(int Id, string Text) { }

    public virtual void OnComboboxSelectionChanged(int Id, int Item) { }

    public void OnListboxSelectionChanged(int Id, int Item) { }

    public void OnSelectionboxFocusChanged(int Id) { }

    public virtual void OnSelectionboxListChanged(int Id, int Count) { }

    public void OnSelectionboxCalloutCreated(int Id) { }

    public void OnSelectionboxCalloutDestroyed(int Id) { }

    public virtual bool OnSubmitSelection(
        int Id,
        object Selection,
        int SelType,
        ref string ItemText
    )
    {
        return true;
    }

    public int OnActiveXControlCreated(int Id, bool Status) => 1;

    public void OnSliderPositionChanged(int Id, double Value) { }

    public void OnSliderTrackingCompleted(int Id, double Value) { }

    public bool OnKeystroke(int Wparam, int Message, int Lparam, int Id) => true;

    public void OnPopupMenuItem(int Id) { }

    public void OnPopupMenuItemUpdate(int Id, ref int retval) { }

    public virtual void OnGainedFocus(int Id) { }

    public void OnLostFocus(int Id) { }

    public virtual int OnWindowFromHandleControlCreated(int Id, bool Status) => -1;

    public void OnListboxRMBUp(int Id, int PosX, int PosY) { }

    public virtual void OnNumberBoxTrackingCompleted(int Id, double Value) { }

    #endregion

    #region IDisposable

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing) { }

            if (_page != null)
            {
                Marshal.FinalReleaseComObject(_page);
                _page = null;
            }

            _doc = null;
            App = null;
            _disposed = true;
        }
    }

    #endregion
}
