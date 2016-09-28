using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace TickTrader.BotTerminal
{
    internal enum NumberType
    {
        /// <summary>
        /// Format open/close/current price etc
        /// </summary>
        Price,
        ///// <summary>
        ///// Format order/position/asset amount
        ///// </summary>
        //Amount,
        /// <summary>
        /// Format profit/loss/commision/equity/balance etc
        /// </summary>
        Currency,
        /// <summary>
        /// Format margin level and other precents
        /// </summary>
        Precent
    }

    internal class NumberConverter : IMultiValueConverter
    {
        public NumberType Type { get; set; }

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values == null || values.Length != 2)
                return "";

            if (values[0] is decimal && values[1] is int)
            {
                decimal value = (decimal)values[0];
                int precision = (int)values[1];

                if (Type == NumberType.Currency)
                    return NumberFormat.FormatCurrency(value, precision);
                else if (Type == NumberType.Price)
                    return NumberFormat.FormatPrice(value, precision);
                else if (Type == NumberType.Precent)
                    return NumberFormat.FormatPrecente(value, precision);
            }

            return "";
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
