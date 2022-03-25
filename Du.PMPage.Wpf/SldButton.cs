using System.Windows;
using System.Windows.Input;
using SolidWorks.Interop.sldworks;

namespace Du.PMPage.Wpf
{
    /// <summary>
    /// 属性页按钮
    /// </summary>
    public class SldButton : SldControl<IPropertyManagerPageButton>, ISldBtnCommand
    {

        public string Caption
        {
            get { return (string)GetValue(CaptionProperty); }
            set { SetValue(CaptionProperty, value); }
        }

        /// <summary>
        /// 命令接口
        /// </summary>
        public ICommand Command
        {
            get { return (ICommand)GetValue(CommandProperty); }
            set { SetValue(CommandProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Command.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CommandProperty =
            DependencyProperty.Register("Command", typeof(ICommand), typeof(SldButton), new PropertyMetadata(null));

        // Using a DependencyProperty as the backing store for Caption.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CaptionProperty =
            DependencyProperty.Register("Caption", typeof(string), typeof(SldButton), new PropertyMetadata(null,OnCaptionCallback));

        private static void OnCaptionCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var sld = d as SldButton;
            sld.OnCaptionPropertyChanged((string)e.OldValue, (string)e.NewValue);
        }

        private void OnCaptionPropertyChanged(string oldValue, string newValue)
        {
            if (SControl != null && oldValue != newValue)
            {
                SControl.Caption = newValue;
            }
        }

        protected override void SetSldControl()
        {
            if (SControl != null && !string.IsNullOrWhiteSpace(Caption))
            {
                SControl.Caption = Caption;
            }    
        }
    }
}
