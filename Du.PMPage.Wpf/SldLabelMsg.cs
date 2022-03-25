using System.Windows;
using System.Windows.Controls;

namespace Du.PMPage.Wpf
{
    public class SldLabelMsg:Label
    {
        static SldLabelMsg()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(SldLabelMsg), new FrameworkPropertyMetadata(typeof(SldLabelMsg)));
        }

    }
}
