using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace TickTrader.BotTerminal
{
    public class TextLengthComparisonConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is TextBlock textBlock)
            {
                var ft = new FormattedText(textBlock.Text, CultureInfo.CurrentUICulture, textBlock.FlowDirection, 
                                            new Typeface(textBlock.FontFamily, textBlock.FontStyle, textBlock.FontWeight, textBlock.FontStretch),
                                            textBlock.FontSize, textBlock.Foreground);

                return ft.Width > textBlock.Width;
            }

            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
