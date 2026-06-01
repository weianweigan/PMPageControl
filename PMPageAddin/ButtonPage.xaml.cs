using System;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Du.PMPage.Wpf;
using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;

namespace PMPageAddin;

/// <summary>
/// Demonstrates button controls: SldButton, SldBitmapButton, and
/// SldCheckableBitmapButton with ICommand bindings via RelayCommand.
/// Shows how buttons interact with other controls via data binding.
/// </summary>
public partial class ButtonPage : SldPMPage
{
    public ButtonPage() : base()
    {
        InitializeComponent();
    }

    public ButtonPage(ISldWorks sw) : base()
    {
        App = sw;
        InitializeComponent();
        DataContext = new ButtonViewModel(sw);
    }
}

public partial class ButtonViewModel : ObservableObject
{
    private readonly ISldWorks _sw;
    private int _clickCount;

    public ButtonViewModel(ISldWorks sw)
    {
        _sw = sw ?? throw new ArgumentNullException(nameof(sw));
    }

    [ObservableProperty]
    private string _lastAction = "None";

    [ObservableProperty]
    private string _clickCountText = "0";

    [ObservableProperty]
    private bool _isPreviewMode;

    [ObservableProperty]
    private string _previewStatus = "OFF";

    [ObservableProperty]
    private bool _actionsEnabled = true;

    private void IncrementClickCount()
    {
        _clickCount++;
        ClickCountText = _clickCount.ToString();
    }

    // --- Standard Button Commands ---

    [RelayCommand]
    private void CreateCube()
    {
        IncrementClickCount();
        LastAction = $"CreateCube clicked (#{_clickCount})";
    }

    [RelayCommand]
    private void ShowMessage()
    {
        IncrementClickCount();
        LastAction = $"ShowMessage clicked (#{_clickCount})";
        _sw.ShowBubbleTooltipAt2(
            0, 0,
            (int)swArrowPosition.swArrowLeftTop,
            "PMPage Demo",
            "This is a message from the PMPage button!",
            (int)swBitMaps.swBitMapTreeError,
            "", "", 0,
            (int)swLinkString.swLinkStringNone,
            "", ""
        );
    }

    // --- Bitmap Button Commands ---

    [RelayCommand]
    private void OpenFile()
    {
        IncrementClickCount();
        LastAction = $"OpenFile clicked (#{_clickCount})";
    }

    [RelayCommand]
    private void SaveFile()
    {
        IncrementClickCount();
        LastAction = $"SaveFile clicked (#{_clickCount})";
    }

    // --- Checkable Bitmap Button Command ---

    [RelayCommand]
    private void TogglePreview()
    {
        IsPreviewMode = !IsPreviewMode;
        PreviewStatus = IsPreviewMode ? "ON" : "OFF";
        IncrementClickCount();
        LastAction = $"Preview toggled: {PreviewStatus}";
    }

    // --- Conditional Command ---
    [RelayCommand]
    private void ConditionalAction()
    {
        IncrementClickCount();
        LastAction = $"ConditionalAction (#{_clickCount})";
    }
}
