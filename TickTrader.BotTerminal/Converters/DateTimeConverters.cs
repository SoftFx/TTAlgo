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
    [ValueConversion(typeof(DateTime?), typeof(string))]
    public class FullDateTimeConverter : IValueConverter
    {
        public const string Format = "yyyy.MM.dd HH:mm:ss.fff";
        public const int FormatFixedLength = 23;

        public FullDateTimeConverter()
        {
            ConvertToLocal = true;
        }

        public bool ConvertToLocal { get; set; }

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (ConvertToLocal)
                return ((DateTime?)value)?.ToLocalTime().ToString("G");
            else
                return ((DateTime?)value)?.ToString("G");
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    [ValueConversion(typeof(DateTime?), typeof(string))]
    public class KindAwareDateTimeConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            var timestamp = (DateTime)values[0];
            var toLocal = (bool)values[1];

            if (toLocal)
                return ((DateTime?)timestamp)?.ToLocalTime().ToString(FullDateTimeConverter.Format);
            else
                return ((DateTime?)timestamp)?.ToString(FullDateTimeConverter.Format);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
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
