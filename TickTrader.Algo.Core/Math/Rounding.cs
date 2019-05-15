using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.Algo.Core
{
    public static class Rounding
    {
        private static double[] decMap = new double[]
            {
                1,
                10,
                100,
                1000,
                10000,
                100000,
                1000000,
                10000000,
                100000000,
                1000000000,
                10000000000,
                100000000000,
                1000000000000,
            };

        public static double CeilBy(this double val, int decimals)
        {
            var decimalPart = Math.Truncate(val);
            var fractionalPart = val - Math.Truncate(val);
            var multiplier = decMap[decimals];
            var roundedFrPart = Math.Ceiling(fractionalPart * multiplier) / multiplier;

            return decimalPart + roundedFrPart;
        }

        public static double FloorBy(this double val, int decimals)
        {
            var decimalPart = Math.Truncate(val);
            var fractionalPart = val - Math.Truncate(val);
            var multiplier = decMap[decimals];
            var roundedFrPart = Math.Floor(fractionalPart * multiplier) / multiplier;

            return decimalPart + roundedFrPart;
        }
    }
}
