using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;
using TickTrader.Algo.Core;

namespace TickTrader.BotTerminal
{
    [ValueConversion(typeof(JournalMessageType), typeof(SolidColorBrush))]
    public class LogSeverityToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var level = (LogSeverities)value;
            switch (level)
            {
                case LogSeverities.Info: return new SolidColorBrush(Colors.Gray);
                case LogSeverities.Trade: return new SolidColorBrush(Colors.SkyBlue);
                case LogSeverities.TradeSuccess: return new SolidColorBrush(Colors.Green);
                case LogSeverities.TradeFail: return new SolidColorBrush(Colors.DarkOrange);
                case LogSeverities.Error: return new SolidColorBrush(Colors.Red);
                case LogSeverities.Custom: return new SolidColorBrush(Colors.Violet);
                default: return null;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
