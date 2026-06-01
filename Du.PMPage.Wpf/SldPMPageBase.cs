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

    /// <summary>
    /// Identifies the <see cref="App"/> dependency property.
    /// </summary>
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

    /// <summary>
    /// Identifies the <see cref="PageTitle"/> dependency property.
    /// </summary>
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

    /// <summary>
    /// Identifies the <see cref="PageHeight"/> dependency property.
    /// </summary>
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

    /// <summary>
    /// Identifies the <see cref="Pinned"/> dependency property.
    /// </summary>
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

    /// <summary>
    /// Identifies the <see cref="CloseCommand"/> dependency property.
    /// </summary>
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

    /// <summary>
    /// Identifies the <see cref="PageCreatedWhenShow"/> dependency property.
    /// </summary>
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

    /// <summary>
    /// Delegate for the <see cref="Closed"/> event. Raised after the
    /// PropertyManagerPage has been closed.
    /// </summary>
    /// <param name="reason">The reason the page was closed.</param>
    public delegate void PropertyManagerPageClosedDelegate(
        swPropertyManagerPageCloseReasons_e reason
    );

    /// <summary>
    /// Delegate for the <see cref="Closing"/> event. Raised when the
    /// PropertyManagerPage is about to close. Set
    /// <see cref="ClosingArg.Cancel"/> to <see langword="true"/> to prevent
    /// the page from closing, and optionally provide an error title and
    /// message via <see cref="ClosingArg.ErrorTitle"/> and
    /// <see cref="ClosingArg.ErrorMessage"/>.
    /// </summary>
    /// <param name="reason">The reason the page is closing.</param>
    /// <param name="arg">The closing argument; set <see cref="ClosingArg.Cancel"/>
    /// to <see langword="true"/> to block the close.</param>
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
        // Ensure the current thread is STA for SolidWorks COM interop.
        if (!Thread.CurrentThread.TrySetApartmentState(ApartmentState.STA))
        {
            if (Thread.CurrentThread.GetApartmentState() != ApartmentState.STA)
            {
                throw new InvalidOperationException(
                    "The current thread must be in STA mode to show a SolidWorks PropertyManagerPage. " +
                    "Ensure the calling thread is an STA thread (e.g., the main WPF UI thread).");
            }
        }

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
        Debug.Print("PMPage.Show");
        BeforeShow();
        OnShowPagePreview();
        result = (swPropertyManagerPageStatus_e)_page.Show();
        Debug.Print("PMPage.Showed");

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
    /// Called just before <c>Page.Show()</c>.
    /// </summary>
    protected virtual void OnShowPagePreview() { }

    /// <summary>
    /// Called just after successful <c>Page.Show()</c>.
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

    /// <summary>
    /// Called by SolidWorks after the PropertyManagerPage is activated.
    /// Fires the <see cref="AfterActivationEvent"/>.
    /// </summary>
    public void AfterActivation()
    {
        AfterActivationEvent?.Invoke();
    }

    /// <summary>
    /// Called by SolidWorks when the "What's New" button is clicked.
    /// Fires the <see cref="WhatsNewRequested"/> event.
    /// </summary>
    public void OnWhatsNew()
    {
        WhatsNewRequested?.Invoke();
    }

    /// <summary>
    /// Called by SolidWorks when the PropertyManagerPage is closing.
    /// Raises the <see cref="Closing"/> event; if cancellation is requested
    /// via <see cref="ClosingArg.Cancel"/>, throws a COM exception to
    /// prevent the page from closing. Also validates
    /// <see cref="CloseCommand"/> before allowing an OK close.
    /// </summary>
    /// <param name="Reason">
    /// The close reason from SolidWorks, cast to
    /// <see cref="swPropertyManagerPageCloseReasons_e"/>.
    /// </param>
    /// <exception cref="COMException">
    /// Thrown with <c>S_FALSE</c> when closing is cancelled by the handler
    /// or the close command cannot execute.
    /// </exception>
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

    /// <summary>
    /// Called by SolidWorks after the PropertyManagerPage has closed.
    /// On OK, fires <see cref="OkClicked"/> and executes
    /// <see cref="CloseCommand"/> with <see langword="null"/>; otherwise
    /// passes <c>-1</c>. Then fires <see cref="Closed"/> and calls
    /// <see cref="Dispose()"/>.
    /// </summary>
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

    /// <summary>
    /// Called by SolidWorks when the Help button is clicked.
    /// Fires the <see cref="HelpRequested"/> event.
    /// </summary>
    /// <returns>Always returns <see langword="true"/>.</returns>
    public virtual bool OnHelp()
    {
        HelpRequested?.Invoke();
        return true;
    }

    /// <summary>
    /// Called by SolidWorks when navigating to the previous page in a
    /// multi-page PropertyManagerPage. Override to handle page switching.
    /// </summary>
    /// <returns>
    /// <see langword="true"/> to allow the navigation;
    /// <see langword="false"/> to block it.
    /// </returns>
    public virtual bool OnPreviousPage()
    {
        return true;
    }

    /// <summary>
    /// Called by SolidWorks when navigating to the next page in a
    /// multi-page PropertyManagerPage. Override to handle page switching.
    /// </summary>
    /// <returns>
    /// <see langword="true"/> to allow the navigation;
    /// <see langword="false"/> to block it.
    /// </returns>
    public virtual bool OnNextPage()
    {
        return true;
    }

    /// <summary>
    /// Called by SolidWorks when the PropertyManagerPage is previewed.
    /// </summary>
    /// <returns>Always returns <see langword="true"/>.</returns>
    public bool OnPreview() => true;

    /// <summary>
    /// Called by SolidWorks when the user triggers Undo.
    /// </summary>
    public void OnUndo() { }

    /// <summary>
    /// Called by SolidWorks when the user triggers Redo.
    /// </summary>
    public void OnRedo() { }

    /// <summary>
    /// Called by SolidWorks when a tab is clicked.
    /// </summary>
    /// <param name="Id">The native control ID of the tab.</param>
    /// <returns>Always returns <see langword="true"/>.</returns>
    public bool OnTabClicked(int Id) => true;

    /// <summary>
    /// Called by SolidWorks when a group is expanded or collapsed.
    /// </summary>
    /// <param name="Id">The native control ID of the group.</param>
    /// <param name="Expanded"><see langword="true"/> if expanded, <see langword="false"/> if collapsed.</param>
    public void OnGroupExpand(int Id, bool Expanded) { }

    /// <summary>
    /// Called by SolidWorks when a group check state changes.
    /// </summary>
    /// <param name="Id">The native control ID of the group.</param>
    /// <param name="Checked">The new check state.</param>
    public void OnGroupCheck(int Id, bool Checked) { }

    /// <summary>
    /// Called by SolidWorks when a checkbox is toggled. Override to handle
    /// check state changes.
    /// </summary>
    /// <param name="Id">The native control ID of the checkbox.</param>
    /// <param name="Checked">The new check state.</param>
    public virtual void OnCheckboxCheck(int Id, bool Checked) { }

    /// <summary>
    /// Called by SolidWorks when an option button is selected. Override to
    /// handle option changes.
    /// </summary>
    /// <param name="Id">The native control ID of the option button.</param>
    public virtual void OnOptionCheck(int Id) { }

    /// <summary>
    /// Called by SolidWorks when a button is pressed. Override to handle
    /// button clicks.
    /// </summary>
    /// <param name="Id">The native control ID of the button.</param>
    public virtual void OnButtonPress(int Id) { }

    /// <summary>
    /// Called by SolidWorks when text in a text box changes. Override to
    /// react to text changes.
    /// </summary>
    /// <param name="Id">The native control ID of the text box.</param>
    /// <param name="Text">The current text content.</param>
    public virtual void OnTextboxChanged(int Id, string Text) { }

    /// <summary>
    /// Called by SolidWorks when a number box value changes. Override to
    /// react to value changes.
    /// </summary>
    /// <param name="Id">The native control ID of the number box.</param>
    /// <param name="Value">The current numeric value.</param>
    public virtual void OnNumberboxChanged(int Id, double Value) { }

    /// <summary>
    /// Called by SolidWorks when the text in a combo box edit field changes.
    /// Override to handle text input.
    /// </summary>
    /// <param name="Id">The native control ID of the combo box.</param>
    /// <param name="Text">The current edit text.</param>
    public virtual void OnComboboxEditChanged(int Id, string Text) { }

    /// <summary>
    /// Called by SolidWorks when the selection in a combo box changes.
    /// Override to handle selection changes.
    /// </summary>
    /// <param name="Id">The native control ID of the combo box.</param>
    /// <param name="Item">The index of the newly selected item.</param>
    public virtual void OnComboboxSelectionChanged(int Id, int Item) { }

    /// <summary>
    /// Called by SolidWorks when the selection in a list box changes.
    /// </summary>
    /// <param name="Id">The native control ID of the list box.</param>
    /// <param name="Item">The index of the newly selected item.</param>
    public void OnListboxSelectionChanged(int Id, int Item) { }

    /// <summary>
    /// Called by SolidWorks when a selection box gains or loses focus.
    /// </summary>
    /// <param name="Id">The native control ID of the selection box.</param>
    public void OnSelectionboxFocusChanged(int Id) { }

    /// <summary>
    /// Called by SolidWorks when the selection list in a selection box
    /// changes. Override to react to selection count changes.
    /// </summary>
    /// <param name="Id">The native control ID of the selection box.</param>
    /// <param name="Count">The number of items in the selection list.</param>
    public virtual void OnSelectionboxListChanged(int Id, int Count) { }

    /// <summary>
    /// Called by SolidWorks when a selection box callout is created.
    /// </summary>
    /// <param name="Id">The native control ID of the selection box.</param>
    public void OnSelectionboxCalloutCreated(int Id) { }

    /// <summary>
    /// Called by SolidWorks when a selection box callout is destroyed.
    /// </summary>
    /// <param name="Id">The native control ID of the selection box.</param>
    public void OnSelectionboxCalloutDestroyed(int Id) { }

    /// <summary>
    /// Called by SolidWorks when the user submits a selection. Override to
    /// validate or transform the selection.
    /// </summary>
    /// <param name="Id">The native control ID of the selection box.</param>
    /// <param name="Selection">The selected object.</param>
    /// <param name="SelType">The type of the selection.</param>
    /// <param name="ItemText">
    /// The display text for the selection item; modify to change the label.
    /// </param>
    /// <returns>
    /// <see langword="true"/> to accept the selection;
    /// <see langword="false"/> to reject it.
    /// </returns>
    public virtual bool OnSubmitSelection(
        int Id,
        object Selection,
        int SelType,
        ref string ItemText
    )
    {
        return true;
    }

    /// <summary>
    /// Called by SolidWorks when an ActiveX control is created.
    /// </summary>
    /// <param name="Id">The native control ID.</param>
    /// <param name="Status">Creation status from SolidWorks.</param>
    /// <returns>Always returns 1 (S_OK).</returns>
    public int OnActiveXControlCreated(int Id, bool Status) => 1;

    /// <summary>
    /// Called by SolidWorks when a slider position changes.
    /// </summary>
    /// <param name="Id">The native control ID of the slider.</param>
    /// <param name="Value">The current slider value.</param>
    public void OnSliderPositionChanged(int Id, double Value) { }

    /// <summary>
    /// Called by SolidWorks when slider tracking is completed.
    /// </summary>
    /// <param name="Id">The native control ID of the slider.</param>
    /// <param name="Value">The final slider value.</param>
    public void OnSliderTrackingCompleted(int Id, double Value) { }

    /// <summary>
    /// Called by SolidWorks when a keystroke occurs in the
    /// PropertyManagerPage. Override to handle or intercept keyboard input.
    /// </summary>
    /// <param name="Wparam">The wParam of the keystroke message.</param>
    /// <param name="Message">The message identifier.</param>
    /// <param name="Lparam">The lParam of the keystroke message.</param>
    /// <param name="Id">The native control ID that received the keystroke.</param>
    /// <returns>
    /// <see langword="true"/> to allow the keystroke;
    /// <see langword="false"/> to suppress it.
    /// </returns>
    public bool OnKeystroke(int Wparam, int Message, int Lparam, int Id) => true;

    /// <summary>
    /// Called by SolidWorks when a popup menu item is selected.
    /// </summary>
    /// <param name="Id">The native control ID associated with the popup.</param>
    public void OnPopupMenuItem(int Id) { }

    /// <summary>
    /// Called by SolidWorks when a popup menu item needs to update its state.
    /// </summary>
    /// <param name="Id">The native control ID associated with the popup.</param>
    /// <param name="retval">Reference to the return value to set.</param>
    public void OnPopupMenuItemUpdate(int Id, ref int retval) { }

    /// <summary>
    /// Called by SolidWorks when a control gains focus. Override to
    /// perform actions on focus gain.
    /// </summary>
    /// <param name="Id">The native control ID that gained focus.</param>
    public virtual void OnGainedFocus(int Id) { }

    /// <summary>
    /// Called by SolidWorks when a control loses focus.
    /// </summary>
    /// <param name="Id">The native control ID that lost focus.</param>
    public void OnLostFocus(int Id) { }

    /// <summary>
    /// Called by SolidWorks when a WindowFromHandle control is created.
    /// Override to return the Win32 window handle for WPF interop.
    /// </summary>
    /// <param name="Id">The native control ID.</param>
    /// <param name="Status">Creation status from SolidWorks.</param>
    /// <returns>
    /// The Win32 window handle (as <see cref="int"/>) to host WPF content,
    /// or -1 on failure.
    /// </returns>
    public virtual int OnWindowFromHandleControlCreated(int Id, bool Status) => -1;

    /// <summary>
    /// Called by SolidWorks when the right mouse button is released over a
    /// list box.
    /// </summary>
    /// <param name="Id">The native control ID of the list box.</param>
    /// <param name="PosX">The X coordinate of the mouse.</param>
    /// <param name="PosY">The Y coordinate of the mouse.</param>
    public void OnListboxRMBUp(int Id, int PosX, int PosY) { }

    /// <summary>
    /// Called by SolidWorks when number box tracking is completed
    /// (slider drag finished).
    /// </summary>
    /// <param name="Id">The native control ID of the number box.</param>
    /// <param name="Value">The final numeric value.</param>
    public virtual void OnNumberBoxTrackingCompleted(int Id, double Value) { }

    #endregion

    #region IDisposable

    /// <summary>
    /// Releases all resources used by the <see cref="SldPMPageBase"/>.
    /// Releases COM references, clears events, and suppresses finalization.
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Releases the unmanaged resources used by the
    /// <see cref="SldPMPageBase"/> and optionally releases the managed
    /// resources.
    /// </summary>
    /// <param name="disposing">
    /// <see langword="true"/> to release both managed and unmanaged resources;
    /// <see langword="false"/> to release only unmanaged resources.
    /// </param>
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

            if (_doc != null)
            {
                Marshal.FinalReleaseComObject(_doc);
                _doc = null;
            }
            App = null;

            AfterActivationEvent = null;
            OkClicked = null;
            HelpRequested = null;
            WhatsNewRequested = null;
            Closing = null;
            Closed = null;

            _disposed = true;
        }
    }

    #endregion
}
