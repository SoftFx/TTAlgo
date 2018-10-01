using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;

namespace TickTrader.BotTerminal
{
    [ValueConversion(typeof(DateTime?), typeof(string))]
    public class FullDateTimeConverter : IValueConverter
    {
        public const string Format = "yyyy.MM.dd HH:mm:ss.fff";

        public FullDateTimeConverter()
        {
            ConvertToLocal = true;
        }

        public bool ConvertToLocal { get; set; }

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (ConvertToLocal)
                return ((DateTime?)value)?.ToLocalTime().ToString(Format);
            else
                return ((DateTime?)value)?.ToString(Format);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    [ValueConversion(typeof(DateTime?), typeof(string))]
    public class FullTimeConverter : IValueConverter
    {
        public const string Format = "HH:mm:ss.fff";

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return ((DateTime?)value)?.ToLocalTime().ToString(Format);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

}
