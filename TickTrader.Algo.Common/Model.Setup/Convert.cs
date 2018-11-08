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
    }
}
