using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Du.PMPage.Wpf;
using SolidWorks.Interop.sldworks;
using System;
using System.Collections.ObjectModel;

namespace PMPageAddin;

/// <summary>
/// Demonstrates hosting custom WPF content in a native SolidWorks TaskPane
/// via <see cref="SldTaskPane"/>.
/// </summary>
/// <remarks>
/// <para>Unlike <see cref="SldPMPage"/> (which converts WPF controls to native
/// SolidWorks PropertyManagerPage controls), the TaskPane hosts raw WPF content
/// through an <see cref="System.Windows.Forms.Integration.ElementHost"/>.
/// This gives you access to the full WPF ecosystem: data grids, charts, custom
/// drawings, animations, and any third-party WPF library.</para>
///
/// <para><b>Usage:</b>
/// <code>
/// var taskPane = new TaskPanePage(swApp);
/// taskPane.TaskPaneToolTip = "My Tool";
/// taskPane.TaskPaneBitmap = "MyBitmapResource";
/// taskPane.ShowView();
/// </code>
/// </para>
/// </remarks>
public partial class TaskPanePage : SldTaskPane
{
    private readonly TaskPaneViewModel _viewModel;

    /// <summary>
    /// Parameterless constructor for XAML designer support.
    /// </summary>
    public TaskPanePage() : base()
    {
        InitializeComponent();
        _viewModel = new TaskPaneViewModel(null);
        DataContext = _viewModel;
    }

    /// <summary>
    /// Initializes a new TaskPane page with the SolidWorks application reference.
    /// </summary>
    /// <param name="sw">The SolidWorks application instance.</param>
    public TaskPanePage(ISldWorks sw) : base(sw)
    {
        InitializeComponent();
        _viewModel = new TaskPaneViewModel((SldWorks)sw);
        DataContext = _viewModel;

        // Refresh document info on creation.
        _viewModel.RefreshDocumentInfo();
    }

    /// <inheritdoc/>
    protected override void OnTaskPaneCreated()
    {
        _viewModel.Log("TaskPane native view created successfully.");
        base.OnTaskPaneCreated();
    }

    /// <inheritdoc/>
    protected override void OnTaskPaneClosing()
    {
        _viewModel.Log("TaskPane is closing...");
        base.OnTaskPaneClosing();
    }
}

/// <summary>
/// View model for the <see cref="TaskPanePage"/> demo.
/// </summary>
public partial class TaskPaneViewModel : ObservableObject
{
    private readonly SldWorks _sw;

    [ObservableProperty]
    private string _documentTitle = "No document information available.";

    [ObservableProperty]
    private int _clickCount;

    [ObservableProperty]
    private string _countButtonText = "Click Me (0)";

    /// <summary>
    /// Gets the activity log entries.
    /// </summary>
    public ObservableCollection<string> LogEntries { get; } = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="TaskPaneViewModel"/> class.
    /// </summary>
    /// <param name="sw">The SolidWorks application instance (may be null for designer).</param>
    public TaskPaneViewModel(SldWorks sw)
    {
        _sw = sw;
        Log("TaskPane view model initialized.");
    }

    /// <summary>
    /// Refreshes the active document information from SolidWorks.
    /// </summary>
    [RelayCommand]
    public void RefreshDocumentInfo()
    {
        try
        {
            if (_sw == null)
            {
                DocumentTitle = "[Design mode — no SW instance]";
                Log("Refresh: no SolidWorks instance available.");
                return;
            }

            var doc = _sw.IActiveDoc2;
            if (doc == null)
            {
                DocumentTitle = "No document is currently open.";
                Log("Refresh: no active document.");
                return;
            }

            var title = doc.GetTitle();
            var path = doc.GetPathName();
            var typeName = doc.GetType() switch
            {
                var t when t == (int)SolidWorks.Interop.swconst.swDocumentTypes_e.swDocPART => "Part",
                var t when t == (int)SolidWorks.Interop.swconst.swDocumentTypes_e.swDocASSEMBLY => "Assembly",
                var t when t == (int)SolidWorks.Interop.swconst.swDocumentTypes_e.swDocDRAWING => "Drawing",
                _ => "Unknown"
            };

            DocumentTitle = $"{title}\nType: {typeName}\nPath: {path}";
            Log($"Document refreshed: {title} ({typeName})");
        }
        catch (Exception ex)
        {
            DocumentTitle = $"Error retrieving document info: {ex.Message}";
            Log($"Error refreshing document: {ex.Message}");
        }
    }

    /// <summary>
    /// Increments the click counter to demonstrate interactive WPF controls
    /// in the TaskPane.
    /// </summary>
    [RelayCommand]
    public void IncrementCount()
    {
        ClickCount++;
        CountButtonText = $"Clicked {ClickCount} time{(ClickCount == 1 ? "" : "s")}";
        Log($"Button clicked: count = {ClickCount}");
    }

    /// <summary>
    /// Adds a message to the activity log.
    /// </summary>
    /// <param name="message">The message to log.</param>
    public void Log(string message)
    {
        var timestamp = DateTime.Now.ToString("HH:mm:ss");
        LogEntries.Insert(0, $"[{timestamp}] {message}");

        // Keep the log from growing unbounded.
        while (LogEntries.Count > 50)
        {
            LogEntries.RemoveAt(LogEntries.Count - 1);
        }
    }
}
