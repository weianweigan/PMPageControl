using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using SolidWorks.Interop.swconst;

namespace Du.PMPage.Wpf;

/// <summary>
/// A WPF <see cref="ListBox"/> that integrates with SolidWorks selection filtering.
/// Raises an <see cref="Activated"/> event when clicked (background area), and supports
/// filtering by <see cref="SwSelectTypes"/> to restrict which entity types can be selected.
/// Maintains a bindable <see cref="Selections"/> collection similar to
/// <see cref="Du.PMPage.Wpf.Controls.SldSelectionBox"/>.
/// </summary>
/// <remarks>
/// <para>
/// The <see cref="Selections"/> collection is automatically synchronised to
/// <see cref="ItemsControl.ItemsSource"/> so that the list box renders
/// selection items without extra bindings.
/// </para>
/// <para>
/// Pressing <c>Delete</c> or <c>Backspace</c> removes the currently selected
/// item(s) from <see cref="Selections"/>.  Set <see cref="AllowDelete"/> to
/// <see langword="false"/> to suppress this behaviour.
/// </para>
/// </remarks>
public class WpfSelectionList : ListBox
{
    static WpfSelectionList()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(WpfSelectionList),
            new FrameworkPropertyMetadata(typeof(WpfSelectionList)));
    }

    #region Events

    /// <summary>
    /// Raised when the selection list is activated by a mouse click on its
    /// background (not on an item).  The list starts receiving SolidWorks
    /// selections routed by the parent <see cref="SldWindow"/>.
    /// </summary>
    public event Action<object> Activated;

    /// <summary>
    /// Raised when the selection list is deactivated (another list in the
    /// same window was activated, or <see cref="IsActive"/> was set to
    /// <see langword="false"/>).  Deactivation preserves existing
    /// <see cref="Selections"/> — it merely stops new SolidWorks picks
    /// from being routed to this list.
    /// </summary>
    public event Action<object> Deactivated;

    /// <summary>Obsolete. Use <see cref="Activated"/> instead.</summary>
    [Obsolete("Use 'Activated' instead. 'Actived' will be removed in a future version.")]
    public event Action<object> Actived
    {
        add { Activated += value; }
        remove { Activated -= value; }
    }

    /// <summary>
    /// Raised when the <see cref="Selections"/> collection changes
    /// (items added or removed externally, i.e. not by the internal
    /// <see cref="AddSelection(SwSeleTypeObjectPair)"/> path).
    /// </summary>
    public event Action<WpfSelectionList> SelectionsUpdated;

    #endregion

    #region Dependency Properties

    // ── IsActive ──────────────────────────────────────────────

    /// <summary>
    /// Gets or sets whether the selection list is currently active (has been clicked).
    /// When <see langword="true"/>, SolidWorks selections are routed to this list
    /// by the parent <see cref="SldWindow"/>.  When <see langword="false"/>, the
    /// list retains its existing <see cref="Selections"/> but stops receiving new
    /// ones.
    /// </summary>
    public bool IsActive
    {
        get { return (bool)GetValue(IsActiveProperty); }
        set { SetValue(IsActiveProperty, value); }
    }

    /// <summary>Identifies the <see cref="IsActive"/> dependency property.</summary>
    public static readonly DependencyProperty IsActiveProperty = DependencyProperty.Register(
        "IsActive",
        typeof(bool),
        typeof(WpfSelectionList),
        new PropertyMetadata(false, OnIsActivePropertyChanged));

    private static void OnIsActivePropertyChanged(
        DependencyObject d,
        DependencyPropertyChangedEventArgs e)
    {
        var list = (WpfSelectionList)d;
        list.UpdateVisualState();

        if ((bool)e.NewValue)
            list.Activated?.Invoke(list);
        else
            list.Deactivated?.Invoke(list);
    }

    // ── Mark ─────────────────────────────────────────────────

    /// <summary>
    /// Selection mark identifier. Must be a power of two
    /// (1, 2, 4, 8, …) to match SolidWorks selection-mark conventions.
    /// Default is 1.
    /// </summary>
    public int Mark
    {
        get { return (int)GetValue(MarkProperty); }
        set { SetValue(MarkProperty, value); }
    }

    /// <summary>Identifies the <see cref="Mark"/> dependency property.</summary>
    public static readonly DependencyProperty MarkProperty = DependencyProperty.Register(
        "Mark",
        typeof(int),
        typeof(WpfSelectionList),
        new PropertyMetadata(1, OnMarkChanged),
        ValidateMark);

    private static bool ValidateMark(object value)
    {
        int v = (int)value;
        return v > 0 && (v & (v - 1)) == 0; // power of two
    }

    private static void OnMarkChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        // No runtime action needed; Mark is used externally by SldWindow / consumer code.
    }

    // ── SwSelectTypes ────────────────────────────────────────

    /// <summary>
    /// Gets or sets the allowed SolidWorks selection types for filtering.
    /// When set, only entities matching these types can be selected.
    /// </summary>
    public List<swSelectType_e> SwSelectTypes
    {
        get { return (List<swSelectType_e>)GetValue(SwSelectTypesProperty); }
        set { SetValue(SwSelectTypesProperty, value); }
    }

    /// <summary>Identifies the <see cref="SwSelectTypes"/> dependency property.</summary>
    public static readonly DependencyProperty SwSelectTypesProperty = DependencyProperty.Register(
        "SwSelectTypes",
        typeof(List<swSelectType_e>),
        typeof(WpfSelectionList),
        new PropertyMetadata(null));

    // ── SingleEntityOnly ─────────────────────────────────────

    /// <summary>
    /// Gets or sets whether this selection list accepts only a single entity (true)
    /// or multiple entities (false). Default is false.
    /// When true, a new selection replaces the previous one.
    /// </summary>
    public bool SingleEntityOnly
    {
        get { return (bool)GetValue(SingleEntityOnlyProperty); }
        set { SetValue(SingleEntityOnlyProperty, value); }
    }

    /// <summary>Identifies the <see cref="SingleEntityOnly"/> dependency property.</summary>
    public static readonly DependencyProperty SingleEntityOnlyProperty = DependencyProperty.Register(
        "SingleEntityOnly",
        typeof(bool),
        typeof(WpfSelectionList),
        new PropertyMetadata(false));

    // ── Selections ───────────────────────────────────────────

    /// <summary>
    /// The collection of selected objects in this selection list.
    /// Bind to this property to observe selection changes.
    /// This collection is automatically assigned to <see cref="ItemsControl.ItemsSource"/>
    /// so the list box renders its contents.
    /// </summary>
    public ObservableCollection<SwSeleTypeObjectPair> Selections
    {
        get { return (ObservableCollection<SwSeleTypeObjectPair>)GetValue(SelectionsProperty); }
        set { SetValue(SelectionsProperty, value); }
    }

    /// <summary>
    /// Identifies the <see cref="Selections"/> dependency property.
    /// Binds two-way by default for MVVM scenarios.
    /// </summary>
    public static readonly DependencyProperty SelectionsProperty = DependencyProperty.Register(
        "Selections",
        typeof(ObservableCollection<SwSeleTypeObjectPair>),
        typeof(WpfSelectionList),
        new FrameworkPropertyMetadata(
            null,
            FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
            OnSelectionsChanged));

    private static void OnSelectionsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not WpfSelectionList list)
            return;

        list.OnSelectionsCollectionChanged(
            (ObservableCollection<SwSeleTypeObjectPair>)e.OldValue,
            (ObservableCollection<SwSeleTypeObjectPair>)e.NewValue);
    }

    private void OnSelectionsCollectionChanged(
        ObservableCollection<SwSeleTypeObjectPair> oldValue,
        ObservableCollection<SwSeleTypeObjectPair> newValue)
    {
        if (oldValue != null)
        {
            oldValue.CollectionChanged -= Selections_CollectionChanged;
        }
        if (newValue != null)
        {
            newValue.CollectionChanged += Selections_CollectionChanged;
        }

        // Auto-sync ItemsSource so the ListBox displays selection items.
        ItemsSource = newValue;
    }

    // ── AllowDelete ──────────────────────────────────────────

    /// <summary>
    /// Gets or sets whether the user can remove items from the list by
    /// pressing <c>Delete</c> or <c>Backspace</c>. Default is <see langword="true"/>.
    /// </summary>
    public bool AllowDelete
    {
        get { return (bool)GetValue(AllowDeleteProperty); }
        set { SetValue(AllowDeleteProperty, value); }
    }

    /// <summary>Identifies the <see cref="AllowDelete"/> dependency property.</summary>
    public static readonly DependencyProperty AllowDeleteProperty = DependencyProperty.Register(
        "AllowDelete",
        typeof(bool),
        typeof(WpfSelectionList),
        new PropertyMetadata(true));

    // ── Caption ──────────────────────────────────────────────

    /// <summary>
    /// Gets or sets an optional caption displayed above or inside the list.
    /// </summary>
    public string Caption
    {
        get { return (string)GetValue(CaptionProperty); }
        set { SetValue(CaptionProperty, value); }
    }

    /// <summary>Identifies the <see cref="Caption"/> dependency property.</summary>
    public static readonly DependencyProperty CaptionProperty = DependencyProperty.Register(
        "Caption",
        typeof(string),
        typeof(WpfSelectionList),
        new PropertyMetadata(null));

    /// <summary>
    /// Gets or sets the height of the list area in device-independent pixels.
    /// Default is <c>NaN</c> (automatic).
    /// </summary>
    public double ListHeight
    {
        get { return (double)GetValue(ListHeightProperty); }
        set { SetValue(ListHeightProperty, value); }
    }

    /// <summary>Identifies the <see cref="ListHeight"/> dependency property.</summary>
    public static readonly DependencyProperty ListHeightProperty = DependencyProperty.Register(
        "ListHeight",
        typeof(double),
        typeof(WpfSelectionList),
        new PropertyMetadata(double.NaN));

    #endregion

    #region Internal State

    private bool _isInternalUpdate;

    #endregion

    #region Public / Internal API

    /// <summary>
    /// Adds a selection to this list from the SolidWorks event handler.
    /// Respects <see cref="SingleEntityOnly"/> by clearing existing selections first.
    /// </summary>
    /// <param name="item">The selection to add.</param>
    internal void AddSelection(SwSeleTypeObjectPair item)
    {
        if (Selections == null)
            return;

        _isInternalUpdate = true;
        try
        {
            if (SingleEntityOnly)
                Selections.Clear();

            Selections.Add(item);
        }
        finally
        {
            _isInternalUpdate = false;
        }
    }

    /// <summary>
    /// Removes all selections from this list.
    /// </summary>
    internal void ClearSelections()
    {
        if (Selections == null)
            return;

        _isInternalUpdate = true;
        try
        {
            Selections.Clear();
        }
        finally
        {
            _isInternalUpdate = false;
        }
    }

    #endregion

    #region Overrides

    /// <inheritdoc />
    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
        UpdateVisualState();
        UpdateEmptyState();
    }

    /// <summary>
    /// Raises the <see cref="Activated"/> event and marks the list as active
    /// when the user clicks on the list background (not on an item).
    /// </summary>
    protected override void OnMouseUp(MouseButtonEventArgs e)
    {
        // Only activate on background click, not on item clicks.
        // Item clicks are for selection within the list.
        var hit = VisualTreeHelper.HitTest(this, e.GetPosition(this));
        if (hit == null)
        {
            // Hit test failed — activate anyway as fallback.
            Activate();
        }
        else
        {
            // Walk the visual tree to determine if the click landed on
            // an item container or on the background.
            var element = hit.VisualHit as DependencyObject;
            bool isOnItem = false;
            while (element != null && element != this)
            {
                if (element is ListBoxItem)
                {
                    isOnItem = true;
                    break;
                }
                element = VisualTreeHelper.GetParent(element);
            }

            if (!isOnItem)
                Activate();
        }

        base.OnMouseUp(e);
    }

    /// <summary>
    /// Handles key-down events to allow deleting items with Delete / Backspace.
    /// </summary>
    protected override void OnKeyDown(KeyEventArgs e)
    {
        if (AllowDelete && (e.Key == Key.Delete || e.Key == Key.Back))
        {
            if (Selections != null && SelectedItems != null)
            {
                // Collect items to remove (iterate over a copy because we mutate).
                var toRemove = new List<SwSeleTypeObjectPair>();
                foreach (var item in SelectedItems)
                {
                    if (item is SwSeleTypeObjectPair pair)
                        toRemove.Add(pair);
                }

                _isInternalUpdate = true;
                try
                {
                    foreach (var pair in toRemove)
                        Selections.Remove(pair);
                }
                finally
                {
                    _isInternalUpdate = false;
                }
                SelectionsUpdated?.Invoke(this);
                e.Handled = true;
                return;
            }
        }

        base.OnKeyDown(e);
    }

    /// <summary>
    /// Called when the <see cref="Selections"/> collection changes.
    /// Fires <see cref="SelectionsUpdated"/> for external (non-internal) changes.
    /// Updates the empty-state placeholder visibility.
    /// </summary>
    protected virtual void OnSelectionsChanged()
    {
        UpdateEmptyState();
        if (!_isInternalUpdate)
            SelectionsUpdated?.Invoke(this);
    }

    #endregion

    #region Helpers

    private void Activate()
    {
        // Setting IsActive = true triggers OnIsActivePropertyChanged,
        // which fires Activated.  Do NOT fire it again here.
        IsActive = true;
    }

    private void UpdateVisualState()
    {
        // Visual state is driven by XAML triggers on IsActive.
        // This method is a hook for future extension.
    }

    /// <summary>
    /// Code-based fallback for toggling the empty-state placeholder.
    /// The XAML <c>Trigger</c> on <see cref="ItemsControl.HasItems"/> handles
    /// this declaratively; this method runs on every collection change as a
    /// safety net for environments where the trigger may not re-evaluate.
    /// </summary>
    private void UpdateEmptyState()
    {
        var emptyText = GetTemplateChild("PART_EmptyText") as UIElement;
        if (emptyText != null)
        {
            bool hasItems = HasItems;
            emptyText.Visibility = hasItems ? Visibility.Collapsed : Visibility.Visible;
        }
    }

    private void Selections_CollectionChanged(
        object sender,
        System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        OnSelectionsChanged();
    }

    #endregion
}
