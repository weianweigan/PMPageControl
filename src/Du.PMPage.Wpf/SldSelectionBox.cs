using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;

namespace Du.PMPage.Wpf
{
    public class SldSelectionBox : SldControl<IPropertyManagerPageSelectionbox>
    {
        public delegate bool OnSubmitSelection(int Id, object Selection, int SelType, ref string ItemText);

        public event OnSubmitSelection OnSubmitSelectionNotify;

        #region Dependecy Properties

        ///// <summary>
        ///// 是否单选，默认为false
        ///// </summary>
        public bool SingleEntityOnly
        {
            get { return (bool)GetValue(SingleEntityOnlyProperty); }
            set { SetValue(SingleEntityOnlyProperty, value); }
        }

        public static readonly DependencyProperty SingleEntityOnlyProperty =
            DependencyProperty.Register("SingleEntityOnly", typeof(bool), typeof(SldSelectionBox), new PropertyMetadata(false, OnSingleEntityChanged));

        private static void OnSingleEntityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var box = d as SldSelectionBox;
            box.SingleEntityOnly = (bool)e.NewValue;
            box.OnSingleEntityChanged((bool)e.OldValue, (bool)e.NewValue);
        }

        private void OnSingleEntityChanged(bool oldValue, bool newValue)
        {
            if (oldValue != newValue
                && SControl != null)
            {
                SControl.SingleEntityOnly = newValue;
            }
        }

        /// <summary>
        /// 可以选择的类型
        /// </summary>
        public List<swSelectType_e> SwSelectTypes
        {
            get { return (List<swSelectType_e>)GetValue(SwSelectTypesProperty); }
            set { SetValue(SwSelectTypesProperty, value); }
        }

        public static readonly DependencyProperty SwSelectTypesProperty =
            DependencyProperty.Register("SwSelectTypes", typeof(List<swSelectType_e>), typeof(SldSelectionBox), new PropertyMetadata(null,OnSwSelectTypesChanged));

