using SolidWorks.Interop.swconst;

namespace Du.PMPage.Wpf.Controls;

/// <summary>
/// Defines the range, unit, and increment settings for a SolidWorks number box control.
/// Used to configure the spinner behavior and validation bounds of a
/// <see cref="SldNumberBox"/> or native SolidWorks number box.
/// </summary>
public class NumberBoxRange
{
    /// <summary>
    /// Gets or sets the unit type for the number box.
    /// </summary>
    public swNumberboxUnitType_e Units { get; set; }

    /// <summary>
    /// Gets or sets the minimum allowed value.
    /// </summary>
    public double Minimum { get; set; }

    /// <summary>
    /// Gets or sets the maximum allowed value.
    /// </summary>
    public double Maximum { get; set; }

    /// <summary>
    /// Gets or sets whether the range is inclusive. If true, the value must be
    /// within [<see cref="Minimum"/>, <see cref="Maximum"/>]; if false, the value
    /// must be outside that range. Default is true.
    /// </summary>
    public bool Inclusive { get; set; } = true;

    /// <summary>
    /// Gets or sets the amount by which the value changes on a single increment
    /// (e.g., when clicking the spinner arrows). Default is 1.
    /// </summary>
    public double Increment { get; set; } = 1;

    /// <summary>
    /// Gets or sets the amount by which the value changes during fast scrolling
    /// (e.g., when the mouse wheel is spun quickly). Default is 1.
    /// </summary>
    public double FastIncr { get; set; } = 1;

    /// <summary>
    /// Gets or sets the amount by which the value changes during slow scrolling
    /// (e.g., when the mouse wheel is spun slowly). Default is 1.
    /// </summary>
    public double SlowIncr { get; set; } = 1;
}
