using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace TickTrader.BotTerminal
{
    public class AdaptiveComboBoxConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Count() == 2 && values[0] is ComboBox box && values[1] is GridLength minLength)
            {
                var ft = new FormattedText(box.Text, CultureInfo.CurrentUICulture, box.FlowDirection,
                                            new Typeface(box.FontFamily, box.FontStyle, box.FontWeight, box.FontStretch),
                                            box.FontSize, box.Foreground);

                return ft.Width < minLength.Value ? minLength.Value : ft.Width;
            }

            return null;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
