using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using Du.PMPage.Wpf;
using SolidWorks.Interop.sldworks;

namespace PMPageAddin;

/// <summary>
/// Demonstrates SldContentHost — hosting custom WPF content inside a
/// SolidWorks PropertyManagerPage via WindowFromHandle interop.
///
/// The WPF content (a styled ListBox with add/remove buttons) interacts
/// with native SW controls (TextBox, NumberBox, Label) through a shared
/// ViewModel. This shows bi-directional WPF↔SW data flow.
/// </summary>
public partial class ContentHostPage : SldPMPage
{
    private ContentHostViewModel _vm;

    public ContentHostPage() : base()
    {
        InitializeComponent();
    }

    public ContentHostPage(ISldWorks sw) : base()
    {
        App = sw;
        _vm = new ContentHostViewModel();
        DataContext = _vm;
        InitializeComponent();
    }

    private void AddButton_Click(object sender, RoutedEventArgs e)
    {
        _vm.AddOperation();
    }

    private void RemoveButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is FrameworkElement fe && fe.Tag is OperationItem item)
            _vm.RemoveOperation(item);
    }

    private void ClearButton_Click(object sender, RoutedEventArgs e)
    {
        _vm.ClearAll();
    }
}

// --- ViewModel ---
public partial class ContentHostViewModel : ObservableObject
{
    [ObservableProperty]
    private string _newItemText = string.Empty;

    [ObservableProperty]
    private double _newItemQty = 1.0;

    [ObservableProperty]
    private ObservableCollection<OperationItem> _operations = new();

    [ObservableProperty]
    private int _selectedOperationIndex = -1;

    [ObservableProperty]
    private string _itemCountText = "0";

    [ObservableProperty]
    private string _totalQtyText = "0";

    [ObservableProperty]
    private bool _exportToBOM = true;

    [ObservableProperty]
    private bool _exportToCutList;

    public void AddOperation()
    {
        if (string.IsNullOrWhiteSpace(NewItemText))
            return;

        Operations.Add(new OperationItem
        {
            Name = NewItemText,
            Quantity = NewItemQty
        });

        NewItemText = string.Empty;
        NewItemQty = 1.0;
        UpdateSummary();
    }

    public void RemoveOperation(OperationItem item)
    {
        Operations.Remove(item);
        UpdateSummary();
    }

    public void ClearAll()
    {
        Operations.Clear();
        UpdateSummary();
    }

    private void UpdateSummary()
    {
        ItemCountText = Operations.Count.ToString();
        TotalQtyText = Operations.Sum(o => o.Quantity).ToString("F1");
    }
}

// --- Model ---
public class OperationItem : INotifyPropertyChanged
{
    private string _name = string.Empty;
    private double _quantity = 1.0;

    public string Name
    {
        get => _name;
        set { _name = value; OnPropertyChanged(); OnPropertyChanged(nameof(Detail)); }
    }

    public double Quantity
    {
        get => _quantity;
        set { _quantity = value; OnPropertyChanged(); OnPropertyChanged(nameof(Detail)); }
    }

    public string Detail => $"Qty: {Quantity:F1}";

    public event PropertyChangedEventHandler PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
