using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using Du.PMPage.Wpf;
using SolidWorks.Interop.sldworks;

namespace PMPageAddin;

/// <summary>
/// Demonstrates all basic SolidWorks PropertyManagerPage controls:
/// Label, LabelMsg, TextBox, NumberBox, CheckBox, Option, and Combobox.
/// Shows data binding, control interactions, and visible/hidden toggling.
/// </summary>
public partial class BasicControlsPage : SldPMPage
{
    public BasicControlsPage() : base()
    {
        InitializeComponent();
    }

    public BasicControlsPage(ISldWorks sw) : base()
    {
        App = sw;
        InitializeComponent();
        DataContext = new BasicControlsViewModel();
    }
}

public partial class BasicControlsViewModel : ObservableObject
{
    // --- TextBox bindings ---
    [ObservableProperty]
    private string _partName = "MyPart";

    [ObservableProperty]
    private string _description = "A sample part created with PMPage";

    // --- NumberBox bindings ---
    [ObservableProperty]
    private double _length = 100.0;

    [ObservableProperty]
    private double _width = 50.0;

    [ObservableProperty]
    private double _height = 25.0;

    // --- CheckBox bindings ---
    [ObservableProperty]
    private bool _enableFillet = true;

    [ObservableProperty]
    private bool _enableChamfer;

    [ObservableProperty]
    private double _filletRadius = 5.0;

    // --- Option bindings (simulate radio group via property change) ---
    [ObservableProperty]
    private bool _isSteel = true;

    [ObservableProperty]
    private bool _isAluminum;

    [ObservableProperty]
    private bool _isTitanium;

    // --- Combobox bindings ---
    [ObservableProperty]
    private int _selectedColorIndex;

    [ObservableProperty]
    private int _selectedSizeIndex = 1;

    [ObservableProperty]
    private string _customSize = string.Empty;

    // --- React to option changes: ensure mutual exclusivity ---
    partial void OnIsSteelChanged(bool oldValue, bool newValue)
    {
        if (newValue)
        {
            IsAluminum = false;
            IsTitanium = false;
        }
    }

    partial void OnIsAluminumChanged(bool oldValue, bool newValue)
    {
        if (newValue)
        {
            IsSteel = false;
            IsTitanium = false;
        }
    }

    partial void OnIsTitaniumChanged(bool oldValue, bool newValue)
    {
        if (newValue)
        {
            IsSteel = false;
            IsAluminum = false;
        }
    }

    // --- React to checkbox: auto-disable fillet radius ---
    partial void OnEnableFilletChanged(bool oldValue, bool newValue)
    {
        if (!newValue)
            FilletRadius = 0;
    }
}
