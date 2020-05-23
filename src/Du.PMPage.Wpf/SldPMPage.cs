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
    public class SldPMPage : UserControl, IPropertyManagerPage2Handler9,IDisposable
    {
        #region Fields

        private ElementHost _host;
        private IModelDoc2 _doc;
        private static List<SldSelectionBox> _selectionBoxs = new List<SldSelectionBox>();

        #endregion

        #region ctor

        static SldPMPage()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(SldPMPage), new FrameworkPropertyMetadata(typeof(SldPMPage)));
        }

        /// <summary>
        /// Donnot use this ctor, Use the ctor SldPMPage(ISldWorks app) please.
        /// </summary>
        public SldPMPage()
        {
            if (!PageCreatedWhenShow)
            {
                if (App == null)
                {
                    throw new ArgumentNullException($"Use the ctor: SldPMPage(ISldWorks app) or Set {nameof(SldPMPage.App)} Property");
                }
                CreatePage();
                AddUserControl();
            }
        }

        public SldPMPage(ISldWorks app)
        {
            App = app;
            if (!PageCreatedWhenShow)
            {
                CreatePage();
                AddUserControl();
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Show this PMPage
        /// </summary>
        public void ShowPage()
        {
            _doc = App.IActiveDoc2;

            if (_doc == null)
            {
                throw new InvalidOperationException($"No active doc,can not show the pmpage");
            }

            if (Page == null)
            {
                CreatePage();
                AddUserControl();

                _host = new ElementHost();
            }

            _host.Child = this;
            PMPageWinformHandle.SetWindowHandlex64(_host.Handle.ToInt64());

            var result_e = (swPropertyManagerPageStatus_e)Page.Show();
            if (result_e != swPropertyManagerPageStatus_e.swPropertyManagerPage_Okay)
            {
                throw new CreatePMPageErrorException($"{nameof(ShowPage)} Error: {result_e.ToString()}");
            }

            AttachDocEvent();
        }

        /// <summary>
        /// ShowBubbleTooltipAt2
        /// </summary>
        /// <param name="title"></param>
        /// <param name="msg"></param>
        /// <param name="postion"></param>
        public void ShowBubbleTooltipAt2(string title,string msg,swArrowPosition postion= swArrowPosition.swArrowLeftTop)
        {
            App.ShowBubbleTooltipAt2(0, 0, (int)swArrowPosition.swArrowLeftTop,
                        title, msg, (int)swBitMaps.swBitMapTreeError,
                        "", "", 0, (int)swLinkString.swLinkStringNone, "", "");
        }
       
        /// <summary>
        /// Dispose the <see cref="ElementHost"/>
        /// </summary>
        public void Dispose()
        {
            if (_host != null)
            {
                _host.Controls.Clear();
                _host.Child = null;
                _host.Dispose();
            }
        }

        #endregion

        #region Private Methods

        //Create PMPage
        private void CreatePage()
        {

            int errors = 0;
            Page = (PropertyManagerPage2)App.CreatePropertyManagerPage(PageTitle, GetOptions(), new PropertyManagerPage2Handler9Wrapper(this), ref errors);
            var errors_e = (swPropertyManagerPageStatus_e)errors;
            if (errors_e != swPropertyManagerPageStatus_e.swPropertyManagerPage_Okay)
            {
                throw new CreatePMPageErrorException($"{nameof(App.ICreatePropertyManagerPage)} Error: {errors_e.ToString()}");
            }
        }

        //add WinformHandleControl for Page
        private void AddUserControl()
        {
            int options = (int)swAddGroupBoxOptions_e.swGroupBoxOptions_Expanded |
                      (int)swAddGroupBoxOptions_e.swGroupBoxOptions_Visible;

           PMPageWinformHandle = (IPropertyManagerPageWindowFromHandle)Page.AddControl2(ID, (int)swPropertyManagerPageControlType_e.swControlType_WindowFromHandle, "WPF Control", (int)swPropertyManagerPageControlLeftAlign_e.swControlAlign_LeftEdge, options, "wpf control");

            PMPageWinformHandle.Height = PageHeight;
        }

        //向PMPage中注册SelectionBox
        private static void RegisterSelectionBox(SldSelectionBox selectionBox)
        {
            _selectionBoxs.Add(selectionBox);
        }

        private void AttachDocEvent()
        {
            var doctype = (swDocumentTypes_e)_doc.GetType();
            switch (doctype)
            {
                case swDocumentTypes_e.swDocPART:
                    var partdoc = _doc as PartDoc;
                    partdoc.UserSelectionPreNotify += UserSelectionPreNotify;
                    partdoc.UserSelectionPostNotify += UserSelectionPostNotify;
                    partdoc.DeleteSelectionPreNotify += DeleteSelectionPreNotify;
                    break;
                case swDocumentTypes_e.swDocASSEMBLY:
                    var assDoc = _doc as AssemblyDoc;
                    assDoc.UserSelectionPreNotify += UserSelectionPreNotify;
                    assDoc.UserSelectionPostNotify += UserSelectionPostNotify;
                    break;
                case swDocumentTypes_e.swDocDRAWING:
                    var draDoc = _doc as DrawingDoc;
                    draDoc.UserSelectionPreNotify += UserSelectionPreNotify;
                    draDoc.UserSelectionPostNotify += UserSelectionPostNotify;
                    break;
                default:
                    throw new NotSupportedException($"Not Supported Doc Type:{doctype.ToString()}");
            }
        }

        private void DeAttachDocEvent()
        {
            var doctype = (swDocumentTypes_e)_doc.GetType();
            switch (doctype)
            {
                case swDocumentTypes_e.swDocPART:
                    var partdoc = _doc as PartDoc;
                    partdoc.UserSelectionPreNotify -= UserSelectionPreNotify;
                    partdoc.UserSelectionPostNotify -= UserSelectionPostNotify;
                    break;
                case swDocumentTypes_e.swDocASSEMBLY:
                    var assDoc = _doc as AssemblyDoc;
                    assDoc.UserSelectionPreNotify -= UserSelectionPreNotify;
                    assDoc.UserSelectionPostNotify -= UserSelectionPostNotify;
                    break;
                case swDocumentTypes_e.swDocDRAWING:
                    var draDoc = _doc as DrawingDoc;
                    draDoc.UserSelectionPreNotify -= UserSelectionPreNotify;
                    draDoc.UserSelectionPostNotify -= UserSelectionPostNotify;
                    break;
                default:
                    throw new NotSupportedException($"Not Supported Doc Type:{doctype.ToString()}");
            }
        }

        #endregion

        #region SolidWorks Selections

        //用户选择后通知--添加到列表中
        private int UserSelectionPostNotify()
        {

            return 0;
        }

        //用户添加前判断
        //返回1则取消此对象的选择，默认返回0
        private int UserSelectionPreNotify(int SelType)
        {
            var count = _selectionBoxs.Count;
            foreach (var item in _selectionBoxs)
            {

            }
            return 0;
        }

        //防止用户清除选择
        private int DeleteSelectionPreNotify()
        {

            return 0;
        }

        #endregion

        #region Properties

        public IPropertyManagerPage2 Page { get; private set; }

        public swPropertyManagerPageCloseReasons_e CloseReason { get; private set; }

        public IPropertyManagerPageWindowFromHandle PMPageWinformHandle { get; private set; }

        public int ID => 1;

        public virtual swPropertyManagerPageOptions_e[] PageOptions { get; set; }

        #endregion

        #region DependencyProperty

        public ISldWorks App { get; set; }

        /// <summary>
        /// <see cref="ISldWorks"/>
        /// </summary>
        public static DependencyProperty AppProperty = DependencyProperty.Register(nameof(App), typeof(ISldWorks), typeof(SldPMPage), new PropertyMetadata(default));

        /// <summary>
        /// PMPage Title
        /// </summary>
        public string PageTitle
        {
            get => (string)GetValue(PageTitleProperty); set
            {
                SetValue(PageTitleProperty, value);
                if (Page != null)
                {
                    Page.Title = value;
                }
            }
        }

        public static readonly DependencyProperty PageTitleProperty = DependencyProperty.Register(nameof(PageTitle), typeof(string), typeof(SldPMPage), new PropertyMetadata(default(string)));

        public bool PageCreatedWhenShow { get; set; } = true;

        /// <summary>
        /// Created Page When Call <see cref="ShowPage"/>
        /// </summary>
        public static readonly DependencyProperty PageCreatedWhenShowProperty = DependencyProperty.Register(nameof(PageCreatedWhenShow), typeof(bool), typeof(SldPMPage), new PropertyMetadata(true));

        /// <summary>
        /// The height of wpf control in this PMPage
        /// </summary>
        public int PageHeight
        {
            get { return (int)GetValue(PageHeightProperty); }
            set { SetValue(PageHeightProperty, value); }
        }

        // Using a DependencyProperty as the backing store for PageHeight.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PageHeightProperty =
            DependencyProperty.Register("PageHeight", typeof(int), typeof(SldPMPage), new PropertyMetadata(500));

        /// <summary>
        /// Gets or sets the title of the PropertyManager page.
        /// </summary>
        public bool Pinned
        {
            get { 
                var pinned = (bool)GetValue(PinnedProperty);
                if (Page != null)
                {
                    if (pinned != Page.Pinned)
                    {
                        SetValue(PinnedProperty, Page.Pinned);
                        pinned = !pinned;
                    }
                }
                return pinned;
            }
            set { SetValue(PinnedProperty, value);
                if (Page != null)
                {
                    Page.Pinned = value;
                }
            }
        }

        // Using a DependencyProperty as the backing store for Pinned.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PinnedProperty =
            DependencyProperty.Register("Pinned", typeof(bool), typeof(SldPMPage), new PropertyMetadata(false));

        #endregion

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

        /// <summary>
        /// Fired when user click help button
        /// </summary>
        public event Action HelpRequested;

        /// <summary>
        /// Fired when user click what's new button
        /// </summary>
        public event Action WhatsNewRequested;

        /// <summary>
        /// Fired when the page Activated
        /// </summary>
        public event Action AfterActivationEvent;

        /// <summary>
        /// Event fired when the page closing
        /// </summary>
        public event PropertyManagerPageClosingDelegate Closing;

        /// <summary>
        /// Event fired when the page closed
        /// </summary>
        public event PropertyManagerPageClosedDelegate Closed;

        /// <summary>
        /// Fired when user click ok on the page
        /// </summary>
        public event Action OkClicked;

        #endregion

        #region Helper Methods

        //Get Options from array
        private int GetOptions()
        {
            return CombineOptions(PageOptions);
        }

        //combine array to a int type
        internal static int CombineOptions(IEnumerable<swPropertyManagerPageOptions_e> options)
        {
            return CombineToInt(options, v => (int)v);
        }

        //combine array to a int type
        internal static int CombineToInt<T>(IEnumerable<T> leftAlign, Func<T, int> fn)
        {
            return leftAlign?.Aggregate(0, (acc, v) => acc | fn(v))
                ?? ((int)swPropertyManagerPageOptions_e.swPropertyManagerOptions_OkayButton | (int)swPropertyManagerPageOptions_e.swPropertyManagerOptions_CancelButton);
        }

        #endregion

        #region PMPHandler

        public void AfterActivation()
        {
            AfterActivationEvent?.Invoke();
        }

        public void OnWhatsNew()
        {
            WhatsNewRequested?.Invoke();
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

            //DeAttach Doc Selection Event
            _selectionBoxs.Clear();
            DeAttachDocEvent();
        }

        public void AfterClose()
        {
            Closed?.Invoke(CloseReason);
            if (CloseReason == swPropertyManagerPageCloseReasons_e.swPropertyManagerPageClose_Okay)
            {
                OkClicked?.Invoke();
            }
        }

        public bool OnHelp()
        {
            HelpRequested?.Invoke();
            return true;
        }

        //----------------not used-------------

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
