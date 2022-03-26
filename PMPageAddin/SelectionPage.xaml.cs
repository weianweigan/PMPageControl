using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.InteropServices;
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
using Du.PMPage.Wpf;
using GalaSoft.MvvmLight;
using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;

namespace PMPageAddin
{
    /// <summary>
    /// SelectionPage.xaml 的交互逻辑
    /// </summary>
    [ComVisible(true)]
    public partial class SelectionPage : SldPMPage
    {
        public SelectionPage(ISldWorks sw):base(sw)
        {
            ModelDoc2 doc2;
            InitializeComponent();

            DataContext = new SelectionPageViewModel();
        }
    }

    public class SelectionPageViewModel : ViewModelBase
    {
        private string _msg = "Selection Msg";
        private ObservableCollection<swSeleTypeObjectPair> _selections = new ObservableCollection<swSeleTypeObjectPair>();
        private int _count;

        public string Msg { get => _msg; set => Set(ref _msg, value); }

        public List<swSelectType_e> AllowSelectionTypes { get; set; } =
            new List<swSelectType_e>() { swSelectType_e.swSelFACES };

        public ObservableCollection<swSeleTypeObjectPair> Selections { 
            get => _selections; 
            set => _selections = value; }

        private CloseCommand _closeCommand;

        public CloseCommand CloseCommand
        {
            get
            {
                if (_closeCommand == null)
                {
                    _closeCommand = new CloseCommand(Close,CanClose);
                }

                return _closeCommand;
            }
        }

        private bool CanClose()
        {
            if(_count++ == 0)
            {
                CloseCommand.ErrorTitle = "Msg";
                CloseCommand.BubbleTooltip = "Send msg to user";
                return false;
            }
            return true;
        }

        private void Close()
        {
            
        }
    }
}
