using System.Windows.Media;
using Du.PMPage.Wpf.Utils;
using SolidWorks.Interop.sldworks;

namespace Du.PMPage.Wpf.Controls;

/// <summary>
/// the wpf wrapper of <see cref="IPropertyManagerPageLabel"/> 背景为黄色的消息文字
/// </summary>
public class SldLabelMsg : SldLabel
{
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
