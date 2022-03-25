namespace Du.PMPage.Wpf
{
    public static class Information
    {
        public static int RGB(int red, int green, int blue)
        {
            if (red > 255)
            {
                red = 255;
            }

            if (green > 255)
            {
                green = 255;
            }

            if (blue > 255)
            {
                blue = 255;
            }

            return checked(blue * 65536 + green * 256 + red);
        }
    }
}
