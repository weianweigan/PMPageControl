using SolidWorks.Interop.sldworks;
using System;
using System.Collections.ObjectModel;

namespace Du.PMPage.Wpf
{
    /// <summary>
    /// Test / demo window for <see cref="SldWindow"/> that simulates
    /// <see cref="Controls.SldSelectionBox"/> behaviour in a standalone
    /// WPF window. Contains three <see cref="WpfSelectionList"/> instances
    /// with different <see cref="SolidWorks.Interop.swconst.swSelectType_e"/>
    /// filters (faces, edges, multi-type).
    /// </summary>
    /// <remarks>
    /// <para>Usage from an add-in:</para>
    /// <code>
    /// var win = new SldSelectionTestWindow(app);
    /// win.Show();
    /// </code>
    /// <para>
    /// Or with explicit owner:
    /// <code>
    /// var win = new SldSelectionTestWindow(app) { Owner = mainWindow };
    /// win.ShowDialog();
    /// </code>
    /// </para>
    /// </remarks>
    public partial class SldSelectionTestWindow : SldWindow
    {
        private readonly ObservableCollection<SwSeleTypeObjectPair> _faceSelections = new();
        private readonly ObservableCollection<SwSeleTypeObjectPair> _edgeSelections = new();
        private readonly ObservableCollection<SwSeleTypeObjectPair> _multiTypeSelections = new();

        /// <summary>
        /// Parameterless constructor for XAML designer support.
        /// </summary>
        public SldSelectionTestWindow()
        {
            InitializeComponent();
            InitializeCollections();
        }

        /// <summary>
        /// Creates the test window connected to a running SolidWorks instance.
        /// </summary>
        /// <param name="app">The SolidWorks application object.</param>
        public SldSelectionTestWindow(ISldWorks app)
            : base(app)
        {
            InitializeComponent();
            InitializeCollections();
            WireUpSelectionLists();
        }

        private void InitializeCollections()
        {
            // Bind collections to the selection lists
            FaceList.Selections = _faceSelections;
            EdgeList.Selections = _edgeSelections;
            MultiTypeList.Selections = _multiTypeSelections;

            // Update status when collections change
            _faceSelections.CollectionChanged += (s, e) => UpdateStatus();
            _edgeSelections.CollectionChanged += (s, e) => UpdateStatus();
            _multiTypeSelections.CollectionChanged += (s, e) => UpdateStatus();
        }

        private void WireUpSelectionLists()
        {
            // Register all lists with the base SldWindow
            SelectionList = new()
            {
                FaceList,
                EdgeList,
                MultiTypeList,
            };

            // Update status bar when activation changes
            FaceList.Activated += OnAnyListActivated;
            EdgeList.Activated += OnAnyListActivated;
            MultiTypeList.Activated += OnAnyListActivated;
        }

        private void OnAnyListActivated(object list)
        {
            UpdateStatus();
        }

        private void UpdateStatus()
        {
            var active = ActiveSelectionList;
            ActiveListName.Text = active?.Name ?? "(none)";

            int total = _faceSelections.Count + _edgeSelections.Count + _multiTypeSelections.Count;
            TotalCount.Text = total.ToString();
        }
    }
}
