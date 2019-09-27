using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.BotTerminal
{
    public static class NumberFormat
    {
        public static readonly NumberFormatInfo AmountNumberInfo = new NumberFormatInfo() { NumberGroupSeparator = " " };

        public static char? GetCurrencySymbol(string currencyName)
        {
            switch (currencyName.ToLower())
            {
                case "usd": return '$';
                case "eur": return '€';
                case "jpy": return '¥';
                case "gbp": return '£';
                default: return null;
            }
        }

        public static string GetCurrencyFormatString(int precision, string currency)
        {
            var sign = GetCurrencySymbol(currency);

            if (sign != null)
                return sign.Value + " {0:N" + precision + "}";
            else
                return "{0:N" + precision + "} " + currency;
        }

        public static string FormatCurrency(decimal value, int precision) => GetFormattedDecimal(value, "N", precision, AmountNumberInfo);

        public static string FormatPrice(decimal value, int precision) => GetFormattedDecimal(value, "F", precision);

        public static string FormatPrice(double value, int precision) => GetFormattedDouble(value, "F", precision);

        public static string FormatPrice(double? value, int precision) => value == null ? value.ToString() : FormatPrice(value.Value, precision);

        public static string FormatPrecente(decimal value, int precision) => GetFormattedDecimal(value, "P", precision, AmountNumberInfo);

        public static string FormatCurrency(double value, int precision, bool suppressNaN = true) => suppressNaN && double.IsNaN(value) ? "" : GetFormattedDouble(value, "N", precision, AmountNumberInfo);

        public static string FormatPrice(double value, int precision, bool suppressNaN = true) => suppressNaN && double.IsNaN(value) ? "" : GetFormattedDouble(value, "F", precision);

        public static string FormatPrecente(double value, int precision, bool suppressNaN = true) => suppressNaN && double.IsNaN(value) ? "" : GetFormattedDouble(value, "P", precision, AmountNumberInfo);

        private static string GetFormattedDouble(double value, string type, int precision, NumberFormatInfo info = null)
        {
            if (info == null)
                return precision >= 0 ? value.ToString($"{type}{precision}") : value.ToString();
            else
                return precision >= 0 ? value.ToString($"{type}{precision}", info) : value.ToString(info);
        }

        private static string GetFormattedDecimal(decimal value, string type, int precision, NumberFormatInfo info = null)
        {
            if (info == null)
                return precision >= 0 ? value.ToString($"{type}{precision}") : value.ToString();
            else
                return precision >= 0 ? value.ToString($"{type}{precision}", info) : value.ToString(info);
        }
    }
}
