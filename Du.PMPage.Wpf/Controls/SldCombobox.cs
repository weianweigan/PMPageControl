using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;

namespace Du.PMPage.Wpf.Controls;

/// <summary>
/// WPF wrapper for a SolidWorks Property Manager Page combo box control
/// (<see cref="IPropertyManagerPageCombobox"/>). Supports items binding via
/// <see cref="SldItems"/>, current selection tracking, editable text, and
/// style/height configuration.
/// </summary>
public class SldCombobox : SldControl<IPropertyManagerPageCombobox>
{
    static SldCombobox()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(SldCombobox),
            new FrameworkPropertyMetadata(typeof(SldCombobox))
        );
    }

    /// <summary>
    /// Gets or sets the initial items for the combo box as a comma-separated string.
    /// These items are used to populate <see cref="SldItems"/> on first creation
    /// when <see cref="SldItems"/> has not been explicitly set.
    /// </summary>
    public string StartUpItems { get; set; }

    #region Dependency Properties

    /// <summary>
    /// Gets or sets the collection of items displayed in the combo box.
    /// Changes to the collection are automatically synchronized to the native
    /// SolidWorks combo box control.
    /// </summary>
    public ObservableCollection<string> SldItems
    {
        get { return (ObservableCollection<string>)GetValue(SldItemsProperty); }
        set { SetValue(SldItemsProperty, value); }
    }

    /// <summary>
    /// Dependency property for <see cref="SldItems"/>.
    /// </summary>
    public static readonly DependencyProperty SldItemsProperty = DependencyProperty.Register(
        "SldItems",
        typeof(ObservableCollection<string>),
        typeof(SldCombobox),
        new PropertyMetadata(null, OnSldItemsPropertyCallback)
    );

    private static void OnSldItemsPropertyCallback(
        DependencyObject d,
        DependencyPropertyChangedEventArgs e
    )
    {
        var sld = (SldCombobox)d;
        sld.OnSldItemsPropertyChanged(
            (ObservableCollection<string>)e.OldValue,
            (ObservableCollection<string>)e.NewValue
        );
    }

    private void OnSldItemsPropertyChanged(
        ObservableCollection<string> oldValue,
        ObservableCollection<string> newValue
    )
    {
        if (oldValue != null)
        {
            oldValue.CollectionChanged -= SldItems_CollectionChanged;
        }
        if (newValue != null)
        {
            if (SControl != null)
                SControl.AddItems(newValue.ToArray());
            newValue.CollectionChanged += SldItems_CollectionChanged;
        }
    }

    private void SldItems_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        if (SControl == null)
        {
            return;
        }
        if (e.OldItems != null)
        {
            for (int i = e.OldStartingIndex; i < e.OldItems.Count; i++)
            {
                SControl.DeleteItem((short)i);
            }
        }
        if (e.NewItems != null && e.NewItems.Count > 0)
        {
            SControl.AddItems(e.NewItems.Cast<string>().ToArray());
        }
        if (e.Action == NotifyCollectionChangedAction.Reset)
        {
            SControl.Clear();
        }
    }

    /// <summary>
    /// Gets and sets the item that is currently selected for this combo box.
    /// </summary>
    public int CurrentSelection
    {
        get { return (int)GetValue(CurrentSelectionProperty); }
        set { SetValue(CurrentSelectionProperty, value); }
    }

    /// <summary>
    /// Dependency property for <see cref="CurrentSelection"/>.
    /// </summary>
    public static readonly DependencyProperty CurrentSelectionProperty =
        DependencyProperty.Register(
            "CurrentSelection",
            typeof(int),
            typeof(SldCombobox),
            new FrameworkPropertyMetadata(
                -1,
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnCurrentSelectionPropertyCallback
            )
        );

    /// <summary>
    /// Gets or sets the style for the attached drop-down list for this combo box.
    /// </summary>
    public swPropMgrPageComboBoxStyle_e SldStyle
    {
        get { return (swPropMgrPageComboBoxStyle_e)GetValue(SldStyleProperty); }
        set { SetValue(SldStyleProperty, value); }
    }

    /// <summary>
    /// Dependency property for <see cref="SldStyle"/>.
    /// </summary>
    public static readonly DependencyProperty SldStyleProperty = DependencyProperty.Register(
        "SldStyle",
        typeof(swPropMgrPageComboBoxStyle_e),
        typeof(SldCombobox),
        new PropertyMetadata(
            swPropMgrPageComboBoxStyle_e.swPropMgrPageComboBoxStyle_AvoidSelectionText,
            OnSldStylePropertyCallback
        )
    );

    /// <summary>
    /// Gets or sets the text in the combo box.
    /// </summary>
    /// <remarks>
    /// IPropertyManagerPageCombobox::Style must be set to swPropMgrPageComboBoxStyle_EditableText to edit text in a combo box.</remarks>
    public string EditText
    {
        get { return (string)GetValue(EditTextProperty); }
        set { SetValue(EditTextProperty, value); }
    }

    /// <summary>
    /// Dependency property for <see cref="EditText"/>.
    /// </summary>
    public static readonly DependencyProperty EditTextProperty = DependencyProperty.Register(
        "EditText",
        typeof(string),
        typeof(SldCombobox),
        new PropertyMetadata("", OnEditTextPropertyCallback)
    );

    /// <summary>
    /// Gets and sets the maximum height of the attached drop-down combo box.
    /// </summary>
    /// <remarks>
    /// Only use this method to set properties on the PropertyManager page before it is displayed or while it is closed. See IPropertyManagerPage2::Show2 and IPropertyManagerPage2::Close.
    /// </remarks>
    public int SldHeight
    {
        get { return (int)GetValue(SldHeightProperty); }
        set { SetValue(SldHeightProperty, value); }
    }

    /// <summary>
    /// Dependency property for <see cref="SldHeight"/>.
    /// </summary>
    public static readonly DependencyProperty SldHeightProperty = DependencyProperty.Register(
        "SldHeight",
        typeof(int),
        typeof(SldCombobox),
        new PropertyMetadata(30, OnSldHeightPropertyCallback)
    );

    #endregion

    #region Properties Callback

    private static void OnSldHeightPropertyCallback(
        DependencyObject d,
        DependencyPropertyChangedEventArgs e
    )
    {
        var sld = (SldCombobox)d;
        sld.OnSldHeightPropertyChanged((int)e.OldValue, (int)e.NewValue);
    }

    private void OnSldHeightPropertyChanged(int oldValue, int newValue)
    {
        if (SControl != null && newValue != oldValue && SldControlVisibility)
        {
            if (newValue < 0 || newValue > 1000)
                throw new ArgumentException("SldHeight should be 0-1000");
            SControl.Height = (short)newValue;
        }
    }

    private static void OnEditTextPropertyCallback(
        DependencyObject d,
        DependencyPropertyChangedEventArgs e
    )
    {
        var sld = (SldCombobox)d;
        sld.OnEditTextPropertyChanged((string)e.OldValue, (string)e.NewValue);
    }

    private void OnEditTextPropertyChanged(string oldValue, string newValue)
    {
        if (SControl != null && oldValue != newValue)
        {
            if (SldStyle != swPropMgrPageComboBoxStyle_e.swPropMgrPageComboBoxStyle_EditableText)
                throw new PropertyManagerPageComboboxNotEditableTextStyleException(
                    $"The Style of {nameof(PropertyManagerPageCombobox)} is not {nameof(swPropMgrPageComboBoxStyle_e.swPropMgrPageComboBoxStyle_EditableText)},So you cannot Set the EditText Property"
                );
            if (SControl.EditText != newValue)
            {
                SControl.EditText = newValue;
            }
        }
    }

    private static void OnSldStylePropertyCallback(
        DependencyObject d,
        DependencyPropertyChangedEventArgs e
    )
    {
        var sld = (SldCombobox)d;
        sld.OnSldStyleChanged(
            (swPropMgrPageComboBoxStyle_e)e.OldValue,
            (swPropMgrPageComboBoxStyle_e)e.NewValue
        );
    }

    private void OnSldStyleChanged(
        swPropMgrPageComboBoxStyle_e oldValue,
        swPropMgrPageComboBoxStyle_e newValue
    )
    {
        if (SControl != null && newValue != oldValue)
        {
            SControl.Style = (int)newValue;
        }
    }

    private static void OnCurrentSelectionPropertyCallback(
        DependencyObject d,
        DependencyPropertyChangedEventArgs e
    )
    {
        var sld = (SldCombobox)d;
        sld.OnCurrentSelectionPropertyChanged((int)e.OldValue, (int)e.NewValue);
    }

    private void OnCurrentSelectionPropertyChanged(int oldValue, int newValue)
    {
        if (SControl != null && oldValue != newValue)
        {
            SControl.CurrentSelection = (short)newValue;
        }
    }
    #endregion

    #region Methods

    /// <summary>
    /// Synchronizes the managed properties to the native SolidWorks combo box control
    /// after the native control has been created.
    /// </summary>
    protected override void SetSldControl()
    {
        SControl.Height = (short)SldHeight;
        SControl.Style = (int)SldStyle;

        if (SldItems != null && SldItems.Count > 0)
        {
            SControl.AddItems(SldItems.ToArray());
        }
        else if (!string.IsNullOrWhiteSpace(StartUpItems))
        {
            SldItems = new ObservableCollection<string>(StartUpItems.Split(','));
        }
    }

    internal void OnItemSelected(int item)
    {
        CurrentSelection = item;
    }

    internal void OnEditTextUpdate(string text)
    {
        EditText = text;
    }

    #endregion
}
