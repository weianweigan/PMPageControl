using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using SolidWorks.Interop.sldworks;

namespace Du.PMPage.Wpf
{
    /// <summary>
    /// the wpf wrapper of <see cref="IPropertyManagerPageCheckbox"/>
    /// </summary>
    public class SldCheckBox : SldControl<IPropertyManagerPageCheckbox>
    {

        public string Caption
        {
            get { return (string)GetValue(CaptionProperty); }
            set { SetValue(CaptionProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Caption.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CaptionProperty =
            DependencyProperty.Register("Caption", typeof(string), typeof(SldCheckBox), new PropertyMetadata("CheckBox",OnCaptionPropertyCallback));

        private static void OnCaptionPropertyCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var sld = d as SldCheckBox;
            sld.OnCaptionChanged(e.OldValue as string, e.NewValue as string);
        }

        private void OnCaptionChanged(string oldValue, string newValue)
        {
            if (SControl != null
                && oldValue != newValue)
            {
                SControl.Caption = newValue;
            }
        }

        public bool Checked
        {
            get { return (bool)GetValue(CheckedProperty); }
            set { SetValue(CheckedProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Checked.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CheckedProperty =
            DependencyProperty.Register("Checked", typeof(bool), typeof(SldCheckBox), new FrameworkPropertyMetadata(false,FrameworkPropertyMetadataOptions.BindsTwoWayByDefault ,CheckedPropertyChanged));

        private static void CheckedPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var sld = d as SldCheckBox;
            sld.OnCheckedChanged((bool)e.OldValue, (bool)e.NewValue);
        }

        private void OnCheckedChanged(bool oldValue, bool newValue)
        {
            if (SControl != null
                && oldValue != newValue)
            {
                SControl.Checked = newValue;
            }
        }

        protected override void SetSldControl()
        {
            SControl.Caption = Caption;
            SControl.Checked = Checked;
        }

        internal void OnUserCheckChanged(bool @checked)
        {
            Checked = @checked;
        }
    }
}
