using System;
using System.Windows;
using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;

namespace Du.PMPage.Wpf
{
    public class SldTextBox : SldControl<IPropertyManagerPageTextbox>
    {
        public int? SldHeight
        {
            get { return (int?)GetValue(SldHeightProperty); }
            set { SetValue(SldHeightProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SldHeight.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SldHeightProperty =
            DependencyProperty.Register("SldHeight", typeof(int?), typeof(SldTextBox), new FrameworkPropertyMetadata(null,FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,OnSldHeightPropertyChangedCallback));

        private static void OnSldHeightPropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var sld = d as SldTextBox;
            sld.OnHeightChanged((int?)e.OldValue, (int?)e.NewValue);
        }

        private void OnHeightChanged(int? oldValue, int? newValue)
        {
            if (SControl != null && newValue != null && oldValue != newValue)
            {
                SControl.Height = (short)newValue.Value;
            }
        }


        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Text.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string), typeof(SldTextBox), new FrameworkPropertyMetadata(null,FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,OnTextPropertyCallback));

        private static void OnTextPropertyCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var sld = d as SldTextBox;
            sld.OnTextChanged((string)e.OldValue, (string)e.NewValue);
        }

        private void OnTextChanged(string oldValue, string newValue)
        {
            if (SControl != null && oldValue != newValue)
            {
                SControl.Text = newValue;
            }
        }

        public swPropMgrPageTextBoxStyle_e SldStyle
        {
            get { return (swPropMgrPageTextBoxStyle_e)GetValue(SldStyleProperty); }
            set { SetValue(SldStyleProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SldStyle.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SldStyleProperty =
            DependencyProperty.Register("SldStyle", typeof(swPropMgrPageTextBoxStyle_e), typeof(SldTextBox), new PropertyMetadata(swPropMgrPageTextBoxStyle_e.swPropMgrPageTextBoxStyle_NotifyOnlyWhenFocusLost,OnStylePropertyCallback));

        private static void OnStylePropertyCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var sld = d as SldTextBox;
            sld.OnSldStyleChanged((swPropMgrPageTextBoxStyle_e)e.OldValue, (swPropMgrPageTextBoxStyle_e)e.NewValue);
        }

        private void OnSldStyleChanged(swPropMgrPageTextBoxStyle_e oldValue, swPropMgrPageTextBoxStyle_e newValue)
        {
            if (SControl != null && oldValue != newValue)
            {
                SControl.Style = (int)newValue;
            }
        }

        ///<inheritdoc/>
        protected override void SetSldControl()
        {
            if (SldHeight != null)
            {
                SControl.Height = (short)SldHeight.Value;
            }
            if (!string.IsNullOrEmpty(Text))
            {
                SControl.Text = Text;
            }
        }
    }
}
