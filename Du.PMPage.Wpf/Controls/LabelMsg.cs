using System.Windows;
using System.Windows.Controls;

namespace Du.PMPage.Wpf.Controls;

/// <summary>
/// A <see cref="Label"/> subclass that serves as a design-time styling marker.
/// Overrides the default style key so that it can be targeted by a dedicated
/// control template in the theme resources (e.g., SldLabelMsg.xaml),
/// distinguishing it from a plain <see cref="Label"/>.
/// </summary>
public class LabelMsg : Label
{
    static LabelMsg()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(LabelMsg),
            new FrameworkPropertyMetadata(typeof(LabelMsg))
        );
    }
}
