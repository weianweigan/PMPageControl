using System.Windows;
using System.Windows.Input;
using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;

namespace Du.PMPage.Wpf.Controls;

public class SldBitmapButton : SldControl<IPropertyManagerPageBitmapButton>, ISldBtnCommand
{
    static SldBitmapButton()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(SldBitmapButton),
            new FrameworkPropertyMetadata(typeof(SldBitmapButton))
        );
    }

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
    public static readonly DependencyProperty CommandProperty = DependencyProperty.Register(
        "Command",
        typeof(ICommand),
        typeof(SldBitmapButton),
        new PropertyMetadata(null)
    );

    protected override void SetSldControl()
    {
        if (BtnStandardBitmap != null)
        {
            SControl.SetStandardBitmaps((int)BtnStandardBitmap.Value);
        }
    }
}
