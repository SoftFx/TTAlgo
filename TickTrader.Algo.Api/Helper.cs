using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.Algo.Api
{
    public static class Helper
    {
        public static string FormatPrice(double price, int digits)
        {
            return price.ToString("F" + digits);
        }

        public static string FormatPrice(this Symbol symbolInfo, double price)
        {
            return FormatPrice(price, symbolInfo.Digits);
        }

        public static string FormatPrice(double price, Symbol symbolInfo)
        {
            return FormatPrice(price, symbolInfo.Digits);
        }
    }
}
