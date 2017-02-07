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
        private const string _format = "dd/MM/yyyy HH:mm:ss.fff";

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return ((DateTime?)value)?.ToLocalTime().ToString(_format);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    [ValueConversion(typeof(DateTime?), typeof(string))]
    public class FullTimeConverter : IValueConverter
    {
        private const string _format = "HH:mm:ss.fff";

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return ((DateTime?)value)?.ToLocalTime().ToString(_format);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

}
