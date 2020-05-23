using Du.PMPage.Wpf;
using SolidWorks.Interop.sldworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PMPageWindow
{
    /// <summary>
    /// Dimension.xaml 的交互逻辑
    /// </summary>
    public partial class Dimension : SldPMPage
    {
        public Dimension()
        {
            InitializeComponent();
        }

        public Dimension(ISldWorks app):base(app)
        {

        }
    }
}
