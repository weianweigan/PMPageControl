using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using Du.PMPage.Wpf;
using SolidWorks.Interop.sldworks;

namespace PMPageAddin;

/// <summary>
/// SelectionPage using SldContentPage — pure WPF content hosted inside
/// a SolidWorks PropertyManagerPage via WindowFromHandle.
/// No SldControl / native SW controls in XAML.
/// </summary>
public partial class SelectionPage : SldPMPage
{
    public SelectionPage() : base()
    {
        InitializeComponent();
    }

    public SelectionPage(ISldWorks sw) : base()
    {
        App = sw;
        InitializeComponent();
        DataContext = new SelectionPageViewModel();
    }
}

public partial class SelectionPageViewModel : ObservableObject
{
    private ObservableCollection<SwSeleTypeObjectPair> _selections =
        new ObservableCollection<SwSeleTypeObjectPair>();

    public ObservableCollection<SwSeleTypeObjectPair> Selections
    {
        get => _selections;
        set => SetProperty(ref _selections, value);
    }

    [ObservableProperty]
    private double _num1;

    [ObservableProperty]
    private double _num2;

    partial void OnNum1Changed(double oldValue, double newValue)
    {
        Num2 = Num1 + 10;
    }
}
