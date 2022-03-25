using Microsoft.VisualBasic;
using SolidWorks.Interop.sldworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Du.PMPage.Wpf
{
    /// <summary>
    /// the wpf wrapper of <see cref="IPropertyManagerPageLabel"/> 背景为黄色的消息文字
    /// </summary>
    public class SldLableMsg:SldLabel
    {        
        protected override void SetSldControl()
        {
            SControl.Caption = SldText;
            if (SControl is IPropertyManagerPageControl control)
            {
                control.BackgroundColor = Information.RGB(Colors.Yellow.R, Colors.Yellow.G, Colors.Yellow.B);
            }
        }
    }
}
