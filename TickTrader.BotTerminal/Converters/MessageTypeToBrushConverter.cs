using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Media;

namespace TickTrader.BotTerminal
{
    [ValueConversion(typeof(JournalMessageType), typeof(SolidColorBrush))]
    public class MessageTypeToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var level = (JournalMessageType)value;
            switch(level)
            {
                case JournalMessageType.Info: return new SolidColorBrush(Colors.Gray);
                case JournalMessageType.Trading: return new SolidColorBrush(Colors.SkyBlue);
                case JournalMessageType.TradingSuccess: return new SolidColorBrush(Colors.Green);
                case JournalMessageType.TradingFail: return new SolidColorBrush(Colors.DarkOrange);
                case JournalMessageType.Error: return new SolidColorBrush(Colors.Red);
                case JournalMessageType.Custom: return new SolidColorBrush(Colors.Violet);
                default: return null;
            } 
        }
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

}
