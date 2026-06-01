using System.Windows.Media;
using Du.PMPage.Wpf.Utils;
using SolidWorks.Interop.sldworks;

namespace Du.PMPage.Wpf.Controls;

/// <summary>
/// A WPF wrapper for <see cref="IPropertyManagerPageLabel"/> that renders a message label
/// with a yellow background. Inherits from <see cref="SldLabel"/> and overrides
/// <see cref="SldLabel.SetSldControl"/> to apply the yellow background color automatically.
/// </summary>
public class SldLabelMsg : SldLabel
{
    /// <summary>
    /// Applies the <see cref="SldLabel.SldText"/> to the native control caption
    /// and sets the background color to yellow.
    /// </summary>
    protected override void SetSldControl()
    {
        SControl.Caption = SldText;
        if (SControl is IPropertyManagerPageControl control)
        {
            control.BackgroundColor = Information.RGB(
                Colors.Yellow.R,
                Colors.Yellow.G,
                Colors.Yellow.B
            );
        }
    }
}
