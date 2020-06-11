using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace TickTrader.BotTerminal
{
    public class DoubleRoundConverter : IValueConverter
    {
        private static readonly string _template = $"0.{new string('#', 10)}";

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is double d ? d.ToString(_template, CultureInfo.InvariantCulture) : value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
