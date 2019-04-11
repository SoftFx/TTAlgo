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
    public class DateTimeConverter : IValueConverter
    {
        public string Format { get; set; } = "g";
        public bool ConvertToLocal { get; set; } = true;

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (ConvertToLocal)
                return FormatDateTime(Format, ((DateTime?)value)?.ToLocalTime());
            else
                return FormatDateTime(Format, (DateTime?)value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public static string FormatDateTime(string format, DateTime? dateTime)
        {
            if (dateTime == null)
                return null;

            if (format == "GExt")
            {
                var info = AppBootstrapper.CultureCache.DateTimeFormat;

                var date = dateTime.Value.ToString(info.ShortDatePattern, AppBootstrapper.CultureCache);
                var time = dateTime.Value.ToString("HH:mm:ss.fff", AppBootstrapper.CultureCache);

                return date + " " + time;
            }
            else
                return dateTime.Value.ToString(format, AppBootstrapper.CultureCache);
            
        }
    }

    [ValueConversion(typeof(DateTime?), typeof(string))]
    public class KindAwareDateTimeConverter : IMultiValueConverter
    {
        public string Format { get; set; } = "g";

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            var timestamp = (DateTime)values[0];
            var toLocal = (bool)values[1];

            if (toLocal)
                return DateTimeConverter.FormatDateTime(Format, ((DateTime?)timestamp)?.ToLocalTime());
            else
                return DateTimeConverter.FormatDateTime(Format, (DateTime?)timestamp);
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
