using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;
using System;
using System.Linq;
using System.Windows;

namespace Du.PMPage.Wpf
{
    public class NumberBoxRange
    {
        /// <summary>
        /// 单位
        /// </summary>
        public swNumberboxUnitType_e Units { get; set; }

        /// <summary>
        /// 最小值
        /// </summary>
        public double Minimum { get; set; }

        /// <summary>
        /// 最大值
        /// </summary>
        public double Maximum { get; set; }

        /// <summary>
        /// true表示包括此范围，false表示在此范围外
        /// </summary>
        public bool Inclusive { get; set; } = true;

        /// <summary>
        /// 每次增长的值
        /// </summary>
        public double Increment { get; set; } = 1;

        /// <summary>
        /// 鼠标快速滚动增长的值
        /// </summary>
        public double FastIncr { get; set; } = 1;

        /// <summary>
        /// 鼠标慢速滚动增长的值
        /// </summary>
        public double SlowIncr { get; set; } = 1;
    }

    public class SldNumberBox : SldControl<IPropertyManagerPageNumberbox>
    {
        /// <summary>
        /// 数字输入框中的值
        /// </summary>
        public double Value
        {
            get { return (double)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Value.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value", typeof(double), typeof(SldNumberBox), new FrameworkPropertyMetadata(default(double),FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,OnValuePropertyCallback));

        private static void OnValuePropertyCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var sld = d as SldNumberBox;
            sld.OnValueChanged((double)e.OldValue, (double)e.NewValue);
        }

        private void OnValueChanged(double oldValue, double newValue)
        {
            if (oldValue != newValue &&
                SControl != null)
            {
                SControl.Value = newValue;
            }
        }

        public NumberBoxRange NumberBoxRange
        {
            get { return (NumberBoxRange)GetValue(NumberBoxRangeProperty); }
            set { SetValue(NumberBoxRangeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for NumberBoxRange.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty NumberBoxRangeProperty =
            DependencyProperty.Register("NumberBoxRange", typeof(NumberBoxRange), typeof(SldNumberBox), new PropertyMetadata(null,OnNumberBoxRangedCallback));

        private static void OnNumberBoxRangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var sld = d as SldNumberBox;
            sld.OnNumberBoxChanged((NumberBoxRange)e.OldValue, (NumberBoxRange)e.NewValue);
        }

        private void OnNumberBoxChanged(NumberBoxRange oldValue, NumberBoxRange newValue)
        {
            if (oldValue != newValue
                && SControl != null &&
                newValue != null)
            {
                if (oldValue != null &&
                    newValue.Units != newValue.Units &&
                    SldControlVisibility)
                {
                    throw new InvalidOperationException($"控件显示后不能更改单位属性：{nameof(NumberBoxRange.Units)}");
                }

                SetNumberBoxRange(newValue);
            }
        }



        public double? Maximum
        {
            get { return (double)GetValue(MaximumProperty); }
            set { SetValue(MaximumProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Maximum.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MaximumProperty =
            DependencyProperty.Register("Maximum", typeof(double?), typeof(SldNumberBox),
                new FrameworkPropertyMetadata(null,FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,OnMaxiumuPropertyChanged));

        private static void OnMaxiumuPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var sld = d as SldNumberBox;
            sld.OnMaxiumuChanged((double?)e.OldValue, (double?)e.NewValue);
        }

        private void OnMaxiumuChanged(double? oldValue, double? newValue)
        {
            if (newValue != null && SControl != null)
            {
                if (oldValue != null && oldValue.Value == newValue.Value)
                {
                    return;
                }

                if (NumberBoxRange == null)
                {
                    throw new ArgumentException("Set NumberBoxRange Property First");
                }

                NumberBoxRange.Maximum = newValue.Value;

                SetNumberBoxRange(NumberBoxRange);
            }
        }

        public double? Minimum
        {
            get { return (double?)GetValue(MinimumProperty); }
            set { SetValue(MinimumProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Minimum.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MinimumProperty =
            DependencyProperty.Register("Minimum", 
                typeof(double?), 
                typeof(SldNumberBox), 
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,OnMinimumPropertyChanged));

        private static void OnMinimumPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var sld = d as SldNumberBox;
            sld.OnMinimumChanged((double?)e.OldValue, (double?)e.NewValue);
        }

        private void OnMinimumChanged(double? oldValue,double? newValue)
        {
            if (newValue != null && SControl != null)
            {
                if (oldValue != null && oldValue.Value == newValue.Value)
                {
                    return;
                }

                if (NumberBoxRange == null)
                {
                    throw new ArgumentException("Set NumberBoxRange Property First");
                }

                NumberBoxRange.Minimum = newValue.Value;

                SetNumberBoxRange(NumberBoxRange);
            }
        }

        private void SetNumberBoxRange(NumberBoxRange newValue)
        {
            SControl.SetRange2(
                (int)newValue.Units,
                newValue.Minimum,
                newValue.Maximum,
                newValue.Inclusive,
                newValue.Increment,
                newValue.FastIncr,
                newValue.SlowIncr);
        }

        ///<inheritdoc/>
        protected override void SetSldControl()
        {
            SControl.Value = Value;
            if (NumberBoxRange != null)
            {
                SetNumberBoxRange(NumberBoxRange);
            }
        }

        internal void OnValueUpdate(double value)
        {
            if (value != Value)
            {
                Value = value;
            }
        }
    }
}
