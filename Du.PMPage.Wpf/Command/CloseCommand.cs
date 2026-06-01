using System;
using System.Windows.Input;

namespace Du.PMPage.Wpf;

/// <summary>
/// Represents a command that handles the closing of a SolidWorks Property Manager Page.
/// </summary>
/// <remarks>
/// <para>
/// This command is bound to <see cref="SldPMPageBase.CloseCommand"/> and is invoked by the
/// framework when the user clicks OK or Cancel. The framework passes <see cref="CancelParameter"/>
/// to <see cref="Execute"/> for cancel actions, and <c>null</c> (or any other value) for OK actions.
/// </para>
/// <para>
/// When <see cref="CanExecute"/> returns <c>false</c>, the OK button is disabled in the UI.
/// If the user bypasses this (e.g., via keyboard), <see cref="ErrorTitle"/> and
/// <see cref="BubbleTooltip"/> are displayed in a bubble tooltip next to the PMPage,
/// and the close is cancelled.
/// </para>
/// <para>Usage example:</para>
/// <code>
/// CloseCmd = new CloseCommand(
///     execute: () => { /* OK action */ },
///     canExecute: () => IsFormValid,
///     cancelClick: () => { /* cleanup on cancel */ })
/// {
///     ErrorTitle = "Validation Failed",
///     BubbleTooltip = "Please fill in all required fields."
/// };
/// </code>
/// </remarks>
public class CloseCommand : ICommand
{
    /// <summary>
    /// The parameter value passed to <see cref="Execute(object)"/> when the user clicks the
    /// Cancel button or otherwise closes the page without clicking OK.
    /// </summary>
    public const int CancelParameter = -1;

    private readonly Action _execute;
    private readonly Func<bool> _canExecute;
    private readonly Action _cancelClick;

    /// <summary>
    /// Initializes a new instance of the <see cref="CloseCommand"/> class.
    /// </summary>
    /// <param name="execute">
    /// The action to invoke when the user clicks OK. Must not be <c>null</c>.
    /// </param>
    /// <param name="canExecute">
    /// An optional predicate that returns <c>true</c> if the OK action is currently allowed.
    /// When <c>null</c>, the OK action is always allowed. Use this to disable the OK button
    /// when the page is in an invalid state.
    /// </param>
    /// <param name="cancelClick">
    /// An optional action to invoke when the user clicks Cancel or otherwise dismisses the page
    /// without clicking OK. Use this for cleanup or logging. When <c>null</c>, no additional
    /// action is taken on cancel.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="execute"/> is <c>null</c>.
    /// </exception>
    public CloseCommand(Action execute, Func<bool> canExecute, Action cancelClick = null)
    {
        _execute = execute ?? throw new ArgumentNullException(nameof(execute));
        _canExecute = canExecute;
        _cancelClick = cancelClick;
    }

    /// <summary>
    /// Gets or sets the title displayed in the error bubble tooltip when <see cref="CanExecute"/>
    /// returns <c>false</c> and the user attempts to close the page via OK.
    /// </summary>
    /// <remarks>
    /// This property is read by <see cref="SldPMPageBase.OnClose"/> to provide user-facing error
    /// feedback. Keep the title short and descriptive (e.g., "Validation Failed").
    /// If left <c>null</c>, the framework falls back to a generic "Cannot execute" message.
    /// </remarks>
    public string ErrorTitle { get; set; }

    /// <summary>
    /// Gets or sets the detail message displayed in the error bubble tooltip when
    /// <see cref="CanExecute"/> returns <c>false</c> and the user attempts to close via OK.
    /// </summary>
    /// <remarks>
    /// This property is read by <see cref="SldPMPageBase.OnClose"/>. Provide a clear, actionable
    /// message explaining why the page cannot be closed (e.g., "Part name is required.").
    /// If left <c>null</c>, the framework falls back to a generic "Unknown error" message.
    /// </remarks>
    public string BubbleTooltip { get; set; }

    /// <inheritdoc />
    /// <remarks>
    /// This implementation subscribes to <see cref="CommandManager.RequerySuggested"/>,
    /// which causes WPF to re-evaluate <see cref="CanExecute"/> on most UI interactions.
    /// For state changes that WPF cannot detect automatically, call
    /// <see cref="RaiseCanExecuteChanged"/> to force an immediate re-query.
    /// </remarks>
    public event EventHandler CanExecuteChanged
    {
        add { CommandManager.RequerySuggested += value; }
        remove { CommandManager.RequerySuggested -= value; }
    }

    /// <summary>
    /// Forces WPF to re-evaluate <see cref="CanExecute"/> by invalidating the current
    /// command state via <see cref="CommandManager.InvalidateRequerySuggested"/>.
    /// </summary>
    /// <remarks>
    /// Call this method when the return value of your <c>canExecute</c> predicate changes due to
    /// state that WPF cannot detect automatically — for example, after a background operation
    /// completes, or when validation state changes programmatically rather than through a UI
    /// interaction. Without this call, the OK button may remain disabled (or enabled) until
    /// the next WPF-triggered requery.
    /// </remarks>
    public void RaiseCanExecuteChanged()
    {
        CommandManager.InvalidateRequerySuggested();
    }

    /// <summary>
    /// Determines whether the OK action can execute based on the current state.
    /// </summary>
    /// <param name="parameter">Not used by this implementation.</param>
    /// <returns>
    /// <c>true</c> if the OK action is allowed to execute; otherwise <c>false</c>.
    /// Returns <c>true</c> when no <c>canExecute</c> predicate was provided to the constructor.
    /// </returns>
    public bool CanExecute(object parameter)
    {
        return _canExecute?.Invoke() ?? true;
    }

    /// <summary>
    /// Executes either the OK action or the cancel action, depending on the parameter value.
    /// </summary>
    /// <param name="parameter">
    /// <list type="bullet">
    ///   <item>
    ///     <description><see cref="CancelParameter"/> (-1) — invokes the cancel action
    ///     (user clicked Cancel or dismissed the page).</description>
    ///   </item>
    ///   <item>
    ///     <description>Any other value (including <c>null</c>) — invokes the OK action
    ///     (user clicked OK).</description>
    ///   </item>
    /// </list>
    /// </param>
    public void Execute(object parameter)
    {
        if (parameter is int arg && arg == CancelParameter)
        {
            _cancelClick?.Invoke();
        }
        else
        {
            _execute.Invoke();
        }
    }
}
