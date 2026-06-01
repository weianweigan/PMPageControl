// Created on 2020/6/7. Hosts a WPF UserControl within a SolidWorks TaskPane
// via an ElementHost for Windows Forms/WPF interop.

namespace Du.PMPage.Wpf
{
    using System;
    using System.Runtime.InteropServices;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Forms.Integration;
    using SolidWorks.Interop.sldworks;

    /// <summary>
    /// A WPF <see cref="UserControl"/> that hosts its visual tree inside a native
    /// SolidWorks TaskPane view.  Uses an <see cref="ElementHost"/> to embed WPF
    /// content into the native TaskPane HWND.
    /// </summary>
    /// <remarks>
    /// <para><b>Two creation paths.</b>
    /// The TaskPane tab icon can be supplied either as a legacy bitmap resource
    /// string (<see cref="TaskPaneBitmap"/>) or as a set of multi-resolution image
    /// files (<see cref="ImageList"/>).  When <see cref="ImageList"/> is set,
    /// <c>ISldWorks.CreateTaskpaneView3</c> is called (supports 20×20 through
    /// 128×128 pixel sizes for high-DPI displays); otherwise
    /// <c>ISldWorks.CreateTaskpaneView2</c> is called with
    /// <see cref="TaskPaneBitmap"/>.</para>
    ///
    /// <para><b>Threading.</b>
    /// SolidWorks is a Single-Threaded Apartment (STA) host.  The
    /// <see cref="ElementHost"/> must be created on the UI thread — it is
    /// instantiated lazily inside <see cref="ShowView"/> rather than at
    /// construction time to avoid cross-thread initialization issues.</para>
    ///
    /// <para><b>Lifecycle.</b>
    /// <list type="number">
    /// <item>Construct the <see cref="SldTaskPane"/> (optionally pass
    /// <see cref="ISldWorks"/> and initial content).</item>
    /// <item>Set <see cref="TaskPaneToolTip"/> and
    /// <see cref="TaskPaneBitmap"/> / <see cref="ImageList"/>.</item>
    /// <item>Call <see cref="ShowView"/> (or <see cref="ShowTaskpane"/>).
    /// This creates the native <c>ITaskpaneView</c>, wires the
    /// <see cref="ElementHost"/>, and makes the pane visible.</item>
    /// <item>Override <see cref="OnTaskPaneCreated"/> to perform
    /// post-creation initialization.</item>
    /// <item>Override <see cref="OnTaskPaneClosing"/> to perform
    /// pre-close cleanup.</item>
    /// <item>Call <see cref="CloseTaskpane"/> or <see cref="IDisposable.Dispose"/>
    /// to tear down the native view and host.</item>
    /// </list></para>
    ///
    /// <para><b>Content swapping.</b>
    /// Call <see cref="SetContent"/> to replace the hosted WPF visual tree at
    /// runtime without destroying the native TaskPane view.</para>
    /// </remarks>
    public class SldTaskPane : UserControl, IDisposable
    {
        #region Fields

        /// <summary>Typed reference to the SolidWorks application.</summary>
        private SldWorks _app;

        /// <summary>
        /// Backing field for <see cref="EleHost"/>.  Created lazily to ensure
        /// STA-thread affinity.
        /// </summary>
        private ElementHost _eleHost;

