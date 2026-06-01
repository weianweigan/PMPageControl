using System.Windows;
using System.Windows.Input;
using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;

namespace Du.PMPage.Wpf.Controls;

/// <summary>
/// WPF wrapper for a SolidWorks Property Manager Page bitmap button control
/// (<see cref="IPropertyManagerPageBitmapButton"/>).
/// </summary>
public class SldBitmapButton : SldControl<IPropertyManagerPageBitmapButton>, ISldBtnCommand
{
    static SldBitmapButton()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(SldBitmapButton),
            new FrameworkPropertyMetadata(typeof(SldBitmapButton))
        );
    }

    /// <summary>
    /// Gets or sets the standard SolidWorks bitmap to display on the button.
    /// When set, the native button will use the specified standard bitmap icon.
    /// </summary>
    public swPropertyManagerPageBitmapButtons_e? BtnStandardBitmap { get; set; }

    /// <summary>
    /// Gets or sets the command to invoke when the button is clicked.
    /// </summary>
    public ICommand Command
    {
        get { return (ICommand)GetValue(CommandProperty); }
        set { SetValue(CommandProperty, value); }
    }

    /// <summary>
    /// Dependency property for <see cref="Command"/>.
    /// </summary>
    public static readonly DependencyProperty CommandProperty = DependencyProperty.Register(
        "Command",
        typeof(ICommand),
        typeof(SldBitmapButton),
        new PropertyMetadata(null)
    );

    /// <summary>
    /// Synchronizes the managed properties to the native SolidWorks bitmap button control
    /// after the native control has been created.
    /// </summary>
    protected override void SetSldControl()
    {
        if (BtnStandardBitmap != null)
        {
            SControl.SetStandardBitmaps((int)BtnStandardBitmap.Value);
        }
    }
}
