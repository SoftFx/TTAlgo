using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace TickTrader.BotTerminal
{
    public class NullToVisibilityConverter : IValueConverter, IMultiValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return IsEmptyValue(value) ? Visibility.Collapsed : Visibility.Visible;
        }

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            bool collapsed = true;

            foreach (var val in values)
                collapsed &= IsEmptyValue(val);

            return !collapsed ;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        private bool IsEmptyValue(object value)
        {
            if (value is string)
                return string.IsNullOrEmpty(value as string);
            else
                return value == null;
        }
    }
}
