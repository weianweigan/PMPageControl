/*
 
   1.创建于 2020/6/7，用于显示Taskpane页。
   托管了一ElementHost 来显示一个WPF的用户控件
 
 */


namespace Du.PMPage.Wpf
{
    using SolidWorks.Interop.sldworks;
    using System;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Forms.Integration;

    /// <summary>
    /// TaskPane in WPF
    /// </summary>
    public class SldTaskPane : UserControl,IDisposable
    {

        #region ctor

        public SldTaskPane(ISldWorks app)
        {
            App = app ?? throw new ArgumentNullException(nameof(app));
        }

        public SldTaskPane()
        {
        }

        #endregion

        #region DependencyProperties

        public ISldWorks App
        {
            get { return (ISldWorks)GetValue(AppProperty); }
            set { SetValue(AppProperty, value); }
        }

        // Using a DependencyProperty as the backing store for App.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty AppProperty =
            DependencyProperty.Register("App", typeof(ISldWorks), typeof(SldTaskPane), new PropertyMetadata(null));

        /// <summary>
        /// ToolTip for TaskPane
        /// </summary>
        public string TaskPaneToolTip
        {
            get { return (string)GetValue(TaskPaneToolTipProperty); }
            set { SetValue(TaskPaneToolTipProperty, value); }
        }

        // Using a DependencyProperty as the backing store for TaskPaneToolTip.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TaskPaneToolTipProperty =
            DependencyProperty.Register("TaskPaneToolTip", typeof(string), typeof(SldTaskPane), new PropertyMetadata(""));

        /// <summary>
        /// BitMap string
        /// </summary>
        public string TaskPaneBitmap
        {
            get { return (string)GetValue(TaskPaneBitmapProperty); }
            set { SetValue(TaskPaneBitmapProperty, value); }
        }

        // Using a DependencyProperty as the backing store for TaskPaneBitmap.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TaskPaneBitmapProperty =
            DependencyProperty.Register("TaskPaneBitmap", typeof(string), typeof(SldTaskPane), new PropertyMetadata(""));

        #endregion

        #region Properties

        /// <summary>
        /// Taskpane view <see cref="ITaskpaneView"/>
        /// </summary>
        public ITaskpaneView TaskpaneView { get; private set; }

        /// <summary>
        /// ElementHost which host a wpf usercontrol
        /// </summary>
        public ElementHost EleHost { get; private set; } = new ElementHost();

        /// <summary>
        /// Array of strings of paths to the image files for the tab of this Task Pane view (see Remarks)
        /// </summary>
        /// <remarks>
        /// This method supports high resolution screens with high resolution operating system scaling options.  ImageList contains paths to images with the following pixel sizes:
        /// 20 X 20 
        /// 32 X 32 
        /// 40 X 40 
        /// 64 X 64 
        /// 96 X 96 
        /// 128 X 128 The images should use a 256-color palette.Use gray (RGB 192, 192, 192) for transparent areas in your graphic.
        /// </remarks>
        public string[] ImageList { get; set; }

        #endregion

        #region Private Methods

        private void AddTaskPane()
        {
            if (App == null)
            {
                throw new NullReferenceException($"Cannot CreateTaskpaneView The Property App(ISldWorks) is Null");
            }

            if (ImageList != null)
            {
                TaskpaneView = App.CreateTaskpaneView3(ImageList, TaskPaneToolTip);
            }
            else
            {
                TaskpaneView = App.CreateTaskpaneView2(TaskPaneBitmap, TaskPaneToolTip);
            }

            if (TaskpaneView == null)
            {
                throw new CreateTaskPaneErrorException();
            }
        }

        private void AddWPFHostControl()
        {
            if (EleHost.Child != null)
            {
                DeBindToEleHost();
            }

            AddUIElement(this);

            TaskpaneView.DisplayWindowFromHandlex64(EleHost.Handle.ToInt64());
        }

        private void DeBindToEleHost()
        {
            EleHost.Controls.Clear();
            EleHost.Child = null;
        }

        private void AddUIElement(UIElement ui)
        {
            EleHost.Child = ui;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Hides the application-level tab on the Task Pane. 
        /// </summary>
        /// <returns>True if the application-level tab is hidden, false if not</returns>
        public bool HideView()
        {
            DeBindToEleHost();
            return TaskpaneView.HideView();
        }

        /// <summary>
        /// Activates the application-level tab of the Task Pane view and makes the view visible. 
        /// </summary>
        /// <returns>True if application-level tab of the Task Pane view is visible, false if not</returns>
        public bool ShowView()
        {
            if (EleHost.Child == null)
            {
                AddWPFHostControl();
            }
            return TaskpaneView.ShowView();
        }

        /// <summary>
        /// Show and activated
        /// </summary>
        public void ShowTaskpane()
        {
            ShowView();
        }

        /// <summary>
        /// Closed and the taskpane will be disposed
        /// </summary>
        public void CloseTaskpane()
        {
            this.Dispose();
        }

        public void Dispose()
        {
            TaskpaneView?.DeleteView();

            if (!this.EleHost.IsDisposed)
            {
                EleHost.Controls.Clear();
                EleHost.Child = null;
                EleHost.Dispose();
            }
        }

        #endregion
    }
}
