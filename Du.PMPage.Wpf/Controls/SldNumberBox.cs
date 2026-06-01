using System;
using System.Windows;
using SolidWorks.Interop.sldworks;

namespace Du.PMPage.Wpf.Controls;

public class SldNumberBox : SldControl<IPropertyManagerPageNumberbox>
{
    static SldNumberBox()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(SldNumberBox),
            new FrameworkPropertyMetadata(typeof(SldNumberBox))
        );
    }

    /// <summary>
    /// 数字输入框中的值
    /// </summary>
    public double Value
    {
        get { return (double)GetValue(ValueProperty); }
        set { SetValue(ValueProperty, value); }
    }

    // Using a DependencyProperty as the backing store for Value.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
        "Value",
        typeof(double),
        typeof(SldNumberBox),
        new FrameworkPropertyMetadata(
            default(double),
            FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
            OnValuePropertyCallback
        )
    );

    private static void OnValuePropertyCallback(
        DependencyObject d,
        DependencyPropertyChangedEventArgs e
    )
    {
        var sld = d as SldNumberBox;
        sld.OnValueChanged((double)e.OldValue, (double)e.NewValue);
    }

    private void OnValueChanged(double oldValue, double newValue)
    {
        if (oldValue != newValue && SControl != null)
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
    public static readonly DependencyProperty NumberBoxRangeProperty = DependencyProperty.Register(
        "NumberBoxRange",
        typeof(NumberBoxRange),
        typeof(SldNumberBox),
        new PropertyMetadata(null, OnNumberBoxRangedCallback)
    );

    private static void OnNumberBoxRangedCallback(
        DependencyObject d,
        DependencyPropertyChangedEventArgs e
    )
    {
        var sld = d as SldNumberBox;
        sld.OnNumberBoxChanged((NumberBoxRange)e.OldValue, (NumberBoxRange)e.NewValue);
    }

    private void OnNumberBoxChanged(NumberBoxRange oldValue, NumberBoxRange newValue)
    {
        if (oldValue != newValue && SControl != null && newValue != null)
        {
            if (oldValue != null && oldValue.Units != newValue.Units && SldControlVisibility)
            {
                throw new InvalidOperationException(
                    $"Cannot change the {nameof(NumberBoxRange.Units)} property after the control has been displayed."
                );
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
    public static readonly DependencyProperty MaximumProperty = DependencyProperty.Register(
        "Maximum",
        typeof(double?),
        typeof(SldNumberBox),
        new FrameworkPropertyMetadata(
            null,
            FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
            OnMaximumPropertyChanged
        )
    );

    private static void OnMaximumPropertyChanged(
        DependencyObject d,
        DependencyPropertyChangedEventArgs e
    )
    {
        var sld = d as SldNumberBox;
        sld.OnMaximumChanged((double?)e.OldValue, (double?)e.NewValue);
    }

    private void OnMaximumChanged(double? oldValue, double? newValue)
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
    public static readonly DependencyProperty MinimumProperty = DependencyProperty.Register(
        "Minimum",
        typeof(double?),
        typeof(SldNumberBox),
        new FrameworkPropertyMetadata(
            null,
            FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
            OnMinimumPropertyChanged
        )
    );

    private static void OnMinimumPropertyChanged(
        DependencyObject d,
        DependencyPropertyChangedEventArgs e
    )
    {
        var sld = d as SldNumberBox;
        sld.OnMinimumChanged((double?)e.OldValue, (double?)e.NewValue);
    }

    private void OnMinimumChanged(double? oldValue, double? newValue)
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
            newValue.SlowIncr
        );
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
