using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;

namespace TickTrader.BotTerminal
{
    [ValueConversion(typeof(Color), typeof(string))]
    public class ColorToHexConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var color = ((Color)value);
            return string.Format("{0:X2}{1:X2}{2:X2}", color.R, color.G, color.B);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var hexColor = ((string)value).Replace("#", "");

            if (string.IsNullOrWhiteSpace(hexColor))
                hexColor = "FF";
            else if (hexColor.Length == 3)
            {
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < 3; i++)
                {
                    sb.Append(hexColor[i]);
                    sb.Append(hexColor[i]);
                }
                hexColor = sb.ToString();
            }
            else if (hexColor.Length > 1 && hexColor.Length % 2 != 0)
                hexColor += hexColor[hexColor.Length - 1];

            try
            {
                return (Color)ColorConverter.ConvertFromString(string.Format("#FF{0}", hexColor));
            }
            catch
            {
                return null;
            }
        }
    }
}
