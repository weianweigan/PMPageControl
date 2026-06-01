using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using Du.PMPage.Wpf;
using SolidWorks.Interop.sldworks;

namespace PMPageAddin;

/// <summary>
/// Demonstrates SldSelectionBox usage with different selection filters
/// (faces, edges, components, multi-type) and single/multi-entity modes.
/// Uses Mark values (powers of 2) to distinguish selection sets.
/// Selection filters are set in XAML via the
/// <see cref="Du.PMPage.Wpf.Controls.SwSelectTypesExtension"/> markup extension.
/// Selection events are handled via ObservableCollection change notifications.
/// </summary>
public partial class SelectionBoxPage : SldPMPage
{
    public SelectionBoxPage()
        : base()
    {
        InitializeComponent();
    }

    public SelectionBoxPage(ISldWorks sw)
        : base()
    {
        App = sw;
        InitializeComponent();
        DataContext = new SelectionBoxViewModel();
    }
}

public partial class SelectionBoxViewModel : ObservableObject
{
    public SelectionBoxViewModel()
    {
        Faces = new ObservableCollection<SwSeleTypeObjectPair>();
        Edges = new ObservableCollection<SwSeleTypeObjectPair>();
        Components = new ObservableCollection<SwSeleTypeObjectPair>();
        MultiTypeItems = new ObservableCollection<SwSeleTypeObjectPair>();

        // Subscribe to collection changes to update status
        Faces.CollectionChanged += (s, e) => UpdateSelectionStatus();
        Edges.CollectionChanged += (s, e) => UpdateSelectionStatus();
        Components.CollectionChanged += (s, e) => UpdateSelectionStatus();
        MultiTypeItems.CollectionChanged += (s, e) => UpdateSelectionStatus();
    }

    // --- Face Selection ---
    [ObservableProperty]
    private ObservableCollection<SwSeleTypeObjectPair> _faces;

    [ObservableProperty]
    private string _faceName = "(none)";

    // --- Edge Selection ---
    [ObservableProperty]
    private ObservableCollection<SwSeleTypeObjectPair> _edges;

    [ObservableProperty]
    private string _edgeName = "(none)";

    // --- Component Selection (multi) ---
    [ObservableProperty]
    private ObservableCollection<SwSeleTypeObjectPair> _components;

    [ObservableProperty]
    private string _componentCount = "0";

    // --- Multi-Type Selection ---
    [ObservableProperty]
    private ObservableCollection<SwSeleTypeObjectPair> _multiTypeItems;

    [ObservableProperty]
    private string _multiTypeCount = "0";

    // --- Settings ---
    [ObservableProperty]
    private bool _autoFocusNext = true;

    // --- Summary ---
    [ObservableProperty]
    private string _totalSelected = "0";

    private void UpdateSelectionStatus()
    {
        // Face
        if (Faces.Count > 0)
            FaceName = Faces[0].Name ?? "(face)";
        else
            FaceName = "(none)";

        // Edge
        if (Edges.Count > 0)
            EdgeName = Edges[0].Name ?? "(edge)";
        else
            EdgeName = "(none)";

        // Component count
        ComponentCount = Components.Count.ToString();

        // Multi-type count
        MultiTypeCount = MultiTypeItems.Count.ToString();

        // Total
        int total = Faces.Count + Edges.Count + Components.Count + MultiTypeItems.Count;
        TotalSelected = total.ToString();
    }
}
