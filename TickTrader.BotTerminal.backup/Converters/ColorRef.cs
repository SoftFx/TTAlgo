using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;

namespace TickTrader.BotTerminal
{
    public class ColorRefToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            Int32 color = (Int32)value;

            byte a = (byte)((/*color*/0xff000000 & 0xff000000) >> 24);
            byte r = (byte)((color & 0x00ff0000) >> 16);
            byte g = (byte)((color & 0x0000ff00) >> 8);
            byte b = (byte)(color & 0x000000ff);

            if (targetType == typeof(Brush))
            {
                return new SolidColorBrush(Color.FromArgb(a, r, g, b));
            }

            throw new FormatException("Only converts to Media.Brush");
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

}