        private static void OnSwSelectTypesChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var box = d as SldSelectionBox;
            box.OnSwSelectTypesChanged((List<swSelectType_e>)e.OldValue, (List<swSelectType_e>)e.NewValue);
        }

        private void OnSwSelectTypesChanged(List<swSelectType_e> oldValue, List<swSelectType_e> newValue)
        {
            if (oldValue != newValue &&
                SControl != null)
            {
                if (SldControlVisibility)
                {
                    throw new InvalidOperationException($"{nameof(PropertyManagerPageSelectionbox)}.{nameof(PropertyManagerPageSelectionbox.SetSelectionFilters)} Cannot be Set after Displayed");
                }
                SControl.SetSelectionFilters(newValue.ToArray());
            }
        }

        /// <summary>
        /// Selection Mark of SelectedObject in this SelectionBox
        /// </summary>
        public int Mark
        {
            get { return (int)GetValue(MarkProperty); }
            set { SetValue(MarkProperty, value); }
        }

        public static readonly DependencyProperty MarkProperty =
            DependencyProperty.Register("Mark", typeof(int), typeof(SldSelectionBox), new PropertyMetadata(-1, OnMarkChanged));

        private static void OnMarkChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var box = d as SldSelectionBox;
            box.Mark = (int)e.NewValue;
            box.OnMarkChanged((int)e.OldValue, (int)e.NewValue);
        }

        private void OnMarkChanged(int oldValue, int newValue)
        {
            if (newValue != 1 && newValue % 2 != 0)
            {
                throw new ArgumentException($"SelectionBox.Mark Cannot be {newValue},It must be powers of two (for example, 1, 2, 4, 8).");
            }
            if (oldValue != newValue &&
                SControl != null)
            {
                SControl.Mark = newValue;
            }
        }

        /// <summary>
        /// Gets or sets whether the same entity can be selected multiple times in this selection box. 
        /// </summary>
        public bool AllowMultipleSelectOfSameEntity
        {
            get { return (bool)GetValue(AllowMultipleSelectOfSameEntityProperty); }
            set { SetValue(AllowMultipleSelectOfSameEntityProperty, value); }
        }

        public static readonly DependencyProperty AllowMultipleSelectOfSameEntityProperty =
            DependencyProperty.Register("AllowMultipleSelectOfSameEntity", typeof(bool), typeof(SldSelectionBox), new PropertyMetadata(false, OnAllowMutipleSelectOfSameEntityPropertyChanged));

        private static void OnAllowMutipleSelectOfSameEntityPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var box = d as SldSelectionBox;
            box.AllowMultipleSelectOfSameEntity = (bool)e.NewValue;
            box.OnAllowMutipleSelectOfSameEntityChanged((bool)e.OldValue, (bool)e.NewValue);
        }

        private void OnAllowMutipleSelectOfSameEntityChanged(bool oldValue, bool newValue)
        {
            if (oldValue != newValue
                && SControl != null)
            {
                if (SldControlVisibility)
                {
                    throw new InvalidOperationException($"{nameof(PropertyManagerPageSelectionbox)}.{nameof(PropertyManagerPageSelectionbox.AllowMultipleSelectOfSameEntity)} Cannot be Set after Displayed");
                }
                else
                {
                    SControl.AllowMultipleSelectOfSameEntity = newValue;
                }
            }
        }

        /// <summary>
        /// Gets or sets whether an entity can be selected in this selection box if the entity is selected elsewhere. 
        /// </summary>
        public bool AllowSelectInMultipleBoxes
        {
            get { return (bool)GetValue(AllowSelectInMultipleBoxesProperty); }
            set { SetValue(AllowSelectInMultipleBoxesProperty, value); }
        }

        public static readonly DependencyProperty AllowSelectInMultipleBoxesProperty =
            DependencyProperty.Register("AllowSelectInMultipleBoxes", typeof(bool), typeof(SldSelectionBox), new PropertyMetadata(false, OnAllowSelectInMultipleBoxesProperty));

        private static void OnAllowSelectInMultipleBoxesProperty(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var box = d as SldSelectionBox;
            box.AllowSelectInMultipleBoxes = (bool)e.NewValue;
            box.OnAllowSelectInMultipleBoxes((bool)e.OldValue, (bool)e.NewValue);
        }

        private void OnAllowSelectInMultipleBoxes(bool oldValue, bool newValue)
        {
            if (oldValue != newValue
                && SControl != null)
            {
                SControl.AllowSelectInMultipleBoxes = oldValue;
                throw new InvalidOperationException($"{nameof(PropertyManagerPageSelectionbox)}.{nameof(PropertyManagerPageSelectionbox.AllowSelectInMultipleBoxes)} Cannot be Set after Displayed");
            }
        }

        public ObservableCollection<swSeleTypeObjectPair> Selections
        {
            get { return (ObservableCollection<swSeleTypeObjectPair>)GetValue(SelectionsProperty); }
            set { SetValue(SelectionsProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Selections.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SelectionsProperty =
            DependencyProperty.Register("Selections", typeof(ObservableCollection<swSeleTypeObjectPair>), typeof(SldSelectionBox), new PropertyMetadata(null,OnSelectionsChanged));

        private static void OnSelectionsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var box = d as SldSelectionBox;
            box.Selections = e.NewValue as ObservableCollection<swSeleTypeObjectPair>;
            box.OnSelectionChanged((ObservableCollection<swSeleTypeObjectPair>)e.OldValue,(ObservableCollection<swSeleTypeObjectPair>)e.NewValue);
        }

        private void OnSelectionChanged(ObservableCollection<swSeleTypeObjectPair> oldValue, ObservableCollection<swSeleTypeObjectPair> newValue)
        {
            if (oldValue != null)
            {
                oldValue.CollectionChanged -= Selections_CollectionChanged;
            }
            if (newValue != null)
            {
                newValue.CollectionChanged += Selections_CollectionChanged;
            }
        }

        private void Selections_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            //内部增加
            if (_innerAddSelection)
            {
                return;
            }

            //TODO: 外部增加选择
            
        }

        public int SldHeight
        {
            get { return (int)GetValue(SldHeightProperty); }
            set { SetValue(SldHeightProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SldHeight.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SldHeightProperty =
            DependencyProperty.Register("SldHeight", typeof(int), typeof(SldSelectionBox), new PropertyMetadata(14, OnSldHeightChanged));

        private static void OnSldHeightChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var sldSeletionBox = d as SldSelectionBox;
            sldSeletionBox.OnHeightChanged((int)e.OldValue, (int)e.NewValue);
        }

        private void OnHeightChanged(int oldValue, int newValue)
        {
            if (oldValue !=  newValue &&
                SControl != null)
            {
                SControl.Height = (short)newValue;
            }
        }

        public int CurrentSelection
        {
            get => SControl?.CurrentSelection ?? -1; set
            {
                if (SControl != null)
                    SControl.CurrentSelection = value;
            } 
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets a multi-row, editable callout for this selection box. 
        /// </summary>
        public Callout Callout
        {
            get => SControl?.Callout; set
            {
                if (SControl != null)
                    SControl.Callout = value;
            }
        }

        /// <summary>
        /// 预先选择的数量
        /// </summary>
        internal int PreSelectionCount { get; set; } = -1;

        /// <summary>
        /// 当此选择框是单选时起作用，决定是否自动将焦点移动到下一个选择框
        /// </summary>
        public bool AutoMoveFocusToThis { get; set; } = true;

        /// <summary>
        /// 设置选择框颜色
        /// </summary>
        /// <remarks>
        /// <see cref="swUserPreferenceIntegerValue_e.swSystemColorsSelectedItem1"/> = 104 第一种颜色，默认
        /// <see cref="swUserPreferenceIntegerValue_e.swSystemColorsSelectedItem2"/> = 105 第二种颜色
        /// <see cref="swUserPreferenceIntegerValue_e.swSystemColorsSelectedItem3"/> = 106 第三种颜色
        /// </remarks>
        public int? SldSystemColorValue { get; set; }

        #endregion

        #region Methods

        protected override void SetSldControl()
        {
            if (SwSelectTypes != null && SwSelectTypes.Any())
            {
                SControl.SetSelectionFilters(SwSelectTypes.ToArray());
            }

            //设置属性 -- 是否选择单个和标记
            SControl.SingleEntityOnly = SingleEntityOnly;
            SControl.Mark = Mark;
            SControl.Height = (short)SldHeight;
            SControl.AllowSelectInMultipleBoxes = AllowSelectInMultipleBoxes;
            SControl.AllowMultipleSelectOfSameEntity = AllowMultipleSelectOfSameEntity;

            //设置颜色
            if (SldSystemColorValue != null)
            {
                SControl.SetSelectionColor(true, SldSystemColorValue.Value);
            }
        }

        private bool _innerAddSelection = false;

        internal void OnSelectionChanged(int count, List<swSeleTypeObjectPair> swSeleTypeObjectPairs)
        {
            if (Selections == null)
                return;

            _innerAddSelection = true;

            //重置
            if (count == 0 /*&& Selections.Count != 0*/)
            {
                Selections.Clear();
            }
            else
            {
                //去除多余的
                for (int i = 0; i < Selections.Count; i++)
                {
                    var pair = swSeleTypeObjectPairs.FirstOrDefault(p => p.Name == Selections[i].Name);
                    if (pair == null)
                    {
                        Selections.RemoveAt(i);
                        --i;
                    }
                }

                //添加新增加的
                for (int i = 0; i < swSeleTypeObjectPairs.Count; i++)
                {
                    var pair = Selections.FirstOrDefault(p => p.Name == swSeleTypeObjectPairs[i].Name);
                    if (pair == null)
                    {
                        Selections.Add(swSeleTypeObjectPairs[i]);
                    }
                }

                //if(Selections.Count != 0)
                //    Selections.Clear();

                //swSeleTypeObjectPairs.ForEach(p => Selections.Add(p));
            }
            _innerAddSelection = false;
        }

        internal bool OnSubmitSelectionCallout(int id,object selection,int selType,ref string itemText)
        {
            return OnSubmitSelectionNotify?.Invoke(id, selection, selType,ref itemText) ?? true;
        }

        #endregion
    }

    /// <summary>
    /// 选择类型和选择对象的键值对
    /// </summary>
    public class swSeleTypeObjectPair:IEqualityComparer<swSeleTypeObjectPair>
    {
        public swSeleTypeObjectPair(int index, swSelectType_e selectType,int mark, object selectedObject, string name, double[] point)
        {
            SelectType = selectType;
            Mark = mark;
            Index = index;
            Point = point;
            SelectedObject = selectedObject;
            Name = name;
        }

        /// <summary>
        /// 序号
        /// </summary>
        public int Index { get; private set; }
        
        /// <summary>
        /// 标记
        /// </summary>
        public int Mark { get; private set; }

        /// <summary>
        /// 选择类型
        /// </summary>
        public swSelectType_e SelectType { get; private set; }

        /// <summary>
        /// 选择对象
        /// </summary>
        public object SelectedObject { get; private set; }

        /// <summary>
        /// 在列表中显示的名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 用来追踪用的标记
        /// </summary>
        public object Tag { get; set; }

        /// <summary>
        /// 用户选择的点
        /// </summary>
        public double[] Point { get; set; }

        /// <summary>
        /// 用来临时存储PID的属性，默认为null
        /// </summary>
        public string PID { get; set; }

        public swPersistReferencedObjectStates_e ReSolveFormPID(IModelDoc2 doc)
        {
            if (string.IsNullOrEmpty(PID))
            {
                throw new ArgumentNullException("请先对属性 PID 赋值");
            }

            var byteId = Convert.FromBase64String(PID);

            SelectedObject = doc.Extension.GetObjectByPersistReference3(byteId, out int errorCode);
            return (swPersistReferencedObjectStates_e)errorCode;
        }

        public bool Equals(swSeleTypeObjectPair x, swSeleTypeObjectPair y)
        {
            return x?.Name == y?.Name;
        }

        public int GetHashCode(swSeleTypeObjectPair obj)
        {
            return obj?.Name.GetHashCode() ?? -1;
        }

        /// <summary>
        /// 在边面顶点的时候，为了保存对象的持久引用，防止引发 <see cref="System.Runtime.InteropServices.COMException"/>被调用的对象已与其客户端断开连接
        /// </summary>
        public void GetSafeEntity()
        {
            if (SelectType == swSelectType_e.swSelEDGES 
                || SelectType == swSelectType_e.swSelFACES 
                || SelectType == swSelectType_e.swSelVERTICES)
            {
                var entity = SelectedObject as IEntity;
                if (!entity.IsSafe)
                    SelectedObject = entity.GetSafeEntity();
            }
        }

        public override string ToString()
        {
            return Name ?? base.ToString();
        }
    }
}
