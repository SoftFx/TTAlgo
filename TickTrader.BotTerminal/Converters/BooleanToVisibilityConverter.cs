using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;

namespace TickTrader.BotTerminal
{
    [ValueConversion(typeof(bool), typeof(Visibility))]
    public class BooleanToVisibilityConverter : IValueConverter, IMultiValueConverter
    {
        public enum Mode
        {
            All,
            LeastOne
        }

        public bool Invert { get; set; }

        public Mode ConversionMode { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var boolValue = (bool)value;

            if (Invert)
                boolValue = !boolValue;

            return boolValue ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool result = (value is Visibility vis) && vis == Visibility.Visible;

            return Invert ? !result : result;
        }

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            var boolValue = ConversionMode == Mode.All ? !values.Any(v => !(bool)v) : values.Any(v => (bool)v);

            if (Invert)
                return !boolValue ? Visibility.Visible : Visibility.Collapsed;

            return boolValue ? Visibility.Visible : Visibility.Collapsed;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
