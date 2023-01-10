using System;
using System.Globalization;
using System.Windows.Data;

namespace TickTrader.BotTerminal
{
    public class NullToBoolConverter : IValueConverter, IMultiValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return IsEmptyValue(value);
        }

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            foreach (var val in values)
            {
                if (!IsEmptyValue(val))
                    return true;
            }

            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        private static bool IsEmptyValue(object value)
        {
            return value is string strVal ? string.IsNullOrEmpty(strVal) : value == null;
        }
    }
}
