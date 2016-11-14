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

    internal class NumberConverter : IMultiValueConverter, IValueConverter
    {
        public NumberType Type { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || parameter == null)
                return "";

            return Convert(new object[] { value, parameter }, targetType, null, culture);
        }

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values == null || values.Length < 2 && values.Length > 3)
                return "";

            if ((values[0] is decimal || values[0] is double) && values[1] is int)
            {
                int precision = (int)values[1];
                var formattedNumber = FormatNumber(values[0], precision);
                return values.Length == 3 ? $"{formattedNumber} {values[2]}" : formattedNumber;
            }

            return "";
        }

        private string FormatNumber(object number, int precision)
        {
            switch (Type)
            {
                case NumberType.Currency:
                    if (number is decimal)
                        return NumberFormat.FormatCurrency((decimal)number, precision);
                    if (number is double)
                        return NumberFormat.FormatCurrency((double)number, precision);
                    break;
                case NumberType.Price:
                    if (number is decimal)
                        return NumberFormat.FormatPrice((decimal)number, precision);
                    if (number is double)
                        return NumberFormat.FormatPrice((double)number, precision);
                    break;
                case NumberType.Precent:
                    if (number is decimal)
                        return NumberFormat.FormatPrecente((decimal)number, precision);
                    if (number is double)
                        return NumberFormat.FormatPrecente((double)number, precision);
                    break;
            }

            return number is decimal ? ((decimal)number).ToString() : ((double)number).ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
