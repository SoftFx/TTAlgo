using SoftFX.Extended;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace TickTrader.BotTerminal
{
    class TradeSideTypeConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length != 2)
                throw new ArgumentException("Wrong number of arguments", nameof(values));

            TradeTransactionModel.TradeSide side = (TradeTransactionModel.TradeSide)values[0];
            TradeRecordType type = (TradeRecordType)values[1];

            if (type == TradeRecordType.Limit || type == TradeRecordType.Stop)
                return string.Format("{0} {1}", side, type);

            return side;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
