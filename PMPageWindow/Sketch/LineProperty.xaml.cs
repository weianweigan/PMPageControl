using Du.PMPage.Wpf;
using GalaSoft.MvvmLight;
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
    /// LineProperty.xaml 的交互逻辑
    /// </summary>
    public partial class LineProperty : SldPMPage
    {
        public LineProperty()
        {
            InitializeComponent();
            DataContext = new LinePropertyViewModel();
        }

        public LineProperty(ISldWorks app) : base(app)
        {
        }
    }

    public class LinePropertyViewModel : ViewModelBase
    {
        private double _doubelValue;

        public double DoubleValue { get => _doubelValue; set => Set(ref _doubelValue ,value); }
    }
}
