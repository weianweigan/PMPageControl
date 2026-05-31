using System.Windows;
using System.Windows.Controls;

namespace Du.PMPage.Wpf.Controls;

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
