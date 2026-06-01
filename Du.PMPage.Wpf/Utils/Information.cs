using System;

namespace Du.PMPage.Wpf.Utils
{
    internal static class Information
    {
        /// <summary>
        /// Converts RGB color values to a SolidWorks color integer (BGR format).
        /// Values are clamped to the 0-255 range.
        /// </summary>
        internal static int RGB(int red, int green, int blue)
        {
            red = Math.Max(0, Math.Min(255, red));
            green = Math.Max(0, Math.Min(255, green));
            blue = Math.Max(0, Math.Min(255, blue));

            return checked(blue * 65536 + green * 256 + red);
        }
    }
}
