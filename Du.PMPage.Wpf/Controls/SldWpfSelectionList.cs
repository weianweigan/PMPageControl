using SolidWorks.Interop.swconst;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;

namespace Du.PMPage.Wpf
{
    public class SldWpfSelectionList : ListBox
    {
        static SldWpfSelectionList()
        {
            //DefaultStyleKeyProperty.OverrideMetadata(typeof(SldWpfSelectionList), new FrameworkPropertyMetadata(typeof(SldWpfSelectionList)));
        }

        public event Action<object> Actived;

        public bool IsActive
        {
            get { return (bool)GetValue(IsActiveProperty); }
            set { SetValue(IsActiveProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsActive.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsActiveProperty =
            DependencyProperty.Register("IsActive", typeof(bool), typeof(SldWpfSelectionList), new PropertyMetadata(false,OnIsActivePropertyChanged));

        private static void OnIsActivePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var list = d as SldWpfSelectionList;
        }

        protected override void OnMouseUp(MouseButtonEventArgs e)
        {
            IsActive = true;
            Actived?.Invoke(this);
            base.OnMouseUp(e);
        }

        public int Mark { get; set; } = 0;

        /// <summary>
        /// 可以选择的类型
        /// </summary>
        public List<swSelectType_e> SwSelectTypes
        {
            get { return (List<swSelectType_e>)GetValue(SwSelectTypesProperty); }
            set { SetValue(SwSelectTypesProperty, value); }
        }

        public static readonly DependencyProperty SwSelectTypesProperty =
            DependencyProperty.Register("SwSelectTypes", typeof(List<swSelectType_e>), typeof(SldWpfSelectionList), new PropertyMetadata(null));

    }
}
