using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;

namespace Du.PMPage.Wpf
{
    /// <summary>
    /// Base class for WPF windows that integrate with SolidWorks selection.
    /// Manages one or more <see cref="WpfSelectionList"/> instances — each
    /// analogous to a native <c>SldSelectionBox</c> — and routes user
    /// selections from the graphics area into the currently active list.
    /// </summary>
    /// <remarks>
    /// <para><b>Selection-list isolation.</b>
    /// On construction the window calls
    /// <see cref="ISelectionMgr.SuspendSelectionList"/> to preserve the
    /// current SolidWorks selection context and start a fresh one.
    /// When the window closes, <see cref="ISelectionMgr.ResumeSelectionList"/>
    /// restores the original context.  This prevents the window's selections
    /// from interfering with the normal SolidWorks selection state.</para>
    ///
    /// <para><b>Multiple lists.</b>
    /// A window can host any number of <see cref="WpfSelectionList"/> controls.
    /// At most one list is <see cref="ActiveSelectionList">active</see> at a
    /// time (the one last clicked).  Selections are validated against the
    /// active list's <see cref="WpfSelectionList.SwSelectTypes"/> and are
    /// added to its <see cref="WpfSelectionList.Selections"/> collection.
    /// Each list should have a unique, power-of-two
    /// <see cref="WpfSelectionList.Mark"/> so that selections can be
    /// distinguished in the native <see cref="ISelectionMgr"/>.</para>
    /// </remarks>
    public class SldWindow : Window, IDisposable
    {
        private readonly SldWorks _app;

        /// <summary>The active SolidWorks document.</summary>
        protected readonly ModelDoc2 _doc;

        /// <summary>
        /// The native SolidWorks selection manager for the active document.
        /// Available to derived classes for advanced selection operations.
        /// </summary>
        protected SelectionMgr SelectionMgr => _seleMgr;
        private readonly SelectionMgr _seleMgr;

        private PartDoc _partDoc;
        private AssemblyDoc _assemblyDoc;
        private DrawingDoc _drawingDoc;

        private List<WpfSelectionList> _selectionList;
        private bool _selectionSuspended;

        private const int S_OK = 0x00000000;
        private const int S_FALSE = 0x00000001;

        #region Ctor

        /// <summary>
        /// Parameterless constructor for XAML designer support.
        /// Does not attach SolidWorks events; use the overload that accepts
        /// <see cref="ISldWorks"/> for runtime usage.
        /// </summary>
        public SldWindow()
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SldWindow"/> class,
        /// caches the active document, obtains the selection manager,
        /// subscribes to selection events, and suspends the native
        /// SolidWorks selection list to isolate the window's selections.
        /// </summary>
        /// <param name="app">The SolidWorks application object.</param>
        /// <exception cref="InvalidOperationException">
        /// Thrown when no document is open in SolidWorks.
        /// </exception>
        public SldWindow(ISldWorks app)
        {
            _app = (SldWorks)app;
            _doc = _app.IActiveDoc2;
            if (_doc == null)
                throw new InvalidOperationException(
                    "No active document found. A document must be open to create SldWindow.");
            _seleMgr = _doc.ISelectionManager;

            AttachEvent();
            SuspendNativeSelection();
        }
        #endregion

        #region Properties

        /// <summary>
        /// Gets the allowed SolidWorks selection types for this window.
        /// Populated automatically from all <see cref="SelectionList"/> items.
        /// This is the union of all lists' <see cref="WpfSelectionList.SwSelectTypes"/>.
        /// Used as the fallback filter when no list is active.
        /// </summary>
        public List<swSelectType_e> AllowSelectTypes { get; } = new List<swSelectType_e>();

        /// <summary>
        /// Gets the currently active <see cref="WpfSelectionList"/>,
        /// i.e. the one the user last clicked. Selections are routed
        /// to this list. Returns <see langword="null"/> when no list
        /// is active.
        /// </summary>
        public WpfSelectionList ActiveSelectionList =>
            _selectionList?.FirstOrDefault(l => l.IsActive);

