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

        public static string FormatCurrency(decimal value, int precision)
        {
            return value.ToString("N" + precision, AmountNumberInfo);
        }

        public static string FormatPrice(decimal value, int precision)
        {
            return value.ToString("F" + precision);
        }

        public static string FormatPrecente(decimal value, int precision)
        {
            return value.ToString("P" + precision, AmountNumberInfo);
        }
    }
}
