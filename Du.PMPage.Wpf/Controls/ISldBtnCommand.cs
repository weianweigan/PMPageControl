using System.Windows.Input;

namespace Du.PMPage.Wpf.Controls;

/// <summary>
/// Exposes an <see cref="ICommand"/> that can be bound to a SolidWorks button control.
/// Implementing this interface on a data context enables command binding for <see cref="SldButton"/>.
/// </summary>
public interface ISldBtnCommand
{
    /// <summary>
    /// Gets or sets the command to execute when the associated button is clicked.
    /// </summary>
    ICommand Command { get; set; }
}