        /// <summary>
        /// Gets or sets the list of <see cref="WpfSelectionList"/> instances
        /// bound to this window. When set, each item's
        /// <see cref="WpfSelectionList.SwSelectTypes"/> are merged into
        /// <see cref="AllowSelectTypes"/> and the <c>Activated</c> event is
        /// wired to manage exclusive activation across lists.
        /// </summary>
        /// <remarks>
        /// <para>Each list should have a unique <see cref="WpfSelectionList.Mark"/>
        /// (power of two) so that selections belonging to different lists can
        /// be distinguished in the native <see cref="ISelectionMgr"/>.</para>
        /// </remarks>
        public List<WpfSelectionList> SelectionList
        {
            get => _selectionList; set
            {
                _selectionList = value;
                if (_selectionList != null)
                {
                    foreach (var item in _selectionList)
                    {
                        if (item.SwSelectTypes != null)
                            AllowSelectTypes.AddRange(item.SwSelectTypes);
                        item.Activated += Item_Activated;
                    }
                }
            }
        }

        /// <summary>
        /// Handles activation of a single <see cref="WpfSelectionList"/>,
        /// deactivating all others so that only one list receives selections
        /// at a time.
        /// </summary>
        /// <param name="obj">The newly activated list.</param>
        private void Item_Activated(object obj)
        {
            foreach (var item in _selectionList)
            {
                if (item != obj)
                    item.IsActive = false;
            }
        }

        #endregion

        #region Native Selection List Lifecycle

        /// <summary>
        /// Suspends the current SolidWorks selection list, preserving its
        /// contents and starting a fresh selection context for this window.
        /// Idempotent — subsequent calls are no-ops until
        /// <see cref="ResumeNativeSelection"/> is called.
        /// </summary>
        /// <returns><see langword="true"/> if the suspend succeeded;
        /// <see langword="false"/> if it was already suspended or unavailable.</returns>
        protected bool SuspendNativeSelection()
        {
            if (_seleMgr == null || _selectionSuspended)
                return false;

            _seleMgr.SuspendSelectionList();
            _selectionSuspended = true;
            return true;
        }

        /// <summary>
        /// Resumes the original SolidWorks selection list that was suspended
        /// by <see cref="SuspendNativeSelection"/>.  Idempotent.
        /// </summary>
        /// <returns><see langword="true"/> if the resume succeeded;
        /// <see langword="false"/> if it was not suspended or unavailable.</returns>
        protected bool ResumeNativeSelection()
        {
            if (_seleMgr == null || !_selectionSuspended)
                return false;

            _seleMgr.ResumeSelectionList();
            _selectionSuspended = false;
            return true;
        }

        /// <summary>
        /// Adds an object to the native suspended selection list with the
        /// specified <paramref name="mark"/>.  Useful when a derived class
        /// needs to programmatically add selections to a specific list's
        /// native mark-set.
        /// </summary>
        /// <param name="obj">The SolidWorks object to add.</param>
        /// <param name="mark">The mark (from <see cref="WpfSelectionList.Mark"/>)
        /// to assign to the selection.</param>
        /// <returns><see langword="true"/> if the object was added successfully.</returns>
        protected bool AddToNativeSelectionList(object obj, int mark)
        {
            if (_seleMgr == null || obj == null)
                return false;

            SelectData selectData = _seleMgr.CreateSelectData();
            selectData.Mark = mark;
            return _seleMgr.AddSelectionListObject(obj, selectData);
        }

        #endregion

        #region Selection Handling

        /// <summary>
        /// Attaches to document-specific selection events based on document type.
        /// </summary>
        private void AttachEvent()
        {
            this.Closed += SldWindow_Closed;

            switch ((swDocumentTypes_e)_doc.GetType())
            {
                case swDocumentTypes_e.swDocPART:
                    _partDoc = (PartDoc)_doc;
                    _partDoc.UserSelectionPostNotify += OnUserSelectionPostNotify;
                    break;

                case swDocumentTypes_e.swDocASSEMBLY:
                    _assemblyDoc = (AssemblyDoc)_doc;
                    _assemblyDoc.UserSelectionPostNotify += OnUserSelectionPostNotify;
                    break;

                case swDocumentTypes_e.swDocDRAWING:
                    _drawingDoc = (DrawingDoc)_doc;
                    _drawingDoc.UserSelectionPostNotify += OnUserSelectionPostNotify;
                    break;
            }
        }

