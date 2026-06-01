using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;

namespace Du.PMPage.Wpf.Controls;

/// <summary>
/// A <see cref="TextBox"/> designed for use inside a SolidWorks PropertyManager page.
/// Handles the <c>WM_GETDLGCODE</c> message to request arrow keys, character input,
/// and selection notifications from the native dialog manager. This ensures proper
/// keyboard navigation and text editing behavior when the WPF text box is hosted
/// inside a native SolidWorks dialog.
/// </summary>
public class IOTextBox : TextBox
{
    private const uint DLGC_WANTARROWS = 0x0001;
    private const uint DLGC_WANTTAB = 0x0002;
    private const uint DLGC_WANTALLKEYS = 0x0004;
    private const uint DLGC_HASSETSEL = 0x0008;
    private const uint DLGC_WANTCHARS = 0x0080;
    private const uint WM_GETDLGCODE = 0x0087;

    private HwndSource _hwndSource;
    private HwndSourceHook _hook;

    /// <summary>
    /// Initializes a new instance of the <see cref="IOTextBox"/> class.
    /// Registers an <see cref="HwndSourceHook"/> that handles the
    /// <c>WM_GETDLGCODE</c> window message to request dialog keyboard messages
    /// (arrow keys and character input) for proper behavior within a SolidWorks
    /// PropertyManager page.
    /// </summary>
    public IOTextBox() : base()
    {
        _hook = new HwndSourceHook(ChildHwndSourceHook);
        Loaded += (s, e) =>
        {
            _hwndSource = PresentationSource.FromVisual(this) as HwndSource;
            _hwndSource?.AddHook(_hook);
        };
        Unloaded += (s, e) =>
        {
            _hwndSource?.RemoveHook(_hook);
            _hwndSource = null;
        };
    }

    /// <summary>
    /// Handles the <c>WM_GETDLGCODE</c> window message. Returns a combination of
    /// <c>DLGC_WANTCHARS</c>, <c>DLGC_WANTARROWS</c>, and <c>DLGC_HASSETSEL</c>
    /// so that the native dialog manager forwards character input, arrow keys,
    /// and supports text selection notifications.
    /// </summary>
    /// <param name="hwnd">The window handle.</param>
    /// <param name="msg">The message identifier.</param>
    /// <param name="wParam">Additional message-specific information.</param>
    /// <param name="lParam">Additional message-specific information.</param>
    /// <param name="handled">Set to true to indicate the message has been handled.</param>
    /// <returns>An <see cref="IntPtr"/> representing the dialog code flags.</returns>
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
