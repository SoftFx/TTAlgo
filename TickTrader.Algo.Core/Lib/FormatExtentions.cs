using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.Algo.Core.Lib
{
    public static class FormatExtentions
    {
        public static NumberFormatInfo CreateTradeFormatInfo(int digits)
        {
            var format = new NumberFormatInfo();
            format.NumberDecimalDigits = digits;
            return format;
        }

        public static StringBuilder AppendNumber(this StringBuilder builder, double number, NumberFormatInfo format)
        {
            return builder.Append(number.ToString("N", format));
        }

        public static StringBuilder AppendNumber(this StringBuilder builder, double number)
        {
            return builder.Append(number.ToString("N"));
        }

        public static StringBuilder AppendNumber(this StringBuilder builder, decimal number, NumberFormatInfo format)
        {
            return builder.Append(number.ToString("N", format));
        }

        public static StringBuilder AppendNumber(this StringBuilder builder, decimal number)
        {
            return builder.Append(number.ToString("N"));
        }
    }
}
