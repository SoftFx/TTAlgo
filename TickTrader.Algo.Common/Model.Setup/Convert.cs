using System.Windows.Media;

namespace TickTrader.Algo.Common.Model.Setup
{
    public static class Convert
    {
        public static Color ToWindowsColor(Api.Colors apiColor)
        {
            return ToWindowsColor(apiColor, Colors.Green);
        }

        public static Color ToWindowsColor(Api.Colors apiColor, Color autoColor)
        {
            if (apiColor == Api.Colors.Auto)
                return autoColor;
            else
            {
                int colorInt = (int)apiColor;
                byte r = (byte)(colorInt >> 16);
                byte g = (byte)(colorInt >> 8);
                byte b = (byte)(colorInt >> 0);
                return Color.FromRgb(r, g, b);
            }
        }

        public static Color ToWindowsColor(int colorRgb)
        {
            return ToWindowsColor(colorRgb, Colors.Green);
        }

        public static Color ToWindowsColor(int colorRgb, Color autoColor)
        {
            if (colorRgb == -1)
                return autoColor;
            else
            {
                byte r = (byte)(colorRgb >> 16);
                byte g = (byte)(colorRgb >> 8);
                byte b = (byte)(colorRgb >> 0);
                return Color.FromRgb(r, g, b);
            }
        }
    }
}
