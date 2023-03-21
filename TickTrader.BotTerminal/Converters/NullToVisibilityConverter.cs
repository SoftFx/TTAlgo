using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace TickTrader.BotTerminal
{
    public class NullToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return IsEmptyValue(value) ? Visibility.Collapsed : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        private static bool IsEmptyValue(object value)
        {
            return value is string strVal ? string.IsNullOrEmpty(strVal) : value == null;
        }
    }
}
