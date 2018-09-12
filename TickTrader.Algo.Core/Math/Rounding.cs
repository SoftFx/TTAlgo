using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.Algo.Core
{
    public static class Rounding
    {
        private static decimal[] decMap = new decimal[] {1, 10, 100, 1000, 1000, 10000, 100000, 10000000, 100000000, 100000000, 100000000, 100000000, 100000000 };

        public static decimal CeilBy(this decimal val, int decimals)
        {
            var decimalPart = decimal.Truncate(val);
            var fractionalPart = val - decimal.Truncate(val);
            var multiplier = decMap[decimals];
            var roundedFrPart = decimal.Ceiling(fractionalPart * multiplier) / multiplier;

            return decimalPart + roundedFrPart;
        }

        public static decimal FloorBy(this decimal val, int decimals)
        {
            var decimalPart = decimal.Truncate(val);
            var fractionalPart = val - decimal.Truncate(val);
            var multiplier = decMap[decimals];
            var roundedFrPart = decimal.Floor(fractionalPart * multiplier) / multiplier;

            return decimalPart + roundedFrPart;
        }
    }
}
