using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Du.PMPage.Wpf.Controls;
using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;

namespace Du.PMPage.Wpf;

public delegate void SelectionBoxItemsChanged(SldSelectionBox selectionBox);

/// <summary>
/// A WPF ContentControl that creates a SolidWorks PropertyManagerPage populated with
/// native SolidWorks controls. <see cref="SldControl"/> children declared in XAML
/// are auto-discovered from the visual tree and converted to native SW controls via
/// <see cref="SldControl.AddToPage(SldPMPageBase, int)"/> / <see cref="SldControl.AddToGroup"/>.
///
/// Usage — set <see cref="SldPMPageBase.App"/> in code-behind before <c>InitializeComponent()</c>,
/// declare <see cref="SldControl"/> elements in XAML, then call <see cref="SldPMPageBase.ShowPage()"/>.
/// </summary>
[ComVisible(true)]
public class SldPMPage : SldPMPageBase
{
    /// <summary>
    /// Event raised when the items in a <see cref="SldSelectionBox"/> have changed, either via user interaction or programmatic updates.
    /// </summary>
    public event SelectionBoxItemsChanged OnSelectionBoxItemsChanged;

    #region Fields

    /// <summary>
    /// Next available control ID. Starts at 2 because ID 1 is reserved
    /// for internal page use by the SolidWorks PropertyManagerPage API.
    /// </summary>
    private int _idIndex = 2;
    private List<SldControl> _discoveredControls;
    private readonly List<SldContentHost> _contentHosts = new List<SldContentHost>();
    private bool _hasRetry;
    private bool _controlsAdded;
    private bool _designTimePreviewBuilt;

    #endregion

    #region Constructors