        /// <summary>
        /// Unified selection post-notify handler for all document types.
        /// Retrieves the most recent selection, filters by the active
        /// <see cref="WpfSelectionList"/>'s allowed types, and routes
        /// to <see cref="OnUserSelected(WpfSelectionList, int, int, swSelectType_e, string, object, double[])"/>.
        /// </summary>
        /// <returns>S_OK to accept the selection; S_FALSE to reject it.</returns>
        private int OnUserSelectionPostNotify()
        {
            var count = _seleMgr.GetSelectedObjectCount2(-1);

            if (count <= 0)
                return S_OK;

            var rawMark = _seleMgr.GetSelectedObjectMark(count);
            var type = _seleMgr.GetSelectedObjectType3(count, -1);
            var obj = _seleMgr.GetSelectedObject6(count, -1);
            var postion = _seleMgr.GetSelectionPoint2(count, -1) as double[];

            var swType = (swSelectType_e)type;
            var displayName = GetSelectionDisplayName(swType, obj, count);

            var activeList = ActiveSelectionList;

            // ── Route to active list ────────────────────────────
            if (activeList != null)
            {
                if (activeList.SwSelectTypes != null &&
                    activeList.SwSelectTypes.Any(p => (int)p == type))
                {
                    // Use the list's own Mark, not the raw event mark.
                    return OnUserSelected(
                        activeList,
                        count,
                        activeList.Mark,
                        swType,
                        displayName,
                        obj,
                        postion)
                        ? S_OK
                        : S_FALSE;
                }
                // Active list exists but type doesn't match — reject.
                return S_FALSE;
            }

            // ── No active list: fall back to AllowSelectTypes union ─
            if (AllowSelectTypes.Any(p => (int)p == type))
            {
                return OnUserSelected(null, count, rawMark, swType, displayName, obj, postion)
                    ? S_OK
                    : S_FALSE;
            }

            return S_OK;
        }

        /// <summary>
        /// Resolves a human-readable display name for the selected entity.
        /// The default implementation combines the selection-type name with
        /// the ordinal <paramref name="count"/> (e.g. "Face 3").
        /// Override to provide custom naming (e.g. query the entity for
        /// a feature name).
        /// </summary>
        /// <param name="type">The SolidWorks selection type.</param>
        /// <param name="obj">The selected COM object.</param>
        /// <param name="count">The selection ordinal.</param>
        /// <returns>A display name for the selection list.</returns>
        protected virtual string GetSelectionDisplayName(swSelectType_e type, object obj, int count)
        {
            return $"{type} {count}";
        }

        #endregion

        #region Virtual Methods (Overridable Hooks)

        /// <summary>
        /// Called when the user selects an object in the SolidWorks graphics area
        /// whose type matches the active <see cref="WpfSelectionList"/>'s
        /// <see cref="WpfSelectionList.SwSelectTypes"/>.
        /// </summary>
        /// <param name="list">
        /// The active <see cref="WpfSelectionList"/> that should receive the selection,
        /// or <see langword="null"/> when no list is active.
        /// </param>
        /// <param name="count">The selection count (index of this pick).</param>
        /// <param name="mark">
        /// The selection mark — when <paramref name="list"/> is provided this is the
        /// list's <see cref="WpfSelectionList.Mark"/>; otherwise the raw event mark.
        /// </param>
        /// <param name="type">The SolidWorks selection type.</param>
        /// <param name="name">The name of the selected object (may be empty).</param>
        /// <param name="obj">The selected COM object.</param>
        /// <param name="postion">The selection point coordinates.</param>
        /// <returns>
        /// <see langword="true"/> to allow the selection to proceed;
        /// <see langword="false"/> to reject it (S_FALSE is returned to SW).
        /// </returns>
        /// <remarks>
        /// <para>
        /// The base implementation:
        /// <list type="number">
        /// <item>Adds the selection to <paramref name="list"/>'s
        /// <see cref="WpfSelectionList.Selections"/> collection via
        /// <see cref="WpfSelectionList.AddSelection(SwSeleTypeObjectPair)"/>.</item>
        /// <item>Adds the object to the native suspended selection list via
        /// <see cref="AddToNativeSelectionList"/> so that the
        /// <see cref="ISelectionMgr"/> is kept in sync with the list's mark.</item>
        /// </list>
        /// Override to customize selection handling (e.g. validation or
        /// transformation).  Return <see langword="false"/> to reject the
        /// selection.
        /// </para>
        /// </remarks>
        protected virtual bool OnUserSelected(
            WpfSelectionList list,
            int count,
            int mark,
            swSelectType_e type,
            string name,
            object obj,
            double[] postion)
        {
            if (list != null)
            {
                var pair = new SwSeleTypeObjectPair(count, type, mark, obj, name, postion);

                // Keep the native suspended selection list in sync.
                AddToNativeSelectionList(obj, list.Mark);

                list.AddSelection(pair);
            }
            return true;
        }

