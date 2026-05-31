using System;
using System.Windows.Controls;
using System.Windows.Interop;

namespace Du.PMPage.Wpf.Controls;

/// <summary>
/// TextBox Used in PMPage
/// </summary>
public class IOTextBox:TextBox
{
    private const uint DLGC_WANTARROWS = 0x0001;
    private const uint DLGC_WANTTAB = 0x0002;
    private const uint DLGC_WANTALLKEYS = 0x0004;
    private const uint DLGC_HASSETSEL = 0x0008;
    private const uint DLGC_WANTCHARS = 0x0080;
    private const uint WM_GETDLGCODE = 0x0087;

    public IOTextBox() : base()
    {
        Loaded += delegate
        {
            HwndSource s = System.Windows.PresentationSource.FromVisual(this) as HwndSource;
            if (s != null)
                s.AddHook(new HwndSourceHook(ChildHwndSourceHook));
        };
    }

    IntPtr ChildHwndSourceHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        if (msg == WM_GETDLGCODE)
        {
            handled = true;
            return new IntPtr(DLGC_WANTCHARS | DLGC_WANTARROWS | DLGC_HASSETSEL);
        }
        return IntPtr.Zero;
    }
}
