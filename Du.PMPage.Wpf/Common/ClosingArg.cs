namespace Du.PMPage.Wpf;

/// <summary>
/// Represents the argument passed to the <c>Closing</c> event of a Property Manager Page,
/// allowing handlers to cancel the close operation and provide error feedback to the user.
/// </summary>
/// <remarks>
/// <para>
/// When <see cref="Cancel"/> is set to <see langword="true"/> and both
/// <see cref="ErrorTitle"/> and <see cref="ErrorMessage"/> are non-empty,
/// the framework displays an error popup box next to the property manager page.
/// </para>
/// <para>
/// Instances of this class are created internally by the framework and passed to
/// event handlers attached to <see cref="SldPMPageBase.Closing"/>.
/// </para>
/// </remarks>
public class ClosingArg
{
    /// <summary>
    /// Gets or sets a value indicating whether the closing of the property manager page
    /// should be cancelled.
    /// </summary>
    /// <value>
    /// <see langword="true"/> to cancel the close operation and keep the page open;
    /// otherwise, <see langword="false"/> to allow the page to close normally.
    /// </value>
    public bool Cancel { get; set; }

    /// <summary>
    /// Gets or sets the title of the error to display to the user when the close is
    /// cancelled.
    /// </summary>
    /// <value>
    /// A short error title (e.g., "Validation Failed"), or an empty string if no error
    /// should be displayed. Only shown when <see cref="Cancel"/> is <see langword="true"/>.
    /// </value>
    public string ErrorTitle { get; set; }

    /// <summary>
    /// Gets or sets the error message to display to the user when the close is cancelled.
    /// </summary>
    /// <value>
    /// A descriptive error message explaining why the close was blocked, or an empty string
    /// if no error should be displayed. Only shown when <see cref="Cancel"/> is
    /// <see langword="true"/>.
    /// </value>
    public string ErrorMessage { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ClosingArg"/> class.
    /// </summary>
    /// <remarks>
    /// This constructor is internal. Instances are created by the framework when the
    /// <c>Closing</c> event is raised.
    /// </remarks>
    internal ClosingArg()
    {
    }
}