        /// <summary>
        /// [Obsolete] Called when the user selects an object whose type matches
        /// <see cref="AllowSelectTypes"/> but no <see cref="WpfSelectionList"/>
        /// is active. Prefer overriding
        /// <see cref="OnUserSelected(WpfSelectionList, int, int, swSelectType_e, string, object, double[])"/>.
        /// </summary>
        /// <returns>
        /// <see langword="true"/> to allow the selection;
        /// <see langword="false"/> to reject it.
        /// </returns>
        [Obsolete("Prefer overriding the overload that accepts a WpfSelectionList parameter.")]
        protected virtual bool OnUserSelected(int count, int mark, swSelectType_e type, string name, object obj, double[] postion)
        {
            return true;
        }

        #endregion

        #region Cleanup

        /// <summary>
        /// Detaches all SolidWorks event handlers and WPF event subscriptions.
        /// Called from <see cref="SldWindow_Closed"/> and <see cref="Dispose()"/>.
        /// Safe to call multiple times.
        /// </summary>
        protected void DeAttachEvent()
        {
            this.Closed -= SldWindow_Closed;

            if (_selectionList != null)
            {
                foreach (var item in _selectionList)
                {
                    item.Activated -= Item_Activated;
                }
            }

            // Detach selection events per document type
            if (_doc != null)
            {
                switch ((swDocumentTypes_e)_doc.GetType())
                {
                    case swDocumentTypes_e.swDocPART:
                        if (_partDoc != null)
                            _partDoc.UserSelectionPostNotify -= OnUserSelectionPostNotify;
                        break;

                    case swDocumentTypes_e.swDocASSEMBLY:
                        if (_assemblyDoc != null)
                            _assemblyDoc.UserSelectionPostNotify -= OnUserSelectionPostNotify;
                        break;

                    case swDocumentTypes_e.swDocDRAWING:
                        if (_drawingDoc != null)
                            _drawingDoc.UserSelectionPostNotify -= OnUserSelectionPostNotify;
                        break;
                }
            }
        }

        /// <summary>
        /// Handles the WPF window closed event.
        /// Resumes the native SolidWorks selection list, then detaches all
        /// event handlers.  Override to add custom cleanup — call the base
        /// implementation to ensure proper teardown.
        /// </summary>
        protected virtual void SldWindow_Closed(object sender, EventArgs e)
        {
            ResumeNativeSelection();
            DeAttachEvent();
        }

        /// <summary>
        /// Releases all COM references, resumes the native selection list,
        /// and detaches event handlers.  Call when the window is no longer needed.
        /// </summary>
        public void Dispose()
        {
            ResumeNativeSelection();
            DeAttachEvent();
            if (_partDoc != null) { Marshal.FinalReleaseComObject(_partDoc); _partDoc = null; }
            if (_assemblyDoc != null) { Marshal.FinalReleaseComObject(_assemblyDoc); _assemblyDoc = null; }
            if (_drawingDoc != null) { Marshal.FinalReleaseComObject(_drawingDoc); _drawingDoc = null; }
            if (_seleMgr != null) { Marshal.FinalReleaseComObject(_seleMgr); }
            if (_doc != null) { Marshal.FinalReleaseComObject(_doc); }
        }

        #endregion

    }
}
