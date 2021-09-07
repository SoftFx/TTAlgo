using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;
using TickTrader.Algo.Core;
using TickTrader.Algo.Domain;

namespace TickTrader.BotTerminal
{
    [ValueConversion(typeof(JournalMessageType), typeof(SolidColorBrush))]
    public class LogSeverityToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var level = (PluginLogRecord.Types.LogSeverity)value;
            switch (level)
            {
                case PluginLogRecord.Types.LogSeverity.Info: return new SolidColorBrush(Colors.Gray);
                case PluginLogRecord.Types.LogSeverity.Trade: return new SolidColorBrush(Colors.SkyBlue);
                case PluginLogRecord.Types.LogSeverity.TradeSuccess: return new SolidColorBrush(Colors.Green);
                case PluginLogRecord.Types.LogSeverity.TradeFail: return new SolidColorBrush(Colors.DarkOrange);
                case PluginLogRecord.Types.LogSeverity.Error: return new SolidColorBrush(Colors.Red);
                case PluginLogRecord.Types.LogSeverity.Custom: return new SolidColorBrush(Colors.Violet);
                case PluginLogRecord.Types.LogSeverity.Alert: return new SolidColorBrush(Colors.Khaki);
                default: return null;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
