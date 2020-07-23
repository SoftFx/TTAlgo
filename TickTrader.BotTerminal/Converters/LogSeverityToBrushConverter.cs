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
            var level = (UnitLogRecord.Types.LogSeverity)value;
            switch (level)
            {
                case UnitLogRecord.Types.LogSeverity.Info: return new SolidColorBrush(Colors.Gray);
                case UnitLogRecord.Types.LogSeverity.Trade: return new SolidColorBrush(Colors.SkyBlue);
                case UnitLogRecord.Types.LogSeverity.TradeSuccess: return new SolidColorBrush(Colors.Green);
                case UnitLogRecord.Types.LogSeverity.TradeFail: return new SolidColorBrush(Colors.DarkOrange);
                case UnitLogRecord.Types.LogSeverity.Error: return new SolidColorBrush(Colors.Red);
                case UnitLogRecord.Types.LogSeverity.Custom: return new SolidColorBrush(Colors.Violet);
                case UnitLogRecord.Types.LogSeverity.Alert: return new SolidColorBrush(Colors.Khaki);
                default: return null;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