    static SldPMPage()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(SldPMPage),
            new FrameworkPropertyMetadata(typeof(SldPMPage))
        );
    }

    /// <summary>
    /// Parameterless constructor for XAML support.
    /// Set <see cref="SldPMPageBase.App"/> in code-behind before <c>InitializeComponent()</c>.
    /// </summary>
    public SldPMPage()
    {
        if (DesignerProperties.GetIsInDesignMode(this))
        {
            if (Template == null)
                Template = CreateDesignTimeTemplate();

            Loaded += OnDesignModeLoaded;
            return;
        }

        // When PageCreatedWhenShow is false, create the native SW page early if App is
        // already set (e.g. via XAML property). Control discovery is always deferred to
        // BeforeShow() so the visual tree is fully constructed.
        if (!PageCreatedWhenShow && App != null)
        {
            CreatePage();
        }
    }

    /// <summary>
    /// Convenience constructor.
    /// </summary>
    public SldPMPage(ISldWorks app)
        : this()
    {
        App = app;
    }

    #endregion

    #region Design-Time

    private static ControlTemplate CreateDesignTimeTemplate()
    {
        var template = new ControlTemplate(typeof(SldPMPage));
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

    /// <summary>
    /// In design mode, auto-generates a preview from discovered SldControls
    /// so the WPF designer renders a meaningful representation.
    /// </summary>
    private void OnDesignModeLoaded(object sender, RoutedEventArgs e)
    {
        if (_designTimePreviewBuilt)
            return;
        _designTimePreviewBuilt = true;

        // Ensure container controls render their children at design time
        foreach (var ctrl in EnumerateSldControls(this))
        {
            if (ctrl is SldGroupBox groupBox)
                groupBox.EnsureDesignTimeContent();
            else if (ctrl is SldTabControl tabControl)
                tabControl.EnsureDesignTimeContent();
        }
    }

    #endregion

    #region Public Properties

    /// <summary>
    /// Gets or sets the SldControl that currently has focus.
    /// </summary>
    public SldControl FocusSldControl
    {
        get { return (SldControl)GetValue(FocusSldControlProperty); }
        set { SetValue(FocusSldControlProperty, value); }
    }

    /// <summary>Identifies the <see cref="FocusSldControl"/> dependency property.</summary>
    public static readonly DependencyProperty FocusSldControlProperty = DependencyProperty.Register(
        nameof(FocusSldControl),
        typeof(SldControl),
        typeof(SldPMPage),
        new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault)
    );

    /// <summary>
    /// Options for the PMPage. When set, these override the default options
    /// (OkayButton | CancelButton).
    /// </summary>
    public swPropertyManagerPageOptions_e[] PageOptions
    {
        get { return (swPropertyManagerPageOptions_e[])GetValue(PageOptionsProperty); }
        set { SetValue(PageOptionsProperty, value); }
    }

    /// <summary>Identifies the <see cref="PageOptions"/> dependency property.</summary>
    public static readonly DependencyProperty PageOptionsProperty = DependencyProperty.Register(
        nameof(PageOptions),
        typeof(swPropertyManagerPageOptions_e[]),
        typeof(SldPMPage),
        new PropertyMetadata(default(swPropertyManagerPageOptions_e[]))
    );

    #endregion

    #region Control ID Management

    /// <inheritdoc/>
    internal override int GetNextControlId() => _idIndex++;

    #endregion

    #region Control Discovery & Registration

    /// <summary>
    /// Walks the visual tree in document order, interleaving <see cref="SldControl"/> and
    /// <see cref="SldContentHost"/> instances so the native SW control order matches the
    /// XAML declaration order. Idempotent — subsequent calls are no-ops.
    /// </summary>
    private void DiscoverAndAddControls()
    {
        if (_controlsAdded)
            return;

        _contentHosts.Clear();
        _discoveredControls = [.. EnumerateSldControls(this)];

        foreach (var c in _discoveredControls)
        {
            if (c is SldGroupBox gb)
            {
                _idIndex = gb.AddToPage(this, _idIndex);

                foreach (var child in gb.Children)
                {
                    if (child is SldContentHost sldContentHost)
                    {
                        _contentHosts.Add(sldContentHost);
                    }
                }
            }
            else if (c is SldTabControl tab)
            {
                _idIndex = tab.AddToPage(this, _idIndex);

                foreach (var child in tab.Children)
                {
                    if (child is SldGroupBox groupBox)
                    {
                        foreach (var childChild in groupBox.Children)
                        {
                            if (childChild is SldContentHost sldContentHost)
                            {
                                _contentHosts.Add(sldContentHost);
                            }
                        }
                    }
                }
            }
            else
            {
                _idIndex = c.AddToPage(this, _idIndex);

                if (c is SldContentHost sldContentHost)
                {
                    _contentHosts.Add(sldContentHost);
                }
            }
        }
        WireExternalSelectionHandlers();
        _controlsAdded = true;
    }

    /// <summary>
    /// Wires <see cref="SldSelectionBox.ExternalSelectionHandler"/> for every discovered
    /// selection box so that external modifications to the <see cref="SldSelectionBox.Selections"/>
    /// collection are reflected in the SolidWorks selection manager.
    /// </summary>
    private void WireExternalSelectionHandlers()
    {
        foreach (var box in _discoveredControls.OfType<SldSelectionBox>())
        {
            box.ExternalSelectionHandler = HandleExternalSelection;
        }
    }

    /// <summary>
    /// Translates an external <see cref="SldSelectionBox.Selections"/> collection change
    /// into a SolidWorks <c>ISelectionManager</c> / <see cref="IEntity"/> operation.
    /// </summary>
    /// <param name="item">The selection pair to act on, or null for Reset.</param>
    /// <param name="select">true to select; false to deselect; null to clear all.</param>
    private void HandleExternalSelection(SwSeleTypeObjectPair item, bool? select)
    {
        var selMgr = ActiveDoc?.ISelectionManager;
        if (selMgr == null)
            return;

        if (select == null)
        {
            // Reset — deselect all items under every known selection box's mark
            foreach (var box in _discoveredControls.OfType<SldSelectionBox>())
            {
                int count = selMgr.GetSelectedObjectCount2(box.Mark);
                for (int i = count; i >= 1; i--)
                {
                    selMgr.DeSelect2(i, box.Mark);
                }
            }
            return;
        }

        if (item?.SelectedObject == null)
            return;

        if (select.Value)
        {
            // Select via IEntity.Select4 with the selection box's mark
            if (item.SelectedObject is IEntity entity)
            {
                var selectData = selMgr.CreateSelectData();
                selectData.Mark = item.Mark;
                entity.Select4(true, selectData);
            }
        }
        else
        {
            // Find the item by identity and deselect it
            int count = selMgr.GetSelectedObjectCount2(item.Mark);
            for (int i = 1; i <= count; i++)
            {
                var candidate = selMgr.GetSelectedObject6(i, item.Mark);
                if (candidate != null && App.IsSame(candidate, item.SelectedObject) == (int)swObjectEquality.swObjectSame)
                {
                    selMgr.DeSelect2(i, item.Mark);
                    break;
                }
            }
        }
    }

    private static IEnumerable<SldControl> EnumerateSldControls(DependencyObject root)
    {
        if (root is SldPMPageBase page)
        {
            return page.Items.OfType<SldControl>();
        }

        return [];
    }

    #endregion

    #region Control Lookup

    private TControl GetControlById<TControl>(int id)
        where TControl : SldControl
    {
        if (_discoveredControls == null)
            return null;

        foreach (var control in _discoveredControls)
        {
            if (control.ID == id)
                return control as TControl;

            if (control is SldGroupBox groupBox)
            {
                var found = groupBox.Children.OfType<TControl>().FirstOrDefault(p => p.ID == id);
                if (found != null)
                    return found;
            }

            if (control is SldTabControl tab)
            {
                var found = tab.Children.OfType<TControl>().FirstOrDefault(p => p.ID == id);
                if (found != null)
                    return found;
            }
        }
        return null;
    }

    private SldControl GetControlById(int id)
    {
        if (_discoveredControls == null)
            return null;

        foreach (var control in _discoveredControls)
        {
            if (control.ID == id)
                return control;

            if (control is SldGroupBox groupBox)
            {
                var found = groupBox.Children.FirstOrDefault(p => p.ID == id);
                if (found != null)
                    return found;
            }

            if (control is SldTabControl tab)
            {
                var found = tab.Children.FirstOrDefault(p => p.ID == id);
                if (found != null)
                    return found;
            }
        }
        return null;
    }

    private SldSelectionBox GetSelectionBoxById(int id) => GetControlById<SldSelectionBox>(id);

    private IEnumerable<TControl> GetSpecControl<TControl>()
        where TControl : SldControl
    {
        if (_discoveredControls == null)
            yield break;

        foreach (var control in _discoveredControls)
        {
            if (control is SldGroupBox groupBox)
            {
                foreach (var item in groupBox.Children)
                {
                    if (item is TControl specControl)
                        yield return specControl;
                }
            }
            else if (control is SldTabControl tab)
            {
                foreach (var item in tab.Children)
                {
                    if (item is TControl specControl)
                        yield return specControl;
                }
            }
            else if (control is TControl specControl)
            {
                yield return specControl;
            }
        }
    }

    #endregion

    #region Override Hooks

    /// <summary>
    /// Auto-discovers SldControls from the visual tree and adds them as native
    /// SW controls via <c>page.AddControl2</c> / <c>group.AddControl2</c>.
    /// Called after <see cref="SldPMPageBase.CreatePage"/> and before showing.
    /// </summary>
    protected override void BeforeShow()
    {
        DiscoverAndAddControls();
    }

    /// <summary>
    /// Binds each <see cref="SldContentHost"/>'s window handle to the native SW
    /// WindowFromHandle control. Called just before <c>Page.Show()</c>.
    /// </summary>
    protected override void OnShowPagePreview()
    {
        foreach (var host in _contentHosts)
            host.BindWindowHandle();
    }

    /// <summary>
    /// Called after the native SW page has been shown.
    /// Sets all discovered SldControls to visible and processes pre-selections.
    /// </summary>
    protected override void OnShowPagePost()
    {
        _discoveredControls?.ForEach(p => p.SldControlVisibility = true);
        ProcessPreSelect();
    }

    /// <summary>
    /// Called before the native SW page is created. Processes any pre-existing
    /// selection set on the active document and assigns marks for discovered
    /// <see cref="SldSelectionBox"/> instances, deferring full pre-selection
    /// processing to <see cref="ProcessPreSelect"/>.
    /// </summary>
    protected override void OnCreatePagePreview()
    {
        var doc = ActiveDoc;
        if (doc == null)
            return;

        var selMgr = doc.ISelectionManager;
        var count = selMgr.GetSelectedObjectCount2(-1);
        if (count == 0)
            return;

        var seleBoxes = GetSpecControl<SldSelectionBox>().ToArray();
        if (!seleBoxes.Any())
            return;

        int i = 1;
        for (int j = 0; j < seleBoxes.Length; j++)
        {
            if (seleBoxes[j].SingleEntityOnly)
            {
                selMgr.SetSelectedObjectMark(
                    i++,
                    seleBoxes[j].Mark,
                    (int)swSelectionMarkAction_e.swSelectionMarkSet
                );
            }
            else
            {
                for (int k = 1; k < count + 1; k++)
                    selMgr.SetSelectedObjectMark(
                        k,
                        seleBoxes[j].Mark,
                        (int)swSelectionMarkAction_e.swSelectionMarkSet
                    );
                return;
            }

            if (i > count + 1)
                break;
        }

        for (int l = i; l < count + 1; l++)
            selMgr.DeSelect(l);
    }

    /// <summary>
    /// Combines <see cref="PageOptions"/> into a bitmask for the native
    /// SolidWorks PropertyManagerPage creation call.
    /// If <see cref="PageOptions"/> is null or empty, defaults to
    /// <c>OkayButton | CancelButton</c>.
    /// </summary>
    protected override int GetOptions()
    {
        var options = PageOptions;
        return CombineOptions(options);
    }

    private static int CombineOptions(swPropertyManagerPageOptions_e[] options)
    {
        if (options == null || options.Length == 0)
            return (int)swPropertyManagerPageOptions_e.swPropertyManagerOptions_OkayButton
                | (int)swPropertyManagerPageOptions_e.swPropertyManagerOptions_CancelButton;

        return options.Aggregate(0, (acc, v) => acc | (int)v);
    }

    #endregion

    #region Pre-Selection

    private void ProcessPreSelect()
    {
        var selMgr = ActiveDoc?.ISelectionManager;
        if (selMgr == null)
            return;

        var seleBoxes = GetSpecControl<SldSelectionBox>().ToArray();
        if (!seleBoxes.Any())
            return;

        for (int j = 0; j < seleBoxes.Length; j++)
        {
            var count = selMgr.GetSelectedObjectCount2(seleBoxes[j].Mark);
            if (count > 0)
            {
                List<SwSeleTypeObjectPair> selections = new(count);
                for (int k = 1; k < count + 1; k++)
                {
                    var selection = selMgr.GetSelectedObject6(k, seleBoxes[j].Mark);
                    var type = selMgr.GetSelectedObjectType3(k, seleBoxes[j].Mark);

                    if (!seleBoxes[j].SwSelectTypes.Any(p => (int)p == type))
                    {
                        selMgr.DeSelect2(k, seleBoxes[j].Mark);
                        continue;
                    }

                    var selePoint = (double[])selMgr.GetSelectionPoint2(k, seleBoxes[j].Mark);
                    string itemText = seleBoxes[j].SControl.ItemText[(short)k];

                    bool res = seleBoxes[j]
                        .OnSubmitSelectionCallout(seleBoxes[j].ID, selection, type, ref itemText);
                    if (res)
                    {
                        selections.Add(
                            new SwSeleTypeObjectPair(
                                k,
                                (swSelectType_e)type,
                                seleBoxes[j].Mark,
                                selection,
                                itemText,
                                selePoint
                            )
                        );
                    }
                    else
                    {
                        selMgr.DeSelect2(k, seleBoxes[j].Mark);
                    }
                }
                if (selections.Any())
                {
                    seleBoxes[j].PreSelectionCount = selections.Count;
                }
                seleBoxes[j].OnSelectionChanged(count, selections);
            }
        }
    }

    #endregion

    #region IPropertyManagerPage2Handler9 Overrides

    /// <summary>
    /// Handles WindowFromHandle control creation callback with retry logic.
    /// Routes to the matching <see cref="SldContentHost"/> if the ID corresponds to one,
    /// otherwise uses legacy single-host retry logic.
    /// </summary>
    public override int OnWindowFromHandleControlCreated(int Id, bool Status)
    {
        // Route to the correct SldContentHost if the ID matches
        foreach (var host in _contentHosts)
        {
            if (host.ID == Id)
                return host.HandleWindowFromHandleCreatedCallback(Id, Status);
        }

        // Legacy fallback (no matching SldContentHost)
        if (Status)
            return -1;

        if (_hasRetry)
            return (int)
                swHandleWindowFromHandleCreationFailure_e.swHandleWindowFromHandleCreationFailure_Cancel;

        _hasRetry = true;
        return (int)
            swHandleWindowFromHandleCreationFailure_e.swHandleWindowFromHandleCreationFailure_Retry;
    }

    /// <inheritdoc/>
    public override void OnCheckboxCheck(int Id, bool Checked)
    {
        var sldCheckBox = GetControlById<SldCheckBox>(Id);
        sldCheckBox?.OnUserCheckChanged(Checked);
    }

    /// <inheritdoc/>
    public override void OnOptionCheck(int Id)
    {
        var sldOption = GetControlById<SldOption>(Id);
        if (
            sldOption != null
            && sldOption.SControl != null
            && sldOption.Checked != sldOption.SControl.Checked
        )
        {
            sldOption.OnCheckedUpdate();
        }
    }

    /// <inheritdoc/>
    public override void OnButtonPress(int Id)
    {
        var sldBtn =
            GetControlById<SldBitmapButton>(Id) as ISldBtnCommand
            ?? GetControlById<SldButton>(Id) as ISldBtnCommand;
        sldBtn?.Command?.Execute(null);
    }

    /// <inheritdoc/>
    public override void OnTextboxChanged(int Id, string Text)
    {
        var control = GetControlById<SldTextBox>(Id);
        if (control != null)
            control.Text = Text;
    }

    /// <inheritdoc/>
    public override void OnNumberboxChanged(int Id, double Value)
    {
        var sldNumberBox = GetControlById<SldNumberBox>(Id);
        if (sldNumberBox != null)
            sldNumberBox.OnValueUpdate(Value);
    }

    /// <inheritdoc/>
    public override void OnNumberBoxTrackingCompleted(int Id, double Value)
    {
        var sldNumberBox = GetControlById<SldNumberBox>(Id);
        if (sldNumberBox != null)
            sldNumberBox.OnValueUpdate(Value);
    }

    /// <inheritdoc/>
    public override void OnComboboxEditChanged(int Id, string Text)
    {
        var control = GetControlById<SldCombobox>(Id);
        if (control != null)
            control.OnEditTextUpdate(Text);
    }

    /// <inheritdoc/>
    public override void OnComboboxSelectionChanged(int Id, int Item)
    {
        var sControl = GetControlById<SldCombobox>(Id);
        sControl?.OnItemSelected(Item);
    }

    /// <inheritdoc/>
    public override void OnSelectionboxListChanged(int Id, int Count)
    {
        SldSelectionBox sldSelectionBox = GetSelectionBoxById(Id);

        if (sldSelectionBox == null)
            return;

        if (Count == 0)
        {
            sldSelectionBox.OnSelectionChanged(Count, null);
            return;
        }

        var seleMgr = ActiveDoc?.ISelectionManager;
        if (seleMgr == null)
            return;

        List<SwSeleTypeObjectPair> selections = new List<SwSeleTypeObjectPair>(Count);
        for (int i = 1; i < Count + 1; i++)
        {
            var seleType = seleMgr.GetSelectedObjectType3(i, sldSelectionBox.Mark);
            var seleObj = seleMgr.GetSelectedObject6(i, sldSelectionBox.Mark);
            var selePoint = (double[])seleMgr.GetSelectionPoint2(i, sldSelectionBox.Mark);
            var name = sldSelectionBox.SControl.ItemText[(short)(i - 1)];

            if (i <= sldSelectionBox.PreSelectionCount && sldSelectionBox.Selections?.Count > i - 1)
            {
                if (
                    App.IsSame(seleObj, sldSelectionBox.Selections[i - 1].SelectedObject)
                    == (int)swObjectEquality.swObjectSame
                )
                {
                    name = sldSelectionBox.Selections[i - 1].Name;
                }
            }

            selections.Add(
                new SwSeleTypeObjectPair(
                    i,
                    (swSelectType_e)seleType,
                    sldSelectionBox.Mark,
                    seleObj,
                    name,
                    selePoint
                )
            );
        }

        sldSelectionBox.OnSelectionChanged(Count, selections);

        // Single-entity selection box: move focus to next
        if (sldSelectionBox.SingleEntityOnly)
        {
            bool isCurrent = false;
            SldSelectionBox next = default;
            foreach (var sldSeleItem in GetSpecControl<SldSelectionBox>())
            {
                if (isCurrent)
                {
                    next = sldSeleItem;
                    break;
                }
                if (sldSeleItem.ID == sldSelectionBox.ID)
                    isCurrent = true;
            }
            if (next != null && next.SldControlIsVisible() && next.AutoMoveFocusToThis)
            {
                next.SControl.SetSelectionFocus();
            }
        }

        OnSelectionBoxItemsChanged?.Invoke(sldSelectionBox);
    }

    /// <inheritdoc/>
    public override bool OnSubmitSelection(
        int Id,
        object Selection,
        int SelType,
        ref string ItemText
    )
    {
        var sldSeleBox = GetSelectionBoxById(Id);
        return sldSeleBox?.OnSubmitSelectionCallout(Id, Selection, SelType, ref ItemText) ?? true;
    }

    /// <inheritdoc/>
    public override void OnGainedFocus(int Id)
    {
        var control = GetControlById(Id);
        if (control != null)
            FocusSldControl = control;
    }

    #endregion

    #region IDisposable

    /// <inheritdoc/>
    protected override void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                _discoveredControls?.ForEach(p => p.SldControlVisibility = false);
                _discoveredControls = null;

                // Cleanup content hosts
                foreach (var host in _contentHosts)
                    host.CleanupNativeHandle();
                _contentHosts.Clear();
            }

            base.Dispose(disposing);
        }
    }

    #endregion
}
