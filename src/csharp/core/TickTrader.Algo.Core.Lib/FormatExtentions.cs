using System.Globalization;
using System.Text;

namespace TickTrader.Algo.Core.Lib
{
    public static class FormatExtentions
    {
        public static NumberFormatInfo CreateTradeFormatInfo(int digits) => new NumberFormatInfo { NumberDecimalDigits = digits };

        public static string FormatPlain(this decimal number, NumberFormatInfo format)
        {
            return number.ToString("N", format);
        }

        public static string FormatPlain(this double number, NumberFormatInfo format)
        {
            return number.ToString("N", format);
        }

        public static StringBuilder AppendNumber(this StringBuilder builder, double number, string customFormat)
        {
            return builder.Append(number.ToString(customFormat));
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
