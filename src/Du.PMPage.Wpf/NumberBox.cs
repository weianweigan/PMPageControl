namespace Du.PMPage.Wpf
{
    using System;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;

    public class NumberBox : TextBox
    {
        static NumberBox()
        {
            //NumberBox基本不需要额外的样式，用Textbox的就够了
            //DefaultStyleKeyProperty.OverrideMetadata(typeof(NumberBox), new FrameworkPropertyMetadata(typeof(NumberBox)));
        }
        public NumberBox()
        {
            InputMethod.SetIsInputMethodEnabled(this, false);//禁用文本框的输入法 
            this.VerticalContentAlignment = VerticalAlignment.Center;
            this.Text = "0";
        }

        protected override void OnLostFocus(RoutedEventArgs e)
        {
            base.OnLostFocus(e);
            if (double.TryParse(this.Text, out double value))
            {//如果是有效的数字则替换当前值
                updateValue(value);
            }
            else
            {//失去焦点时如果不是有效的数字则用当前值替换用户的输入
                updateText(this.Value.ToString(this.FormatString));
            }
        }

        private bool isUpdateValue = false, isUpdateText = false;

        protected override void OnTextInput(TextCompositionEventArgs e)
        {
            var txt = e.Text;
            if (txt != null)
            {
                foreach (var item in txt)
                {
                    if (!(char.IsDigit(item) || item == '.' || item == '-'))
                    {
                        e.Handled = true;
                        break;
                    }
                    else
                    {
                        if (!AllowDecimals && item == '.')
                        {//不允许输入小数
                            e.Handled = true;
                            break;
                        }
                        if (item == '.' || item == '-')
                        {//小数点和负号只能出现一次
                            if (contains(item))
                            {
                                e.Handled = true;
                                break;
                            }
                        }
                    }

                }
            }
            else
            {
                e.Handled = true;
            }

            base.OnTextInput(e);
        }

        /// <summary>
        /// 当前文本中是否包含指定的字符
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        private bool contains(char c)
        {
            if (this.Text == null)
            {
                return false;
            }
            foreach (var item in this.Text)
            {
                if (item == c)
                {
                    return true;
                }
            }
            return false;
        }


        /// <summary>
        /// 静默更新值
        /// </summary>
        /// <param name="value"></param>
        protected void updateValue(double value)
        {
            isUpdateValue = true;
            try
            {
                this.Value = value;
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                isUpdateValue = false;
            }
        }
        /// <summary>
        /// 静默更新文本
        /// </summary>
        /// <param name="text"></param>
        protected void updateText(string text)
        {
            isUpdateText = true;
            try
            {
                this.Text = text;
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                isUpdateText = false;
            }
        }

        /// <summary>
        /// 当前值
        /// </summary>
        public double Value
        {
            get { return (double)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value", typeof(double), typeof(NumberBox), new PropertyMetadata(0d));


        /// <summary>
        /// 数值的格式化字符串,默认最多保留小数点后6位。
        /// </summary>
        public string FormatString
        {
            get { return (string)GetValue(FormatStringProperty); }
            set { SetValue(FormatStringProperty, value); }
        }

        public static readonly DependencyProperty FormatStringProperty =
            DependencyProperty.Register("FormatString", typeof(string), typeof(NumberBox), new PropertyMetadata("0.######"));


        /// <summary>
        /// 是否允许输入小数
        /// </summary>
        public bool AllowDecimals
        {
            get { return (bool)GetValue(AllowDecimalsProperty); }
            set { SetValue(AllowDecimalsProperty, value); }
        }

        public static readonly DependencyProperty AllowDecimalsProperty =
            DependencyProperty.Register("AllowDecimals", typeof(bool), typeof(NumberBox), new PropertyMetadata(true));



        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);
            if (e.Property == ValueProperty)
            {
                this.OnValueChanged();
            }
            else if (e.Property == FormatStringProperty)
            {
                updateText(this.Value.ToString(this.FormatString));
            }
        }
        /// <summary>
        /// 在当前值改变后触发
        /// </summary>
        public event EventHandler ValueChanged;

        protected virtual void OnValueChanged()
        {
            if (!isUpdateValue)
            {
                updateText(this.Value.ToString(this.FormatString));
            }
            this.ValueChanged?.Invoke(this, EventArgs.Empty);
        }
        protected override void OnTextChanged(TextChangedEventArgs e)
        {
            if (!isUpdateText)
            {
                if (double.TryParse(this.Text, out double value))
                {
                    updateValue(AllowDecimals ? value : Convert.ToInt32(value));//不允许小数则将值强行取整
                }
            }
            base.OnTextChanged(e);
        }
    }
}
