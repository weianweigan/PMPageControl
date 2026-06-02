using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;

namespace Du.PMPage.Wpf.Controls;

/// <summary>
/// A WPF control that wraps a native SolidWorks <see cref="IPropertyManagerPageSelectionbox"/>,
/// providing selection filtering, marking, and a bindable <see cref="Selections"/> collection.
/// </summary>
public class SldSelectionBox : SldControl<IPropertyManagerPageSelectionbox>
{
    static SldSelectionBox()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(SldSelectionBox),
            new FrameworkPropertyMetadata(typeof(SldSelectionBox))
        );
    }

    /// <summary>
    /// Handler for validating a selection before it is accepted into the selection box.
    /// </summary>
    /// <param name="Id">The control ID of this selection box.</param>
    /// <param name="Selection">The native SolidWorks object being selected.</param>
    /// <param name="SelType">The type of the selected entity.</param>
    /// <param name="ItemText">The display text for the selection. May be modified by the handler.</param>
    /// <returns>true to accept the selection; false to reject it.</returns>
    /// <remarks>
    /// <para><b>Multicast warning:</b> When multiple handlers are attached, only the
    /// return value and <c>ItemText</c> from the <b>last</b> invoked handler
    /// are used. For predictable behavior, attach a single handler.</para>
    /// </remarks>
    public delegate bool SubmitSelectionEventHandler(
        int Id,
        object Selection,
        int SelType,
        ref string ItemText
    );

    /// <summary>
    /// Raised when the user picks a selection, allowing the handler to validate
    /// or modify the selection before it is accepted.
    /// </summary>
    /// <remarks>
    /// <para><b>Multicast warning:</b> When multiple handlers are attached, only the
    /// return value and <c>ItemText</c> from the <b>last</b> invoked handler
    /// are used. For predictable behavior, attach a single handler.</para>
    /// </remarks>
    public event SubmitSelectionEventHandler SubmitSelection;

    #region Dependency Properties

    /// <summary>
    /// Whether the selection box accepts only a single entity (true) or
    /// multiple entities (false). Default is false.
    /// </summary>
    public bool SingleEntityOnly
    {
        get { return (bool)GetValue(SingleEntityOnlyProperty); }
        set { SetValue(SingleEntityOnlyProperty, value); }
    }

    /// <summary>Identifies the <see cref="SingleEntityOnly"/> dependency property.</summary>
    public static readonly DependencyProperty SingleEntityOnlyProperty =
        DependencyProperty.Register(
            "SingleEntityOnly",
            typeof(bool),
            typeof(SldSelectionBox),
            new FrameworkPropertyMetadata(false, OnSingleEntityChanged)
        );

    private static void OnSingleEntityChanged(
        DependencyObject d,
        DependencyPropertyChangedEventArgs e
    )
    {
        var box = d as SldSelectionBox;
        if (box == null)
            return;
        box.OnSingleEntityChanged((bool)e.OldValue, (bool)e.NewValue);
    }

    private void OnSingleEntityChanged(bool oldValue, bool newValue)
    {
        if (oldValue != newValue && SControl != null)
        {
            SControl.SingleEntityOnly = newValue;
        }
    }

    /// <summary>
    /// The types of entities that can be selected in this selection box.
    /// </summary>
    /// <remarks>
    /// <para>This is a list reference property. Replacing the entire list triggers the
    /// change callback; modifying list contents does not. Set the full list before the
    /// control is displayed.</para>
    /// </remarks>
    public List<swSelectType_e> SwSelectTypes
    {
        get { return (List<swSelectType_e>)GetValue(SwSelectTypesProperty); }
        set { SetValue(SwSelectTypesProperty, value); }
    }

    /// <summary>Identifies the <see cref="SwSelectTypes"/> dependency property.</summary>
    public static readonly DependencyProperty SwSelectTypesProperty = DependencyProperty.Register(
        "SwSelectTypes",
        typeof(List<swSelectType_e>),
        typeof(SldSelectionBox),
        new FrameworkPropertyMetadata(null, OnSwSelectTypesChanged)
    );

    private static void OnSwSelectTypesChanged(
        DependencyObject d,
        DependencyPropertyChangedEventArgs e
    )
    {
        if (d is not SldSelectionBox box)
            return;
        box.OnSwSelectTypesChanged(
            (List<swSelectType_e>)e.OldValue,
            (List<swSelectType_e>)e.NewValue
        );
    }

    private void OnSwSelectTypesChanged(
        List<swSelectType_e> oldValue,
        List<swSelectType_e> newValue
    )
    {
        if (oldValue != newValue && SControl != null)
        {
            if (SldControlVisibility)
            {
                string msg =
                    $"{nameof(PropertyManagerPageSelectionbox)}.{nameof(PropertyManagerPageSelectionbox.SetSelectionFilters)} Cannot be Set after Displayed";
                Debug.Print(msg);
                throw new InvalidOperationException(msg);
            }
            SControl.SetSelectionFilters(newValue.ToArray());
        }
    }

    /// <summary>
    /// Selection Mark of SelectedObject in this SelectionBox
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
        typeof(SldSelectionBox),
        new FrameworkPropertyMetadata(1, OnMarkChanged)
    );

    private static void OnMarkChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not SldSelectionBox box)
            return;
        box.OnMarkChanged((int)e.OldValue, (int)e.NewValue);
    }

    private void OnMarkChanged(int oldValue, int newValue)
    {
        if (newValue <= 0 || (newValue & (newValue - 1)) != 0)
        {
            throw new ArgumentException(
                $"SelectionBox.Mark must be a power of two (for example, 1, 2, 4, 8). Current value: {newValue}."
            );
        }
        if (oldValue != newValue && SControl != null)
        {
            SControl.Mark = newValue;
        }
    }

    /// <summary>
    /// Gets or sets whether the same entity can be selected multiple times in this selection box.
    /// </summary>
    public bool AllowMultipleSelectOfSameEntity
    {
        get { return (bool)GetValue(AllowMultipleSelectOfSameEntityProperty); }
        set { SetValue(AllowMultipleSelectOfSameEntityProperty, value); }
    }

    /// <summary>
    /// When true, the selection box allows the same entity to be selected multiple times.
    /// When false, selecting an entity that is already selected in this box replaces the previous selection of that entity.
    /// Default is false.
    /// </summary>
    public static readonly DependencyProperty AllowMultipleSelectOfSameEntityProperty =
        DependencyProperty.Register(
            "AllowMultipleSelectOfSameEntity",
            typeof(bool),
            typeof(SldSelectionBox),
            new FrameworkPropertyMetadata(false, OnAllowMultipleSelectOfSameEntityChanged)
        );

    private static void OnAllowMultipleSelectOfSameEntityChanged(
        DependencyObject d,
        DependencyPropertyChangedEventArgs e
    )
    {
        if (d is not SldSelectionBox box)
            return;
        box.OnAllowMultipleSelectOfSameEntityChanged((bool)e.OldValue, (bool)e.NewValue);
    }

    private void OnAllowMultipleSelectOfSameEntityChanged(bool oldValue, bool newValue)
    {
        if (oldValue != newValue && SControl != null)
        {
            if (SldControlVisibility)
            {
                string msg =
                    $"{nameof(PropertyManagerPageSelectionbox)}.{nameof(PropertyManagerPageSelectionbox.AllowMultipleSelectOfSameEntity)} Cannot be Set after Displayed";
                Debug.Print(msg);
                throw new InvalidOperationException(msg);
            }
            else
            {
                SControl.AllowMultipleSelectOfSameEntity = newValue;
            }
        }
    }

    /// <summary>
    /// Gets or sets whether an entity can be selected in this selection box if the entity is selected elsewhere.
    /// </summary>
    public bool AllowSelectInMultipleBoxes
    {
        get { return (bool)GetValue(AllowSelectInMultipleBoxesProperty); }
        set { SetValue(AllowSelectInMultipleBoxesProperty, value); }
    }

    /// <summary>
    /// When true, the selection box allows an entity to be selected even if it is already selected in another selection box.
    /// </summary>
    public static readonly DependencyProperty AllowSelectInMultipleBoxesProperty =
        DependencyProperty.Register(
            "AllowSelectInMultipleBoxes",
            typeof(bool),
            typeof(SldSelectionBox),
            new FrameworkPropertyMetadata(false, OnAllowSelectInMultipleBoxesChanged)
        );

    private static void OnAllowSelectInMultipleBoxesChanged(
        DependencyObject d,
        DependencyPropertyChangedEventArgs e
    )
    {
        var box = d as SldSelectionBox;
        if (box == null)
            return;
        box.OnAllowSelectInMultipleBoxes((bool)e.OldValue, (bool)e.NewValue);
    }

    private void OnAllowSelectInMultipleBoxes(bool oldValue, bool newValue)
    {
        if (oldValue != newValue && SControl != null)
        {
            if (SldControlVisibility)
            {
                string msg =
                    $"{nameof(PropertyManagerPageSelectionbox)}.{nameof(PropertyManagerPageSelectionbox.AllowSelectInMultipleBoxes)} Cannot be Set after Displayed";
                Debug.Print(msg);
                throw new InvalidOperationException(msg);
            }
            SControl.AllowSelectInMultipleBoxes = newValue;
        }
    }

    /// <summary>
    /// The collection of selected objects in this selection box.
    /// Bind to this property with Mode=TwoWay (or rely on the default) to observe
    /// selection changes driven by SolidWorks.
    /// </summary>
    public ObservableCollection<SwSeleTypeObjectPair> Selections
    {
        get { return (ObservableCollection<SwSeleTypeObjectPair>)GetValue(SelectionsProperty); }
        set { SetValue(SelectionsProperty, value); }
    }

    /// <summary>
    /// This is a reference property. Replacing the entire collection triggers the change callback;
    /// </summary>
    public static readonly DependencyProperty SelectionsProperty = DependencyProperty.Register(
        "Selections",
        typeof(ObservableCollection<SwSeleTypeObjectPair>),
        typeof(SldSelectionBox),
        new FrameworkPropertyMetadata(
            null,
            FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
            OnSelectionsChanged
        )
    );

    private static void OnSelectionsChanged(
        DependencyObject d,
        DependencyPropertyChangedEventArgs e
    )
    {
        if (d is not SldSelectionBox box)
            return;

        box.OnSelectionChanged(
            (ObservableCollection<SwSeleTypeObjectPair>)e.OldValue,
            (ObservableCollection<SwSeleTypeObjectPair>)e.NewValue
        );
    }

    private void OnSelectionChanged(
        ObservableCollection<SwSeleTypeObjectPair> oldValue,
        ObservableCollection<SwSeleTypeObjectPair> newValue
    )
    {
        if (oldValue != null)
        {
            oldValue.CollectionChanged -= Selections_CollectionChanged;
        }
        if (newValue != null)
        {
            newValue.CollectionChanged += Selections_CollectionChanged;
        }
    }

    /// <summary>
    /// Callback invoked when the <see cref="Selections"/> collection is externally
    /// modified. Set by the owning page to translate collection changes into
    /// SolidWorks selection manager operations.
    /// </summary>
    /// <remarks>
    /// The delegate receives:
    /// <list type="bullet">
    /// <item><description>The affected <see cref="SwSeleTypeObjectPair"/>, or <see langword="null"/> when all items should be deselected (Reset action).</description></item>
    /// <item><description><see langword="true"/> to select the item in SolidWorks; <see langword="false"/> to deselect it; <see langword="null"/> to deselect all items for this selection box.</description></item>
    /// </list>
    /// </remarks>
    internal Action<SwSeleTypeObjectPair, bool?> ExternalSelectionHandler { get; set; }

    private void Selections_CollectionChanged(
        object sender,
        System.Collections.Specialized.NotifyCollectionChangedEventArgs e
    )
    {
        //内部增加 — 由 OnSelectionChanged 驱动，跳过
        if (_innerAddSelection)
        {
            return;
        }

        switch (e.Action)
        {
            case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
                if (e.NewItems != null)
                {
                    foreach (SwSeleTypeObjectPair item in e.NewItems)
                        ExternalSelectionHandler?.Invoke(item, true);
                }
                break;

            case System.Collections.Specialized.NotifyCollectionChangedAction.Remove:
                if (e.OldItems != null)
                {
                    foreach (SwSeleTypeObjectPair item in e.OldItems)
                        ExternalSelectionHandler?.Invoke(item, false);
                }
                break;

            case System.Collections.Specialized.NotifyCollectionChangedAction.Replace:
                if (e.OldItems != null)
                {
                    foreach (SwSeleTypeObjectPair item in e.OldItems)
                        ExternalSelectionHandler?.Invoke(item, false);
                }
                if (e.NewItems != null)
                {
                    foreach (SwSeleTypeObjectPair item in e.NewItems)
                        ExternalSelectionHandler?.Invoke(item, true);
                }
                break;

            case System.Collections.Specialized.NotifyCollectionChangedAction.Reset:
                // Entire collection replaced — deselect all items for this box
                ExternalSelectionHandler?.Invoke(null, null);
                break;
        }
    }

    /// <summary>
    /// The height of the selection box in dialog units. Default is 14.
    /// </summary>
    /// <remarks>
    /// The value is cast to <see cref="short"/> when applied to the native SolidWorks
    /// control. Values outside the short range will overflow silently.
    /// </remarks>
    public int SldHeight
    {
        get { return (int)GetValue(SldHeightProperty); }
        set { SetValue(SldHeightProperty, value); }
    }

    /// <summary>
    /// The height of the selection box in dialog units. Default is 14.
    /// </summary>
    public static readonly DependencyProperty SldHeightProperty = DependencyProperty.Register(
        "SldHeight",
        typeof(int),
        typeof(SldSelectionBox),
        new FrameworkPropertyMetadata(14, OnSldHeightChanged)
    );

    private static void OnSldHeightChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var box = d as SldSelectionBox;
        if (box == null)
            return;
        box.OnHeightChanged((int)e.OldValue, (int)e.NewValue);
    }

    private void OnHeightChanged(int oldValue, int newValue)
    {
        if (oldValue != newValue && SControl != null)
        {
            SControl.Height = (short)newValue;
        }
    }

    /// <summary>
    /// The index of the currently selected item in this selection box, or -1 if no selection is currently active.
    /// </summary>
    public int CurrentSelection
    {
        get => SControl?.CurrentSelection ?? -1;
        set
        {
            if (SControl != null)
                SControl.CurrentSelection = value;
        }
    }

    #endregion

    #region Properties

    /// <summary>
    /// Gets or sets a multi-row, editable callout for this selection box.
    /// </summary>
    public Callout Callout
    {
        get => SControl?.Callout;
        set
        {
            if (SControl != null)
                SControl.Callout = value;
        }
    }

    /// <summary>
    /// 预先选择的数量
    /// </summary>
    internal int PreSelectionCount { get; set; } = -1;

    /// <summary>
    /// 当此选择框是单选时起作用，决定是否自动将焦点移动到下一个选择框
    /// </summary>
    public bool AutoMoveFocusToThis { get; set; } = true;

    /// <summary>
    /// 设置选择框颜色
    /// </summary>
    /// <remarks>
    /// <see cref="swUserPreferenceIntegerValue_e.swSystemColorsSelectedItem1"/> = 104 第一种颜色，默认
    /// <see cref="swUserPreferenceIntegerValue_e.swSystemColorsSelectedItem2"/> = 105 第二种颜色
    /// <see cref="swUserPreferenceIntegerValue_e.swSystemColorsSelectedItem3"/> = 106 第三种颜色
    /// </remarks>
    public int? SldSystemColorValue { get; set; }

    #endregion

    #region Methods

    /// <summary>
    /// Applies the current property values to the native SolidWorks selection box control.
    /// </summary>
    protected override void SetSldControl()
    {
        if (SwSelectTypes != null && SwSelectTypes.Any())
        {
            SControl.SetSelectionFilters(SwSelectTypes.ToArray());
        }

        //设置属性 -- 是否选择单个和标记
        SControl.SingleEntityOnly = SingleEntityOnly;
        SControl.Mark = Mark;
        SControl.Height = (short)SldHeight;
        SControl.AllowSelectInMultipleBoxes = AllowSelectInMultipleBoxes;
        SControl.AllowMultipleSelectOfSameEntity = AllowMultipleSelectOfSameEntity;

        //设置颜色
        if (SldSystemColorValue != null)
        {
            SControl.SetSelectionColor(true, SldSystemColorValue.Value);
        }
    }

    private bool _innerAddSelection = false;

    internal void OnSelectionChanged(int count, List<SwSeleTypeObjectPair> swSeleTypeObjectPairs)
    {
        if (Selections == null)
            return;

        _innerAddSelection = true;
        try
        {
            //重置
            if (count == 0)
            {
                Selections.Clear();
            }
            else
            {
                //去除多余的
                for (int i = 0; i < Selections.Count; i++)
                {
                    var pair = swSeleTypeObjectPairs.FirstOrDefault(p =>
                        p.Name == Selections[i].Name
                    );
                    if (pair == null)
                    {
                        Selections.RemoveAt(i);
                        --i;
                    }
                }

                //添加新增加的
                for (int i = 0; i < swSeleTypeObjectPairs.Count; i++)
                {
                    var pair = Selections.FirstOrDefault(p =>
                        p.Name == swSeleTypeObjectPairs[i].Name
                    );
                    if (pair == null)
                    {
                        Selections.Add(swSeleTypeObjectPairs[i]);
                    }
                }
            }
        }
        finally
        {
            _innerAddSelection = false;
        }
    }

    internal bool OnSubmitSelectionCallout(
        int id,
        object selection,
        int selType,
        ref string itemText
    )
    {
        return SubmitSelection?.Invoke(id, selection, selType, ref itemText) ?? true;
    }

    #endregion
}
