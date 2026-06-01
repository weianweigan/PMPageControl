using CommunityToolkit.Mvvm.ComponentModel;
using Du.PMPage.Wpf;
using SolidWorks.Interop.sldworks;

namespace PMPageAddin;

/// <summary>
/// Demonstrates SldTabControl — organizing controls into multiple tabs.
/// Each tab contains SldGroupBox instances with related controls.
/// Shows cross-tab data binding (e.g., Dimensions tab values affect
/// calculated properties).
/// </summary>
public partial class TabControlPage : SldPMPage
{
    public TabControlPage() : base()
    {
        InitializeComponent();
    }

    public TabControlPage(ISldWorks sw) : base()
    {
        App = sw;
        InitializeComponent();
        DataContext = new TabControlViewModel();
    }
}

public partial class TabControlViewModel : ObservableObject
{
    // --- Dimensions Tab ---
    [ObservableProperty]
    private double _length = 100.0;

    [ObservableProperty]
    private double _width = 50.0;

    [ObservableProperty]
    private double _depth = 30.0;

    [ObservableProperty]
    private string _volumeText = "150000.00 mm³";

    [ObservableProperty]
    private string _surfaceAreaText = "19000.00 mm²";

    // --- Features Tab ---
    [ObservableProperty]
    private bool _addCenterHole;

    [ObservableProperty]
    private double _holeDiameter = 10.0;

    [ObservableProperty]
    private double _holeDepth = 20.0;

    [ObservableProperty]
    private int _patternTypeIndex;

    [ObservableProperty]
    private double _patternInstances = 4.0;

    // --- Options Tab ---
    [ObservableProperty]
    private bool _showDimensions = true;

    [ObservableProperty]
    private bool _showAnnotations = true;

    [ObservableProperty]
    private bool _wireframeMode;

    [ObservableProperty]
    private bool _useMillimeters = true;

    [ObservableProperty]
    private bool _useInches;

    // --- Recalculate volume & surface area when dimensions change ---
    partial void OnLengthChanged(double oldValue, double newValue) => Recalculate();
    partial void OnWidthChanged(double oldValue, double newValue) => Recalculate();
    partial void OnDepthChanged(double oldValue, double newValue) => Recalculate();

    private void Recalculate()
    {
        double volume = Length * Width * Depth;
        double surfaceArea = 2 * (Length * Width + Length * Depth + Width * Depth);
        VolumeText = $"{volume:F2} mm³";
        SurfaceAreaText = $"{surfaceArea:F2} mm²";
    }

    // --- Unit mutual exclusivity ---
    partial void OnUseMillimetersChanged(bool oldValue, bool newValue)
    {
        if (newValue) UseInches = false;
    }

    partial void OnUseInchesChanged(bool oldValue, bool newValue)
    {
        if (newValue) UseMillimeters = false;
    }
}
