using System;
using System.Linq;
using System.Windows;
using System.Threading;
using System.Diagnostics;
using System.Windows.Input;
using System.Windows.Controls;
using System.Collections.Generic;
using SolidWorks.Interop.swconst;
using SolidWorks.Interop.sldworks;
using System.Runtime.InteropServices;
using SolidWorks.Interop.swpublished;
using System.Windows.Forms.Integration;

namespace Du.PMPage.Wpf
{
    [ComVisible(true)]
    public class SldPMPage : UserControl, IPropertyManagerPage2Handler9, IDisposable
    {
        public const int S_FALSE = 1;

        #region Fields

        private ElementHost _host;
        private IModelDoc2 _doc;

        //Wpf窗口放在GroupBox中需要保持引用，否则会被错误的垃圾回收，造成CallBack错误，
        private IPropertyManagerPageGroup _wpfGroup;

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

                AddAllControls();
            }

        }

        private void AddAllControls()
        {
            AddSldControls();

            AddUserControl();

            AddSldBackControls();
        }

        public SldPMPage(ISldWorks app)
        {
            App = app;
            if (!PageCreatedWhenShow)
            {
                CreatePage();

                AddAllControls();
            }

        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Show this PMPage
        /// </summary>
        public void ShowPage()
        {
            //使用单线程单元，防止SolidWorks闪退
            Thread.CurrentThread.TrySetApartmentState(System.Threading.ApartmentState.STA);

            _doc = App.IActiveDoc2;

            if (_doc == null)
            {
                throw new InvalidOperationException($"No active doc,can not show the pmpage");
            }

            if (Page == null)
            {
                CreatePage();

                AddSldControls();
                AddUserControl();
                AddSldBackControls();

                _host = new ElementHost();                
            }

            //数据绑定在此处执行
            _host.Child = this;
            PMPageWinformHandle.SetWindowHandlex64(_host.Handle.ToInt64());
            
            OnShowPagePreview();

            swPropertyManagerPageStatus_e result_e = swPropertyManagerPageStatus_e.swPropertyManagerPage_Okay;
            try
            {
                Debug.Print("PMPage.Show");
                result_e = (swPropertyManagerPageStatus_e)Page.Show();// 2((int)swPropertyManagerPageShowOptions_e.swPropertyManagerShowOptions_StackPage);
                Debug.Print("PMPage.Showed");
            }
            catch (Exception)
            {
                throw;
            }

            if (result_e != swPropertyManagerPageStatus_e.swPropertyManagerPage_Okay)
            {
                throw new CreatePMPageErrorException($"{nameof(ShowPage)} Error: {result_e}");
            }

            //处理预先选择
            ProcessPreSelect();

            //设置控件可见
            SldControls.ForEach(p => p.SldControlVisibility = true);

            //AttachDocEvent();
        }

        private void ProcessPreSelect()
        {
            //获取选中对象
            var selMgr = _doc.ISelectionManager;

            var seleBoxes = GetSpecControl<SldSelectionBox>().ToArray();
            if (!seleBoxes.Any())
                return;

            for (int j = 0; j < seleBoxes.Length; j++)
            {
                var count = selMgr.GetSelectedObjectCount2(seleBoxes[j].Mark);
                if (count > 0)
                {
                    //实例化一个选择列表
                    List<swSeleTypeObjectPair> selections = new List<swSeleTypeObjectPair>(count);
                    for (int k = 1; k < count + 1; k++)
                    {
                        var selection = selMgr.GetSelectedObject6(k, seleBoxes[j].Mark);
                        var type = selMgr.GetSelectedObjectType3(k, seleBoxes[j].Mark);

                        //判断预选类型是否满足要求
                        if (!seleBoxes[j].SwSelectTypes.Any(p => (int)p == type))
                        {
                            selMgr.DeSelect2(k, seleBoxes[j].Mark);
                            continue;
                        }

                        var selePoint = (double[])selMgr.GetSelectionPoint2(k, seleBoxes[j].Mark);
                        string itemText = seleBoxes[j].SControl.ItemText[(short)k];

                        bool res = seleBoxes[j].OnSubmitSelectionCallout(seleBoxes[j].ID, selection, type, ref itemText);
                        if (res)
                        {
                            selections.Add(new swSeleTypeObjectPair(k, (swSelectType_e)type, seleBoxes[j].Mark, selection, itemText, selePoint));
                        }
                        else
                        {
                            selMgr.DeSelect2(k, seleBoxes[j].Mark);
                        }
                    }
                    if (selections.Any())
                    {
                        seleBoxes[j].PreSelectionCount = selections.Count;
                    }
                    seleBoxes[j].OnSelectionChanged(count, selections);
                    //OnSelectionboxListChanged(seleBox[j].ID, count);
                }
            }

        }

        /// <summary>
        /// ShowBubbleTooltipAt2
        /// </summary>
        /// <param name="title"></param>
        /// <param name="msg"></param>
        /// <param name="postion"></param>
        public void ShowBubbleTooltipAt2(string title, string msg, swArrowPosition postion = swArrowPosition.swArrowLeftTop)
        {
            App.ShowBubbleTooltipAt2(0, 0, (int)postion,
                        title, msg, (int)swBitMaps.swBitMapTreeError,
                        "", "", 0, (int)swLinkString.swLinkStringNone, "", "");
        }

        #endregion

        #region Protect Methods

        protected virtual void OnCreatePagePreview(IModelDoc2 _doc)
        {
            //获取选中对象
            var selMgr = _doc.ISelectionManager;

            var count = selMgr.GetSelectedObjectCount2(-1);
            if (count == 0)
                return;

            var seleBox = GetSpecControl<SldSelectionBox>().ToArray();
            if (!seleBox.Any())
                return;

            int i = 1;
            for (int j = 0; j < seleBox.Length; j++)
            {
                //放到单选框
                if (seleBox[j].SingleEntityOnly)
                {
                    bool res = selMgr.SetSelectedObjectMark(i++, seleBox[j].Mark, (int)swSelectionMarkAction_e.swSelectionMarkSet);
                }
                else
                {
                    //全部选中到第一个选中框
                    for (int k = 1; k < count + 1; k++)
                    {
                        bool res = selMgr.SetSelectedObjectMark(k, seleBox[j].Mark, (int)swSelectionMarkAction_e.swSelectionMarkSet);
                    }
                    return;
                }

                if (i > count + 1)
                {
                    break;
                }
            }

            //多余的选择,清楚掉
            for (int l = i; l < count + 1; l++)
            {
                selMgr.DeSelect(l);
            }
        }

        private IEnumerable<TControl> GetSpecControl<TControl>()
            where TControl : SldControl
        {
            foreach (var control in SldControls)
            {
                if (control is SldGroupBox groupBox)
                {
                    foreach (var item in groupBox.Children)
                    {
                        if (item is TControl specControl)
                        {
                            yield return specControl;
                        }
                    }
                }
                else if (control is TControl specControl)
                {
                    yield return specControl;
                }
            }
        }

        protected virtual void OnCreatePagePost() { }

        protected virtual void OnShowPagePreview() { }

        #endregion

        #region Private Methods

        //Create PMPage
        private void CreatePage()
        {
            OnCreatePagePreview(_doc);

            int errors = 0;
            Page = (PropertyManagerPage2)App.CreatePropertyManagerPage(PageTitle, GetOptions(), new PropertyManagerPage2Handler9Wrapper(this), ref errors);
            var errors_e = (swPropertyManagerPageStatus_e)errors;
            if (errors_e != swPropertyManagerPageStatus_e.swPropertyManagerPage_Okay)
            {
                throw new CreatePMPageErrorException($"{nameof(App.ICreatePropertyManagerPage)} Error: {errors_e.ToString()}");
            }

            OnCreatePagePost();
        }

        private int _idIndex=2;

        private void AddSldControls()
        {
            if (!SldControls.Any())
                return;

            foreach (var control in SldControls)
            {
                if (!control.IsPostionBack)
                {
                    this.AddVisualChild(control);
                    _idIndex = control.AddToPage(this, _idIndex);
                }
            }
        }

        private void AddSldBackControls()
        {
            if (!SldControls.Any())
                return;

            foreach (var control in SldControls)
            {
                if (control.IsPostionBack)
                {
                    this.AddVisualChild(control);
                    _idIndex = control.AddToPage(this, _idIndex);
                }
            }
        }

        /// <summary>
        /// add WinformHandleControl for Page
        /// </summary>
        private void AddUserControl()
        {
            int options = (int)swAddGroupBoxOptions_e.swGroupBoxOptions_Expanded |
                      (int)swAddGroupBoxOptions_e.swGroupBoxOptions_Visible;

            if (string.IsNullOrWhiteSpace(GroupTitle))
            {

                PMPageWinformHandle = (IPropertyManagerPageWindowFromHandle)Page.AddControl2(
                    _idIndex++,
                    (int)swPropertyManagerPageControlType_e.swControlType_WindowFromHandle,
                    "WPF Control",
                    (int)swPropertyManagerPageControlLeftAlign_e.swControlAlign_LeftEdge,
                    options,
                    "wpf control");

            }
            else
            {
                _wpfGroup = Page.AddGroupBox(
                    _idIndex++,
                    GroupTitle,
                    (int)(swAddGroupBoxOptions_e.swGroupBoxOptions_Expanded | swAddGroupBoxOptions_e.swGroupBoxOptions_Visible)) as IPropertyManagerPageGroup;

                _wpfGroup.Expanded = GroupExpanded;

                PMPageWinformHandle = (IPropertyManagerPageWindowFromHandle)_wpfGroup.AddControl2(
                    _idIndex++,
                    (int)swPropertyManagerPageControlType_e.swControlType_WindowFromHandle,
                    "WPF Control",
                    (int)swPropertyManagerPageControlLeftAlign_e.swControlAlign_LeftEdge,
                    options,
                    "wpf control");
            }

            PMPageWinformHandle.Height = PageHeight;
        }

        #endregion

        #region Properties

        public IPropertyManagerPage2 Page { get; private set; }

        public swPropertyManagerPageCloseReasons_e CloseReason { get; private set; }

        public IPropertyManagerPageWindowFromHandle PMPageWinformHandle { get; private set; }

        //public int ID => 1;

        public virtual swPropertyManagerPageOptions_e[] PageOptions { get; set; }

        public List<SldControl> SldControls { get; set; } = new List<SldControl>();

        #endregion

        #region DependencyProperty

        public ISldWorks App { get; set; }

        ///// <summary>
        ///// <see cref="ISldWorks"/>
        ///// </summary>
        //public static DependencyProperty AppProperty = DependencyProperty.Register(nameof(App), typeof(ISldWorks), typeof(SldPMPage), new PropertyMetadata(default));

        /// <summary>
        /// PMPage Title
        /// </summary>
        public string PageTitle { get; set; }

        /// <summary>
        /// 将wpf控件放到一个Group里，如果此值为null，则直接添加到页面里面
        /// </summary>
        public string GroupTitle { get; set; }

        /// <summary>
        /// 将wpf控件放到一个Group里，指示这个Group是否展开
        /// </summary>
        public bool GroupExpanded { get; set; } = true;

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

        public ICommand CloseCommand
        {
            get { return (ICommand)GetValue(CloseComamdProperty); }
            set { SetValue(CloseComamdProperty, value); }
        }

        // Using a DependencyProperty as the backing store for CloseComamd.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CloseComamdProperty =
            DependencyProperty.Register("CloseCommand", typeof(ICommand), typeof(SldPMPage), new PropertyMetadata(null));



        public SldControl FocusSldControl
        {
            get { return (SldControl)GetValue(FocusSldControlProperty); }
            set { SetValue(FocusSldControlProperty, value); }
        }

        // Using a DependencyProperty as the backing store for FouceSldControl.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty FocusSldControlProperty =
            DependencyProperty.Register("FocusSldControl", typeof(SldControl), typeof(SldPMPage), new FrameworkPropertyMetadata(null,FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

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
            int IndentSize;
            IndentSize = Debug.IndentSize;
            Debug.WriteLine(IndentSize);

            CloseReason = (swPropertyManagerPageCloseReasons_e)Reason;

            var arg = new ClosingArg();
            Closing?.Invoke(CloseReason, arg);

            if (arg.Cancel)
            {
                if (!string.IsNullOrEmpty(arg.ErrorTitle) || !string.IsNullOrEmpty(arg.ErrorMessage))
                {
                    var title = !string.IsNullOrEmpty(arg.ErrorTitle) ? arg.ErrorTitle : "Error";

                    ShowBubbleTooltipAt2( title, arg.ErrorMessage);
                }

                throw new COMException(arg.ErrorMessage, S_FALSE);
            }

            //执行Command接口
            if (CloseReason == swPropertyManagerPageCloseReasons_e.swPropertyManagerPageClose_Okay)
            {
                bool canExecute = CloseCommand == null ? true : CloseCommand.CanExecute(null);

                if (!canExecute)
                {
                    if (CloseCommand is CloseCommand clsCmd)
                    {
                        ShowBubbleTooltipAt2(clsCmd.ErrorTitle, clsCmd.BubbleTooltip);
                    }
                    else
                    {
                        ShowBubbleTooltipAt2("无法执行", "未知错误");
                    }
                    const int S_FALSE = 1;
                    throw new COMException(arg.ErrorMessage, S_FALSE);
                }
            }
            //DeAttach Doc Selection Event
            //_selectionBoxs.Clear();
            //DeAttachDocEvent();
        }

        public void AfterClose()
        {
            int IndentSize;
            IndentSize = System.Diagnostics.Debug.IndentSize;
            System.Diagnostics.Debug.WriteLine(IndentSize);

            if (CloseReason == swPropertyManagerPageCloseReasons_e.swPropertyManagerPageClose_Okay)
            {
                OkClicked?.Invoke();
                CloseCommand?.Execute(null);
            }
            else
            {
                CloseCommand?.Execute(-1);
            }
            Closed?.Invoke(CloseReason);

            SldControls.ForEach(p => p.SldControlVisibility = false);

            this.Dispose();
        }

        public virtual bool OnHelp()
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
            var sldCheckBox = GetControlById<SldCheckBox>(Id);
            sldCheckBox?.OnUserCheckChanged(Checked);
        }

        public void OnOptionCheck(int Id)
        {
            var sldOption = GetControlById<SldOption>(Id);
            if (sldOption.Checked != sldOption.SControl.Checked)
            {
                sldOption.OnCheckedUpdate();
            }
        }

        public void OnButtonPress(int Id)
        {
            var sldBtn = GetControlById<SldBitmapButton>(Id) as ISldBtnCommand ?? 
                         GetControlById<SldButton>(Id) as ISldBtnCommand;            
            
                sldBtn?.Command?.Execute(null);
        }

        public void OnTextboxChanged(int Id, string Text)
        {
            var control = GetControlById<SldTextBox>(Id);
            if (control != null)
            {
                control.Text = Text;
            }
        }

        public void OnNumberboxChanged(int Id, double Value)
        {
            var sldNumberBox = GetControlById<SldNumberBox>(Id);
            if (sldNumberBox != null)
            {
                sldNumberBox.OnValueUpdate(Value);
            }
        }

        public void OnComboboxEditChanged(int Id, string Text)
        {
            var control = GetControlById<SldCombobox>(Id);
            if (control != null)
            {
                control.OnEditTextUpdate(Text);
            }
        }

        public void OnComboboxSelectionChanged(int Id, int Item)
        {
            //User Selcet it
            var sControl = GetControlById<SldCombobox>(Id);
            sControl.OnItemSelected(Item);
        }

        public void OnListboxSelectionChanged(int Id, int Item)
        {
        }

        public void OnSelectionboxFocusChanged(int Id)
        {
        }

        public void OnSelectionboxListChanged(int Id, int Count)
        {
            //获取当前id代表的选择框（SelectionBox）
            SldSelectionBox sldSelectionBox = GetSelectionBoxById(Id);

            //没有找到匹配的选择框 则返回
            if (sldSelectionBox == null)
                return;

            //当前清楚选择，直接设置为0
            if (Count == 0)
            {
                sldSelectionBox.OnSelectionChanged(Count, null);
                return;
            }

            //获取选择管理器
            var seleMgr = _doc.ISelectionManager;

            //实例化一个选择列表
            List<swSeleTypeObjectPair> selections = new List<swSeleTypeObjectPair>(Count);

            //获取选择，更新列表
            for (int i = 1; i < Count + 1; i++)
            {
                var seleType = seleMgr.GetSelectedObjectType3(i, sldSelectionBox.Mark);
                var seleObj = seleMgr.GetSelectedObject6(i, sldSelectionBox.Mark);
                var selePoint = (double[])seleMgr.GetSelectionPoint2(i, sldSelectionBox.Mark);

                //所有从0开始
                var name = sldSelectionBox.SControl.ItemText[(short)(i-1)];

                //预先选择数量之下
                if (i <= sldSelectionBox.PreSelectionCount
                    && sldSelectionBox.Selections?.Count > i-1)
                {
                    //选择的是相同对象，取相同名称
                    if (App.IsSame(seleObj,sldSelectionBox.Selections[i-1].SelectedObject) == (int)swObjectEquality.swObjectSame)
                    {
                        name = sldSelectionBox.Selections[i - 1].Name;
                    }
                }

                selections.Add(new swSeleTypeObjectPair(i, (swSelectType_e)seleType, sldSelectionBox.Mark, seleObj,name, selePoint));
            }

            sldSelectionBox.OnSelectionChanged(Count, selections);

            //如果是单选框 - 切换到下一个焦点
            if (sldSelectionBox.SingleEntityOnly)
            {
                bool isCurrent = false;
                SldSelectionBox next= default;
                foreach (var sldSeleItem in GetSpecControl<SldSelectionBox>())
                {
                    if (isCurrent)
                    {
                        next = sldSeleItem;
                        break;
                    }
                    if (sldSeleItem.ID == sldSelectionBox.ID)
                        isCurrent = true;
                }
                //存在下一个选择框且下一个选择框不为0，则切换焦点
                if(next != null &&
                   next.SldControlIsVisible() && 
                   next.AutoMoveFocusToThis)
                {
                    next.SControl.SetSelectionFocus();
                }
            }
        }

        private SldSelectionBox GetSelectionBoxById(int id)
        {
            return GetControlById<SldSelectionBox>(id);
        }

        private TControl GetControlById<TControl>(int id)
            where TControl:SldControl
        {
            var sldControl = default(TControl);
            foreach (var control in SldControls)
            {
                if (control.ID == id)
                {
                    sldControl = control as TControl;
                    break;
                }
                if (control is SldGroupBox groupBox)
                {
                    sldControl = groupBox.Children.
                        OfType<TControl>().
                        FirstOrDefault(p => p.ID == id);
                }
                if (sldControl != null)
                    break;
            }

            return sldControl;
        }

        private SldControl GetControlById(int id)
        {
            var sldControl = default(SldControl);
            foreach (var control in SldControls)
            {
                if (control.ID == id)
                {
                    sldControl = control;
                    break;
                }
                if (control is SldGroupBox groupBox)
                {
                    sldControl = groupBox.Children.
                        FirstOrDefault(p => p.ID == id);
                }
                if (sldControl != null)
                    break;
            }

            return sldControl;
        }

        public void OnSelectionboxCalloutCreated(int Id)
        {
        }

        public void OnSelectionboxCalloutDestroyed(int Id)
        {
        }

        public bool OnSubmitSelection(int Id, object Selection, int SelType, ref string ItemText)
        {
            var sldSeleBox = GetSelectionBoxById(Id);
            if (sldSeleBox == null)
            {
                return true;
            }
            else
            {
                return sldSeleBox.OnSubmitSelectionCallout(Id, Selection, SelType,ref ItemText);
            }
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
            var control = GetControlById(Id);
            if (control != null)
            {
                FocusSldControl = control;
            }
        }

        public void OnLostFocus(int Id)
        {

        }

        private bool _hasRetry = false;
        public int OnWindowFromHandleControlCreated(int Id, bool Status)
        {
            if (Status)
            {
                return -1;
            }
            else
            {
                if (_hasRetry)
                {
                    return (int)swHandleWindowFromHandleCreationFailure_e.swHandleWindowFromHandleCreationFailure_Cancel;
                }
                else
                {
                    _hasRetry = true;
                    return (int)swHandleWindowFromHandleCreationFailure_e.swHandleWindowFromHandleCreationFailure_Retry;
                }
            }
        }

        public void OnListboxRMBUp(int Id, int PosX, int PosY)
        {
        }

        public void OnNumberBoxTrackingCompleted(int Id, double Value)
        {
            //数字框失去焦点时产生变化
            var sldNumberBox = GetControlById<SldNumberBox>(Id);
            if (sldNumberBox != null)
            {
                sldNumberBox.OnValueUpdate(Value);
            }
        }

        #endregion

        #region IDisposable Support
        protected bool disposedValue = false; // 要检测冗余调用

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    DisposePage();
                }

                // 释放未托管的资源(未托管的对象)并在以下内容中替代终结器。
                // TODO: 将大型字段设置为 null。

                disposedValue = true;
            }
        }

        protected void DisposePage()
        {
            // 释放托管状态(托管对象)。
            if (_host != null)
            {
                _host.Parent = null;
                _host.Child = null;
                _host.Dispose();
            }

            Marshal.FinalReleaseComObject(Page);
            Marshal.FinalReleaseComObject(PMPageWinformHandle);

            Page = null;
            PMPageWinformHandle = null;
            _wpfGroup = null;
            _doc = null;
            App = null;
        }

        // TODO: 仅当以上 Dispose(bool disposing) 拥有用于释放未托管资源的代码时才替代终结器。
        // ~SldPMPage()
        // {
        //   // 请勿更改此代码。将清理代码放入以上 Dispose(bool disposing) 中。
        //   Dispose(false);
        // }

        // 添加此代码以正确实现可处置模式。
        public void Dispose()
        {
            // 请勿更改此代码。将清理代码放入以上 Dispose(bool disposing) 中。
            Dispose(true);
            // TODO: 如果在以上内容中替代了终结器，则取消注释以下行。
            //GC.SuppressFinalize(this);
        }
        #endregion

    }
}
