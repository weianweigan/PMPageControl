using SolidWorks.Interop.sldworks;
using System;
using System.Windows;

namespace Du.PMPage.Wpf
{
    public class SldOption : SldControl<IPropertyManagerPageOption>
    {

        public bool Checked
        {
            get { return (bool)GetValue(CheckedProperty); }
            set { SetValue(CheckedProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Checked.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CheckedProperty =
            DependencyProperty.Register("Checked", typeof(bool), typeof(SldOption), new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,OnCheckedPropertyCallback));

        private static void OnCheckedPropertyCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var sld = d as SldOption;
            sld.OnCheckedChanged((bool)e.OldValue, (bool)e.NewValue);
        }

        private void OnCheckedChanged(bool oldValue, bool newValue)
        {
            if (oldValue != newValue && SControl != null)
            {
                SControl.Checked = newValue;
            }
        }

        protected override void SetSldControl()
        {
            SControl.Caption = SldCaption;
            SControl.Checked = Checked;
        }

        internal void OnCheckedUpdate()
        {
            Checked = SControl.Checked;
        }
    }
}
