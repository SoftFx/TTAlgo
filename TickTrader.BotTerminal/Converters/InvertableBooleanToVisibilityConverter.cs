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
    [ValueConversion(typeof(bool), typeof(Visibility))]
    public class InvertableBooleanToVisibilityConverter : IValueConverter
    {
        enum Parameters
        {
            Normal,
            Inverted
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var boolValue = (bool)value;

            if(parameter != null && (Parameters)Enum.Parse(typeof(Parameters), (string)parameter) == Parameters.Inverted)
                return !boolValue ? Visibility.Visible : Visibility.Collapsed;

            return boolValue ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var result = (value is Visibility) && (Visibility)value == Visibility.Visible;

            if (parameter != null && (Parameters)Enum.Parse(typeof(Parameters), (string)parameter) == Parameters.Inverted)
                return !result;

            return result;
        }
    }
}
