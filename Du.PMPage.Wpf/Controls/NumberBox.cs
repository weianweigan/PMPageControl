using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Du.PMPage.Wpf.Controls;

/// <summary>
/// A numeric input <see cref="TextBox"/> that restricts text input to valid number characters.
/// Provides a <see cref="Value"/> property (double) that stays in sync with the displayed text,
/// support for decimal or integer-only input, and a configurable format string.
/// </summary>
public class NumberBox : TextBox
{
    static NumberBox()
    {
        // NumberBox uses the default TextBox styling; no additional control template is needed.
        // DefaultStyleKeyProperty.OverrideMetadata(typeof(NumberBox), new FrameworkPropertyMetadata(typeof(NumberBox)));
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="NumberBox"/> class.
    /// Disables IME input, centers content vertically, and sets the initial text to "0".
    /// </summary>
    public NumberBox()
    {
        InputMethod.SetIsInputMethodEnabled(this, false); // Disable IME for this text box.
        VerticalContentAlignment = VerticalAlignment.Center;
        Text = "0";
    }

    /// <summary>
    /// Called when the control loses focus. Validates the current text:
    /// if it is a valid number, updates <see cref="Value"/>; otherwise,
    /// replaces the text with the current <see cref="Value"/> formatted using <see cref="FormatString"/>.
    /// </summary>
    /// <param name="e">The event data.</param>
    protected override void OnLostFocus(RoutedEventArgs e)
    {
        base.OnLostFocus(e);
        if (double.TryParse(Text, out double value))
        {
            // Valid number: update the backing value.
            updateValue(value);
        }
        else
        {
            // Invalid: restore text from the current numeric value.
            updateText(Value.ToString(FormatString));
        }
    }

    private bool isUpdateValue = false,
        isUpdateText = false;

    /// <summary>
    /// Filters text input to allow only digits, a single decimal point, and a single minus sign.
    /// When <see cref="AllowDecimals"/> is false, the decimal point is also rejected.
    /// </summary>
    /// <param name="e">The text composition event data.</param>
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
                    {
                        // Decimals are not allowed.
                        e.Handled = true;
                        break;
                    }
                    if (item == '.' || item == '-')
                    {
                        // Decimal point and minus sign can appear at most once.
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
    /// Returns whether the current text already contains the specified character.
    /// </summary>
    /// <param name="c">The character to check for.</param>
    /// <returns>True if the character is present in <see cref="TextBox.Text"/>; otherwise false.</returns>
    private bool contains(char c)
    {
        if (Text == null)
        {
            return false;
        }
        foreach (var item in Text)
        {
            if (item == c)
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Silently updates <see cref="Value"/> without triggering the text-to-value
    /// synchronization loop.
    /// </summary>
    /// <param name="value">The new numeric value.</param>
    protected void updateValue(double value)
    {
        isUpdateValue = true;
        try
        {
            Value = value;
        }
        finally
        {
            isUpdateValue = false;
        }
    }

    /// <summary>
    /// Silently updates <see cref="TextBox.Text"/> without triggering the value-from-text
    /// synchronization loop.
    /// </summary>
    /// <param name="text">The new text value.</param>
    protected void updateText(string text)
    {
        isUpdateText = true;
        try
        {
            Text = text;
        }
        finally
        {
            isUpdateText = false;
        }
    }

    /// <summary>
    /// Gets or sets the current numeric value.
    /// </summary>
    public double Value
    {
        get { return (double)GetValue(ValueProperty); }
        set { SetValue(ValueProperty, value); }
    }

    /// <summary>
    /// Identifies the <see cref="Value"/> dependency property. Default is 0.
    /// </summary>
    public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
        "Value",
        typeof(double),
        typeof(NumberBox),
        new PropertyMetadata(0d)
    );

    /// <summary>
    /// Gets or sets the numeric format string used when converting <see cref="Value"/> to text.
    /// Default is "0.######" (up to 6 decimal places).
    /// </summary>
    public string FormatString
    {
        get { return (string)GetValue(FormatStringProperty); }
        set { SetValue(FormatStringProperty, value); }
    }

    /// <summary>
    /// Identifies the <see cref="FormatString"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty FormatStringProperty = DependencyProperty.Register(
        "FormatString",
        typeof(string),
        typeof(NumberBox),
        new PropertyMetadata("0.######")
    );

    /// <summary>
    /// Gets or sets whether decimal input is allowed. When false, the decimal point
    /// character is rejected and <see cref="Value"/> is coerced to an integer on text change.
    /// Default is true.
    /// </summary>
    public bool AllowDecimals
    {
        get { return (bool)GetValue(AllowDecimalsProperty); }
        set { SetValue(AllowDecimalsProperty, value); }
    }

    /// <summary>
    /// Identifies the <see cref="AllowDecimals"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty AllowDecimalsProperty = DependencyProperty.Register(
        "AllowDecimals",
        typeof(bool),
        typeof(NumberBox),
        new PropertyMetadata(true)
    );

    /// <summary>
    /// Called when a dependency property value changes. Triggers <see cref="OnValueChanged"/>
    /// when <see cref="Value"/> changes, and updates the displayed text when
    /// <see cref="FormatString"/> changes.
    /// </summary>
    /// <param name="e">The event data.</param>
    protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);
        if (e.Property == ValueProperty)
        {
            OnValueChanged();
        }
        else if (e.Property == FormatStringProperty)
        {
            updateText(Value.ToString(FormatString));
        }
    }

    /// <summary>
    /// Occurs when the <see cref="Value"/> property has changed.
    /// </summary>
    public event EventHandler ValueChanged;

    /// <summary>
    /// Raises the <see cref="ValueChanged"/> event and updates the displayed text
    /// to reflect the new value (unless the change originated from the text box itself).
    /// </summary>
    protected virtual void OnValueChanged()
    {
        if (!isUpdateValue)
        {
            updateText(Value.ToString(FormatString));
        }
        ValueChanged?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Called when the text content changes. Parses the new text and updates
    /// <see cref="Value"/> accordingly, truncating to an integer when
    /// <see cref="AllowDecimals"/> is false.
    /// </summary>
    /// <param name="e">The text changed event data.</param>
    protected override void OnTextChanged(TextChangedEventArgs e)
    {
        if (!isUpdateText)
        {
            if (double.TryParse(Text, out double value))
            {
                updateValue(AllowDecimals ? value : Convert.ToInt32(value)); // Force integer when decimals are not allowed.
            }
        }
        base.OnTextChanged(e);
    }
}
