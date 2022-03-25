using System.Windows;
using System.Windows.Input;
using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;

namespace Du.PMPage.Wpf
{
    public interface ISldBtnCommand
    {
        ICommand Command { get; set; }
    }

    public class SldBitmapButton : SldControl<IPropertyManagerPageBitmapButton>, ISldBtnCommand
    {
        public swPropertyManagerPageBitmapButtons_e? BtnStandardBitmap { get; set; }

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
            DependencyProperty.Register("Command", typeof(ICommand), typeof(SldBitmapButton), new PropertyMetadata(null));


        protected override void SetSldControl()
        {
            if (BtnStandardBitmap != null)
            {
                SControl.SetStandardBitmaps((int)BtnStandardBitmap.Value);
            }
        }
    }

    public class SldCheckableBitmapButton: SldBitmapButton
    {

        /// <summary>
        /// True if bitmap button is clickable, false if not
        /// </summary>
        public bool IsCheckable
        {
            get { return (bool)GetValue(IsCheckableProperty); }
            set { SetValue(IsCheckableProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsCheckable.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsCheckableProperty =
            DependencyProperty.Register("IsCheckable", typeof(bool), typeof(SldCheckableBitmapButton), new PropertyMetadata(true,OnIsCheckableCallback));

        /// <summary>
        /// 是否选中
        /// </summary>
        public bool Checked
        {
            get { return (bool)GetValue(CheckedProperty); }
            set { SetValue(CheckedProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Checked.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CheckedProperty =
            DependencyProperty.Register("Checked", typeof(bool), typeof(SldCheckableBitmapButton), new PropertyMetadata(false, OnCheckedCallback));

        private static void OnIsCheckableCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var sld = d as SldCheckableBitmapButton;
            sld.OnIsCheckablePropertyCallback((bool)e.OldValue, (bool)e.NewValue);
        }

        private void OnIsCheckablePropertyCallback(bool oldValue, bool newValue)
        {
            if (SControl != null && oldValue != newValue)
            {
                SControl.IsCheckable = newValue;
            }
        }

        private static void OnCheckedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var sld = d as SldCheckableBitmapButton;
            sld.OnCheckedPropertyChanged((bool)e.OldValue, (bool)e.NewValue);
        }

        private void OnCheckedPropertyChanged(bool oldValue, bool newValue)
        {
            if (SControl != null && oldValue != newValue)
            {
                if (IsCheckable)
                {
                    SControl.Checked = newValue;
                }
            }
        }

        protected override void SetSldControl()
        {
            SControl.IsCheckable = IsCheckable;
            SControl.Checked = Checked;
            if (BtnStandardBitmap != null)
            {
                SControl.SetStandardBitmaps((int)BtnStandardBitmap.Value);
            }
        }
    }
}
