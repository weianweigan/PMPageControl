using SolidWorks.Interop.swconst;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
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
    ///     <MyNamespace:SldSelctionBox/>
    ///
    /// </summary>
    public class SldSelectionBox : ListBox
    {
        static SldSelectionBox()
        {
           // DefaultStyleKeyProperty.OverrideMetadata(typeof(SldSelectionBox), new FrameworkPropertyMetadata(typeof(SldSelectionBox)));
        }

        #region Dependecy Properties

        ///// <summary>
        ///// 单选框
        ///// </summary>
        public bool SingleEntityOnly
        {
            get { return (bool)GetValue(SingleEntityOnlyProperty); }
            set { SetValue(SingleEntityOnlyProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SingleEntityOnly.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SingleEntityOnlyProperty =
            DependencyProperty.Register("SingleEntityOnly", typeof(bool), typeof(SldSelectionBox), new PropertyMetadata(false));

        /// <summary>
        /// 可以选择的类型
        /// </summary>
        public swSelectType_e[] SwSelectTypes
        {
            get { return (swSelectType_e[])GetValue(SwSelectTypesProperty); }
            set { SetValue(SwSelectTypesProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SwSelectTypes.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SwSelectTypesProperty =
            DependencyProperty.Register("SwSelectTypes", typeof(swSelectType_e[]), typeof(SldSelectionBox), new PropertyMetadata(null));

        /// <summary>
        /// Selection Mark of SelectedObject in this SelectionBox
        /// </summary>
        public int Mark
        {
            get { return (int)GetValue(MarkProperty); }
            set { SetValue(MarkProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Mark.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MarkProperty =
            DependencyProperty.Register("Mark", typeof(int), typeof(SldSelectionBox), new PropertyMetadata(0));

        /// <summary>
        /// Gets or sets whether the same entity can be selected multiple times in this selection box. 
        /// </summary>
        public bool AllowMultipleSelectOfSameEntity
        {
            get { return (bool)GetValue(AllowMultipleSelectOfSameEntityProperty); }
            set { SetValue(AllowMultipleSelectOfSameEntityProperty, value); }
        }

        public static readonly DependencyProperty AllowMultipleSelectOfSameEntityProperty =
            DependencyProperty.Register("AllowMultipleSelectOfSameEntity", typeof(bool), typeof(SldSelectionBox), new PropertyMetadata(false));

        /// <summary>
        /// Gets or sets whether an entity can be selected in this selection box if the entity is selected elsewhere. 
        /// </summary>
        public bool AllowSelectInMultipleBoxes
        {
            get { return (bool)GetValue(AllowSelectInMultipleBoxesProperty); }
            set { SetValue(AllowSelectInMultipleBoxesProperty, value); }
        }

        public static readonly DependencyProperty AllowSelectInMultipleBoxesProperty =
            DependencyProperty.Register("AllowSelectInMultipleBoxes", typeof(bool), typeof(SldSelectionBox), new PropertyMetadata(false));

        #endregion

    }
}
