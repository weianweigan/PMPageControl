using SolidWorks.Interop.swconst;
using System.Collections.Generic;
using System.Windows;

namespace Du.PMPage.Wpf
{
    public class SelectionBoxManager:DependencyObject
    {
        public static bool GetJoin(DependencyObject obj)
        {
            return (bool)obj.GetValue(JoinProperty);
        }

        public static void SetJoin(DependencyObject obj, bool value)
        {
            obj.SetValue(JoinProperty, value);
        }

        /// <summary>
        /// This enables a gobal selection
        /// </summary>
        public static readonly DependencyProperty JoinProperty =
            DependencyProperty.RegisterAttached("Join", typeof(bool), typeof(SelectionBoxManager), new PropertyMetadata(false,OnStateChanged));

        private static void OnStateChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            if (obj is SldSelectionBox)
            {
                var selectionBox  = obj as SldSelectionBox;
                SldPMPage.RegisterSelectionBox(selectionBox);
            }
        }
    }

    internal static class SelectionManagerExtension
    {
        
        /// <summary>
        /// 获取匹配的选择框
        /// </summary>
        /// <param name="selectionBoxes"></param>
        /// <param name="swSelType"><see cref="swSelectType_e"/></param>
        /// <returns></returns>
        internal static IEnumerable<SldSelectionBox> GetFitSelectionBox(this IEnumerable<SldSelectionBox> selectionBoxes,swSelectType_e swSelType)
        {
            foreach (var box in selectionBoxes)
            {
                yield return box;       
            }
        }
    }
}
