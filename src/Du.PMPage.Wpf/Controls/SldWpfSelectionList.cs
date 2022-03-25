using SolidWorks.Interop.swconst;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;

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
    ///     <MyNamespace:SldWpfSelectionList/>
    ///
    /// </summary>
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
