/*
 * Created By WeiGan NJ.Touch.Ltd 2012/6/25
 * 1.适用于带有选择框的wpf窗口基类
 */

using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace Du.PMPage.Wpf
{
    public class SldWindow : Window
    {
        private readonly SldWorks _app;
        protected readonly ModelDoc2 _doc;
        private readonly SelectionMgr _seleMgr;
        private PartDoc _partDoc;

        private List<SldWpfSelectionList> _selectionList;

        private const int S_OK = 0x00000000;
        private const int S_FALSE = 0x00000001;

        #region Ctor

        public SldWindow()
        {

        }

        public SldWindow(ISldWorks app)
        {
            _app = (SldWorks)app;
            _doc = _app.IActiveDoc2;
            _seleMgr = _doc.ISelectionManager;

            AttachEvent();
        }
        #endregion

        #region Properties

        public List<swSelectType_e> AllowSelectTypes { get; } = new List<swSelectType_e>();

        public List<SldWpfSelectionList> SelectionList
        {
            get => _selectionList; set
            {
                _selectionList = value;
                if (_selectionList != null)
                {
                    foreach (var item in _selectionList)
                    {
                        AllowSelectTypes.AddRange(item.SwSelectTypes);
                        item.Actived += Item_Actived; ;
                    }
                }
            }
        }

        private void Item_Actived(object obj)
        {
            //其他都失去焦点
            foreach (var item in _selectionList)
            {
                if (item != obj)
                {
                    item.IsActive = false;
                }
            }
        }

        #endregion

        #region Private Methods

        private void AttachEvent()
        {
            this.Closed += SldWindow_Closed;
            //_app.ActiveDocChangeNotify += _app_ActiveDocChangeNotify;

            switch ((swDocumentTypes_e)_doc.GetType())
            {
                case swDocumentTypes_e.swDocPART:
                    _partDoc = (PartDoc)_doc;
                    _partDoc.UserSelectionPostNotify += _partDoc_UserSelectionPostNotify;
                   
                    break;
            }
        }

        private int _partDoc_NewSelectionNotify()
        {
            throw new NotImplementedException();
        }

        private int _partDoc_UserSelectionPostNotify()
        {
            //获取最后一个选择对象
            var count = _seleMgr.GetSelectedObjectCount2(-1);

            var mark = _seleMgr.GetSelectedObjectMark(count);
            var type = _seleMgr.GetSelectedObjectType3(count, -1);
            var obj = _seleMgr.GetSelectedObject6(count, -1);
            var postion = _seleMgr.GetSelectionPoint2(count, -1) as double[];

            if (AllowSelectTypes.Any(p => (int)p == type))
            {
                return OnUserSelected(count, mark, (swSelectType_e)type, string.Empty, obj, postion) ? 0x00000000 : 0x00000001;
            }

            return S_OK;
        }

        protected virtual bool OnUserSelected(int count, int mark, swSelectType_e type, string name,object obj, double[] postion)
        {
            return true;
        }

        protected void DeAttachEvent()
        {
            this.Closed -= SldWindow_Closed;
            //_app.ActiveDocChangeNotify -= _app_ActiveDocChangeNotify;

            if (_selectionList != null)
            {
                foreach (var item in _selectionList)
                {
                    item.Actived -= Item_Actived;
                }
            }

            //取消选择时间订阅
            switch ((swDocumentTypes_e)_doc.GetType())
            {
                case swDocumentTypes_e.swDocPART:
                    _partDoc.UserSelectionPostNotify -= _partDoc_UserSelectionPostNotify;
                    break;
            }
        }

        protected virtual void SldWindow_Closed(object sender, EventArgs e)
        {
            DeAttachEvent();
        }

        #endregion

    }
}
