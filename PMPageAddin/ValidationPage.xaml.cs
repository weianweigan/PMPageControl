using System;
using System.Text.RegularExpressions;
using CommunityToolkit.Mvvm.ComponentModel;
using Du.PMPage.Wpf;
using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;

namespace PMPageAddin;

/// <summary>
/// Demonstrates input validation using:
/// 1. CloseCommand with CanExecute — blocks OK button when invalid.
/// 2. Closing event — shows bubble tooltip with error details.
/// 3. Real-time validation feedback via checkboxes and status labels.
/// 4. OkClicked event — performs action when validation passes.
/// </summary>
public partial class ValidationPage : SldPMPage
{
    public ValidationPage() : base()
    {
        InitializeComponent();
    }

    public ValidationPage(ISldWorks sw) : base()
    {
        App = sw;
        var vm = new ValidationViewModel();
        DataContext = vm;
        InitializeComponent();

        // Wire up the Closing event for server-side validation
        Closing += (reason, arg) =>
        {
            if (reason == swPropertyManagerPageCloseReasons_e.swPropertyManagerPageClose_Okay)
            {
                if (!vm.IsAllValid)
                {
                    arg.Cancel = true;
                    arg.ErrorTitle = "Validation Failed";
                    arg.ErrorMessage = vm.StatusMessage;
                }
            }
        };

        // Handle successful OK
        OkClicked += () =>
        {
            sw.ShowBubbleTooltipAt2(
                0, 0,
                (int)swArrowPosition.swArrowLeftTop,
                "Success",
                $"Part '{vm.PartName}' (Rev {vm.Revision}) created with Quantity={vm.Quantity}",
                (int)swBitMaps.swBitMapTreeError,
                "", "", 0,
                (int)swLinkString.swLinkStringNone,
                "", ""
            );
        };
    }
}

public partial class ValidationViewModel : ObservableObject
{
    // --- Input fields ---
    [ObservableProperty]
    private string _partName = string.Empty;

    [ObservableProperty]
    private string _revision = string.Empty;

    [ObservableProperty]
    private double _quantity = 1.0;

    // --- Validation indicators ---
    [ObservableProperty]
    private bool _nameValid;

    [ObservableProperty]
    private bool _revisionValid;

    [ObservableProperty]
    private bool _quantityValid = true;

    [ObservableProperty]
    private string _statusMessage = "Fill in all required fields (*)";

    // --- Aggregate ---
    public bool IsAllValid => NameValid && RevisionValid && QuantityValid;

    // --- CloseCommand with CanExecute ---
    public CloseCommand CloseCmd { get; }

    public ValidationViewModel()
    {
        CloseCmd = new CloseCommand(
            execute: () => { /* OK action handled by OkClicked event */ },
            canExecute: () => IsAllValid
        );
    }

    // --- Real-time validation on property changes ---
    partial void OnPartNameChanged(string oldValue, string newValue)
    {
        NameValid = !string.IsNullOrWhiteSpace(newValue);
        UpdateStatus();
    }

    partial void OnRevisionChanged(string oldValue, string newValue)
    {
        // Revision format: A-Z or 1.0 style
        RevisionValid = Regex.IsMatch(newValue ?? "", @"^[A-Z]$|^\d+\.\d+$");
        UpdateStatus();
    }

    partial void OnQuantityChanged(double oldValue, double newValue)
    {
        QuantityValid = newValue >= 1 && newValue <= 1000;
        UpdateStatus();
    }

    private void UpdateStatus()
    {
        // The Closing event and CloseCommand.CanExecute both validate at close time,
        // so SW blocks OK when conditions are not met.
        if (IsAllValid)
            StatusMessage = "✓ All validation rules pass — ready to create.";
        else if (!NameValid)
            StatusMessage = "✗ Part name is required.";
        else if (!RevisionValid)
            StatusMessage = "✗ Revision must be a letter (A-Z) or number (e.g. 1.0).";
        else if (!QuantityValid)
            StatusMessage = "✗ Quantity must be between 1 and 1000.";
        else
            StatusMessage = "Fill in all required fields (*)";
    }
}
