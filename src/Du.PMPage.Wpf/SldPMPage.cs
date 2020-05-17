using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;
using SolidWorks.Interop.swpublished;
using System;
using System.Collections.Generic;
using System.Diagnostics.SymbolStore;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms.Integration;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Du.PMPage.Wpf
{
    /// <summary>
    /// 按照步骤 1a 或 1b 操作，然后执行步骤 2 以在 XAML 文件中使用此自定义控件。
    ///
    /// 步骤 1a) 在当前项目中存在的 XAML 文件中使用该自定义控件。
    /// 将此 XmlNamespace 特性添加到要使用该特性的标记文件的根
    /// 元素中:
    ///
    ///     xmlns:MyNamespace="clr-namespace:Du.PMPage.Wpf"
    ///
    ///
    /// 步骤 1b) 在其他项目中存在的 XAML 文件中使用该自定义控件。
    /// 将此 XmlNamespace 特性添加到要使用该特性的标记文件的根
    /// 元素中:
    ///
    ///     xmlns:MyNamespace="clr-namespace:Du.PMPage.Wpf;assembly=Du.PMPage.Wpf"
    ///
    /// 您还需要添加一个从 XAML 文件所在的项目到此项目的项目引用，
    /// 并重新生成以避免编译错误:
    ///
    ///     在解决方案资源管理器中右击目标项目，然后依次单击
    ///     “添加引用”->“项目”->[浏览查找并选择此项目]
    ///
    ///
    /// 步骤 2)
    /// 继续操作并在 XAML 文件中使用控件。
    ///
    ///     <MyNamespace:SldPMPage/>
    ///
    /// </summary>
    [ComVisible(true)]
    public class SldPMPage : UserControl, IPropertyManagerPage2Handler9
    {
        private ElementHost _host;

        #region ctor

        static SldPMPage()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(SldPMPage), new FrameworkPropertyMetadata(typeof(SldPMPage)));
        }

        public SldPMPage()
        {

        }

        public SldPMPage(ISldWorks app)
        {
            App = app;
        }

        #endregion

        private void CreatePage()
        {
            int options = (int)swPropertyManagerPageOptions_e.swPropertyManagerOptions_OkayButton |
                (int)swPropertyManagerPageOptions_e.swPropertyManagerOptions_CancelButton;

            int errors = 0;
            Page = (PropertyManagerPage2)App.CreatePropertyManagerPage(PageTitle, options, new PropertyManagerPage2Handler9Wrapper(this), ref errors);
            var errors_e = (swPropertyManagerPageStatus_e)errors;
            if (errors_e != swPropertyManagerPageStatus_e.swPropertyManagerPage_Okay)
            {
                throw new CreatePMPageErrorException($"{nameof(App.ICreatePropertyManagerPage)} Error: {errors_e.ToString()}");
            }
        }

        public void ShowPage()
        {
            CreatePage();
            AddUserControl();

            _host = new ElementHost();
            _host.Child = this;
            PMPageWinformHandle.SetWindowHandlex64(_host.Handle.ToInt64());

            var result_e = (swPropertyManagerPageStatus_e)Page.Show();
            if (result_e != swPropertyManagerPageStatus_e.swPropertyManagerPage_Okay)
            {
                throw new CreatePMPageErrorException($"{nameof(ShowPage)} Error: {result_e.ToString()}");
            }
        }

        private void AddUserControl()
        {
            int options = (int)swAddGroupBoxOptions_e.swGroupBoxOptions_Expanded |
                      (int)swAddGroupBoxOptions_e.swGroupBoxOptions_Visible;

           PMPageWinformHandle = (IPropertyManagerPageWindowFromHandle)Page.AddControl2(ID, (int)swPropertyManagerPageControlType_e.swControlType_WindowFromHandle, "WPF Control", (int)swPropertyManagerPageControlLeftAlign_e.swControlAlign_LeftEdge, options, "wpf control");

            PMPageWinformHandle.Height = 100;
        }

        public IPropertyManagerPage2 Page { get; private set; }

        public swPropertyManagerPageCloseReasons_e CloseReason { get; private set; }

        public IPropertyManagerPageWindowFromHandle PMPageWinformHandle { get; private set; }

        public int ID => 1;

        public swPropertyManagerPageOptions_e[] PageOptions { get; set; }


        #region Event

        #region Delegate

        /// <summary>
        /// Delegate for handling the parameters of property manager page closed event
        /// </summary>
        /// <param name="reason">Reason of closing as defined in <see href="http://help.solidworks.com/2016/english/api/swconst/solidworks.interop.swconst~solidworks.interop.swconst.swpropertymanagerpageclosereasons_e.html">swPropertyManagerPageCloseReasons_e Enumeration</see></param>
        public delegate void PropertyManagerPageClosedDelegate(swPropertyManagerPageCloseReasons_e reason);

        /// <summary>
        /// Delegate for handling the parameters of property manager page closing event
        /// </summary>
        /// <param name="reason">Reason of closing as defined in <see href="http://help.solidworks.com/2016/english/api/swconst/solidworks.interop.swconst~solidworks.interop.swconst.swpropertymanagerpageclosereasons_e.html">swPropertyManagerPageCloseReasons_e Enumeration</see></param>
        /// <param name="arg">Closing argument. Use this argument to cancel closing if needed</param>
        public delegate void PropertyManagerPageClosingDelegate(swPropertyManagerPageCloseReasons_e reason, ClosingArg arg);

        #endregion

        internal event Action HelpRequested;
        internal event Action WhatsNewRequested;

        /// <inheritdoc/>
        public event PropertyManagerPageClosingDelegate Closing;

        /// <inheritdoc/>
        public event PropertyManagerPageClosedDelegate Closed;

        #endregion

        #region DependencyProperty

        public ISldWorks App { get; set; }

        public static DependencyProperty AppProperty = DependencyProperty.Register(nameof(App), typeof(ISldWorks), typeof(SldPMPage), new PropertyMetadata(default));

        public string PageTitle { get => (string)GetValue(PageTitleProperty); set => SetValue(PageTitleProperty,value); }

        public static readonly DependencyProperty PageTitleProperty = DependencyProperty.Register(nameof(PageTitle), typeof(string), typeof(SldPMPage), new PropertyMetadata(default(string)));


        #endregion

        #region PMPHandler

        public void AfterActivation()
        {

        }

        public void OnClose(int Reason)
        {
            CloseReason = (swPropertyManagerPageCloseReasons_e)Reason;

            var arg = new ClosingArg();
            Closing?.Invoke(CloseReason, arg);

            if (arg.Cancel)
            {
                if (!string.IsNullOrEmpty(arg.ErrorTitle) || !string.IsNullOrEmpty(arg.ErrorMessage))
                {
                    var title = !string.IsNullOrEmpty(arg.ErrorTitle) ? arg.ErrorTitle : "Error";

                    App.ShowBubbleTooltipAt2(0, 0, (int)swArrowPosition.swArrowLeftTop,
                        title, arg.ErrorMessage, (int)swBitMaps.swBitMapTreeError,
                        "", "", 0, (int)swLinkString.swLinkStringNone, "", "");
                }

                const int S_FALSE = 1;
                throw new COMException(arg.ErrorMessage, S_FALSE);
            }
        }

        public void AfterClose()
        {
            Closed?.Invoke(CloseReason);
        }

        public bool OnHelp()
        {
            HelpRequested?.Invoke();
            return true;
        }

        public bool OnPreviousPage()
        {
            return true;
        }

        public bool OnNextPage()
        {
            return true;
        }

        public bool OnPreview()
        {
            return true;
        }

        public void OnWhatsNew()
        {
            WhatsNewRequested?.Invoke();
        }

        public void OnUndo()
        {
        }

        public void OnRedo()
        {
        }

        public bool OnTabClicked(int Id)
        {
            return true;
        }

        public void OnGroupExpand(int Id, bool Expanded)
        {
        }

        public void OnGroupCheck(int Id, bool Checked)
        {
        }

        public void OnCheckboxCheck(int Id, bool Checked)
        {
        }

        public void OnOptionCheck(int Id)
        {
        }

        public void OnButtonPress(int Id)
        {
        }

        public void OnTextboxChanged(int Id, string Text)
        {
        }

        public void OnNumberboxChanged(int Id, double Value)
        {
        }

        public void OnComboboxEditChanged(int Id, string Text)
        {
        }

        public void OnComboboxSelectionChanged(int Id, int Item)
        {
        }

        public void OnListboxSelectionChanged(int Id, int Item)
        {
        }

        public void OnSelectionboxFocusChanged(int Id)
        {
        }

        public void OnSelectionboxListChanged(int Id, int Count)
        {
        }

        public void OnSelectionboxCalloutCreated(int Id)
        {
        }

        public void OnSelectionboxCalloutDestroyed(int Id)
        {
        }

        public bool OnSubmitSelection(int Id, object Selection, int SelType, ref string ItemText)
        {
            return true;
        }

        public int OnActiveXControlCreated(int Id, bool Status)
        {
            return 1;
        }

        public void OnSliderPositionChanged(int Id, double Value)
        {
        }

        public void OnSliderTrackingCompleted(int Id, double Value)
        {
        }

        public bool OnKeystroke(int Wparam, int Message, int Lparam, int Id)
        {
            return true;
        }

        public void OnPopupMenuItem(int Id)
        {
        }

        public void OnPopupMenuItemUpdate(int Id, ref int retval)
        {
        }

        public void OnGainedFocus(int Id)
        {
        }

        public void OnLostFocus(int Id)
        {
        }

        public int OnWindowFromHandleControlCreated(int Id, bool Status)
        {
            return 1;
        }

        public void OnListboxRMBUp(int Id, int PosX, int PosY)
        {
        }

        public void OnNumberBoxTrackingCompleted(int Id, double Value)
        {
        }

        #endregion
    }
}