        private bool _disposed;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="SldTaskPane"/> class
        /// with a reference to the SolidWorks application.
        /// </summary>
        /// <param name="app">
        /// The <see cref="ISldWorks"/> SolidWorks application instance. Must not be <see langword="null"/>.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="app"/> is <see langword="null"/>.
        /// </exception>
        public SldTaskPane(ISldWorks app)
        {
            _app = (SldWorks)(app ?? throw new ArgumentNullException(nameof(app)));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SldTaskPane"/> class
        /// with a SolidWorks application reference and initial WPF content.
        /// </summary>
        /// <param name="app">
        /// The <see cref="ISldWorks"/> SolidWorks application instance. Must not be <see langword="null"/>.
        /// </param>
        /// <param name="content">
        /// The initial WPF <see cref="UIElement"/> to display inside the TaskPane.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="app"/> is <see langword="null"/>.
        /// </exception>
        public SldTaskPane(ISldWorks app, UIElement content)
            : this(app)
        {
            if (content != null)
            {
                Content = content;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SldTaskPane"/> class.
        /// The <see cref="App"/> property must be set before calling <see cref="ShowView"/>.
        /// </summary>
        /// <remarks>
        /// This constructor is provided for XAML designer support and
        /// code-behind scenarios where the SW application reference
        /// is obtained after construction.
        /// </remarks>
        public SldTaskPane() { }

        #endregion

        #region Dependency Properties

        /// <summary>
        /// Gets or sets the SolidWorks application instance.
        /// Setting this property also updates the internal typed reference
        /// used for native API calls.
        /// </summary>
        public ISldWorks App
        {
            get => (ISldWorks)GetValue(AppProperty);
            set
            {
                SetValue(AppProperty, value);
                if (value != null)
                {
                    _app = (SldWorks)value;
                }
            }
        }

        /// <summary>
        /// Identifies the <see cref="App"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty AppProperty = DependencyProperty.Register(
            nameof(App),
            typeof(ISldWorks),
            typeof(SldTaskPane),
            new PropertyMetadata(null, OnAppChanged)
        );

        /// <summary>
        /// Called when <see cref="AppProperty"/> changes.  Keeps the internal
        /// typed <see cref="SldWorks"/> field in sync.
        /// </summary>
        private static void OnAppChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var pane = (SldTaskPane)d;
            if (e.NewValue is SldWorks sw)
            {
                pane._app = sw;
            }
            else if (e.NewValue is ISldWorks isw)
            {
                pane._app = (SldWorks)isw;
            }
        }

        /// <summary>
        /// Gets or sets the tooltip text displayed for the TaskPane tab
        /// in the SolidWorks Task Pane.
        /// </summary>
        public string TaskPaneToolTip
        {
            get => (string)GetValue(TaskPaneToolTipProperty);
            set => SetValue(TaskPaneToolTipProperty, value);
        }

        /// <summary>
        /// Identifies the <see cref="TaskPaneToolTip"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty TaskPaneToolTipProperty =
            DependencyProperty.Register(
                nameof(TaskPaneToolTip),
                typeof(string),
                typeof(SldTaskPane),
                new PropertyMetadata(string.Empty)
            );

        /// <summary>
        /// Gets or sets the bitmap identifier string for the TaskPane tab icon.
        /// Used when <see cref="ImageList"/> is not set.
        /// </summary>
        /// <remarks>
        /// This is the legacy single-resolution bitmap path.  When both this
        /// property and <see cref="ImageList"/> are set, <see cref="ImageList"/>
        /// takes precedence and <c>CreateTaskpaneView3</c> is used.
        /// </remarks>
        public string TaskPaneBitmap
        {
            get => (string)GetValue(TaskPaneBitmapProperty);
            set => SetValue(TaskPaneBitmapProperty, value);
        }

        /// <summary>
        /// Identifies the <see cref="TaskPaneBitmap"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty TaskPaneBitmapProperty =
            DependencyProperty.Register(
                nameof(TaskPaneBitmap),
                typeof(string),
                typeof(SldTaskPane),
                new PropertyMetadata(string.Empty)
            );

        #endregion

        #region CLR Properties

        public bool UseSystemFont { get; set; } = true;

        /// <summary>
        /// Gets the native SolidWorks TaskPane view (<see cref="ITaskpaneView"/>).
        /// Created internally by <see cref="ShowView"/> on first call.
        /// Returns <see langword="null"/> if the TaskPane has not been shown yet.
        /// </summary>
        public ITaskpaneView TaskpaneView { get; private set; }

        /// <summary>
        /// Gets the <see cref="ElementHost"/> used to host WPF content inside
        /// the native SolidWorks TaskPane window.  Created lazily when
        /// <see cref="ShowView"/> is first called.
        /// </summary>
        /// <remarks>
        /// Must only be accessed from the UI (STA) thread.
        /// </remarks>
        public ElementHost EleHost
        {
            get
            {
                if (_eleHost == null)
                {
                    _eleHost = new ElementHost() { Margin = new System.Windows.Forms.Padding(0) };
                    if (UseSystemFont)
                    {
                        _eleHost.Font = System.Drawing.SystemFonts.MessageBoxFont;
                    }
                }
                return _eleHost;
            }
        }

        /// <summary>
        /// Gets or sets an array of file-system paths to icon images for the
        /// TaskPane tab, supporting multiple resolutions for high-DPI displays.
        /// </summary>
        /// <remarks>
        /// <para>Supported pixel sizes: 20×20, 32×32, 40×40, 64×64, 96×96,
        /// 128×128.  Images should use a 256-color palette.  Use gray
        /// (RGB 192, 192, 192) for transparent areas.</para>
        /// <para>When set, <c>ISldWorks.CreateTaskpaneView3</c> is used
        /// (multi-res); otherwise <c>ISldWorks.CreateTaskpaneView2</c> is
        /// used with <see cref="TaskPaneBitmap"/>.</para>
        /// </remarks>
        public string[] ImageList { get; set; }

        /// <summary>
        /// Gets the native window handle (<c>HWND</c>) of the TaskPane,
        /// or <see cref="IntPtr.Zero"/> if the TaskPane has not been created.
        /// </summary>
        public IntPtr Handle => EleHost?.Handle ?? IntPtr.Zero;

        /// <summary>
        ///  Get a value indicating whether the TaskPane is currently visible and active in the SolidWorks UI.
        /// </summary>
        public bool IsActive => TaskpaneView?.IsActiveTab() ?? false;

        #endregion

        #region Public Methods

        /// <summary>
        /// Creates and shows the application-level tab on the SolidWorks Task Pane.
        /// On first call this creates the native <see cref="ITaskpaneView"/>,
        /// wires the <see cref="ElementHost"/>, and calls
        /// <see cref="OnTaskPaneCreated"/>.
        /// </summary>
        /// <returns>
        /// <see langword="true"/> if the Task Pane view is visible after the call;
        /// <see langword="false"/> otherwise.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown when <see cref="App"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="CreateTaskPaneErrorException">
        /// Thrown when the native TaskPane view could not be created.
        /// </exception>
        public bool ShowView()
        {
            if (_app == null && App == null)
            {
                throw new InvalidOperationException(
                    "Cannot show TaskPane: the App (ISldWorks) property is null. "
                        + "Set App before calling ShowView()."
                );
            }

            if (TaskpaneView == null)
            {
                AddTaskPane();
                OnTaskPaneCreated();
            }

            if (EleHost.Child == null)
            {
                AddWPFHostControl();
            }

            return TaskpaneView.ShowView();
        }

        /// <summary>
        /// Hides the application-level tab on the Task Pane.
        /// Detaches the WPF content from the <see cref="ElementHost"/> before hiding.
        /// </summary>
        /// <returns>
        /// <see langword="true"/> if the application-level tab is hidden;
        /// <see langword="false"/> if the Task Pane view is <see langword="null"/>.
        /// </returns>
        public bool HideView()
        {
            DeBindToEleHost();
            return TaskpaneView?.HideView() ?? false;
        }

        /// <summary>
        /// Shows and activates the TaskPane.  Equivalent to calling
        /// <see cref="ShowView"/>.
        /// </summary>
        public void ShowTaskpane()
        {
            ShowView();
        }

        /// <summary>
        /// Replaces the WPF content hosted inside the TaskPane without
        /// destroying the native view.  If the TaskPane has already been
        /// created, the new content is applied immediately; otherwise it
        /// is stored and applied when <see cref="ShowView"/> is called.
        /// </summary>
        /// <param name="ui">The WPF <see cref="UIElement"/> to display.</param>
        /// <remarks>
        /// By default, the <see cref="SldTaskPane"/> itself is the
        /// hosted content (<c>this</c>).  Use this method to swap in
        /// a different visual tree at runtime.
        /// </remarks>
        public void SetContent(UIElement ui)
        {
            if (TaskpaneView != null && EleHost != null)
            {
                var wasShown = TaskpaneView.ShowView();
                DeBindToEleHost();
                AddUIElement(ui);
                if (wasShown)
                {
                    TaskpaneView.ShowView();
                }
            }
            else
            {
                AddUIElement(ui);
            }
        }

        /// <summary>
        /// Closes the TaskPane and releases all managed and unmanaged resources,
        /// including the native <see cref="ITaskpaneView"/> and the
        /// <see cref="ElementHost"/>.
        /// </summary>
        public void CloseTaskpane()
        {
            Dispose();
        }

        #endregion

        #region Protected Virtual Methods (Lifecycle Hooks)

        /// <summary>
        /// Called after the native <see cref="ITaskpaneView"/> has been
        /// successfully created (inside <see cref="ShowView"/>).
        /// Override to perform post-creation initialization such as
        /// configuring the TaskPane view or loading initial data.
        /// </summary>
        /// <remarks>
        /// The base implementation does nothing.  At this point
        /// <see cref="TaskpaneView"/> is guaranteed to be non-<see langword="null"/>.
        /// </remarks>
        protected virtual void OnTaskPaneCreated() { }

        /// <summary>
        /// Called before the native <see cref="ITaskpaneView"/> is destroyed
        /// (from <see cref="Dispose(bool)"/>).  Override to perform custom
        /// cleanup such as saving state or releasing derived-class resources.
        /// </summary>
        /// <remarks>
        /// The base implementation does nothing.  At this point
        /// <see cref="TaskpaneView"/> is still valid.
        /// </remarks>
        protected virtual void OnTaskPaneClosing() { }

        #endregion

        #region Private / Internal Methods

        /// <summary>
        /// Creates the native SolidWorks TaskPane view using either
        /// <see cref="ImageList"/> (multi-res via <c>CreateTaskpaneView3</c>)
        /// or <see cref="TaskPaneBitmap"/> (legacy via <c>CreateTaskpaneView2</c>).
        /// </summary>
        /// <exception cref="CreateTaskPaneErrorException">
        /// Thrown when the native TaskPane view creation returns <see langword="null"/>.
        /// </exception>
        private void AddTaskPane()
        {
            if (_app == null)
            {
                throw new InvalidOperationException(
                    "Cannot create TaskPane view: the internal SldWorks reference is null."
                );
            }

            if (ImageList != null && ImageList.Length > 0)
            {
                TaskpaneView = _app.CreateTaskpaneView3(ImageList, TaskPaneToolTip);
            }
            else
            {
                TaskpaneView = _app.CreateTaskpaneView2(TaskPaneBitmap, TaskPaneToolTip);
            }

            if (TaskpaneView == null)
            {
                throw new CreateTaskPaneErrorException(
                    "Failed to create the native SolidWorks TaskPane view. "
                        + "Ensure the bitmap resource name is valid and the SW version supports TaskPane creation."
                );
            }
        }

        /// <summary>
        /// Hosts WPF content inside the native TaskPane view.
        /// Re-binds <see cref="EleHost"/>'s child if already set,
        /// then calls <c>ITaskpaneView.DisplayWindowFromHandlex64</c>.
        /// </summary>
        private void AddWPFHostControl()
        {
            if (EleHost.Child != null)
            {
                DeBindToEleHost();
            }

            // Default — host ourself (the UserControl's visual tree).
            AddUIElement(this);

            TaskpaneView.DisplayWindowFromHandlex64(EleHost.Handle.ToInt64());
        }

        /// <summary>
        /// Detaches the WPF child from the <see cref="ElementHost"/>,
        /// clearing its child and any hosted Windows Forms controls.
        /// </summary>
        private void DeBindToEleHost()
        {
            if (_eleHost == null)
            {
                return;
            }

            if (!_eleHost.IsDisposed)
            {
                _eleHost.Controls.Clear();
                _eleHost.Child = null;
            }
        }

        /// <summary>
        /// Sets the WPF <see cref="UIElement"/> as the child of the
        /// <see cref="ElementHost"/>.
        /// </summary>
        /// <param name="ui">The WPF element to host. Must not be <see langword="null"/>.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="ui"/> is <see langword="null"/>.
        /// </exception>
        protected void AddUIElement(UIElement ui)
        {
            if (ui == null)
            {
                throw new ArgumentNullException(nameof(ui));
            }

            EleHost.Child = ui;
        }

        #endregion

        #region IDisposable

        /// <summary>
        /// Releases all resources used by the <see cref="SldTaskPane"/>.
        /// </summary>
        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases the unmanaged resources used by the <see cref="SldTaskPane"/>
        /// and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing">
        /// <see langword="true"/> to release both managed and unmanaged resources;
        /// <see langword="false"/> to release only unmanaged resources.
        /// </param>
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                // Give derived classes a chance to clean up while
                // TaskpaneView is still valid.
                OnTaskPaneClosing();

                if (TaskpaneView != null)
                {
                    // Hide first to ensure clean UI state before delete.
                    TaskpaneView.HideView();
                    TaskpaneView.DeleteView();
                    Marshal.FinalReleaseComObject(TaskpaneView);
                    TaskpaneView = null;
                }

                if (_eleHost != null && !_eleHost.IsDisposed)
                {
                    _eleHost.Controls.Clear();
                    _eleHost.Child = null;
                    _eleHost.Dispose();
                    _eleHost = null;
                }

                _disposed = true;
            }
        }

        #endregion
    }
}
