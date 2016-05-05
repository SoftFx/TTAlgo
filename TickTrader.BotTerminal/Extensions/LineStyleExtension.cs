using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Api;

namespace TickTrader.BotTerminal
{
    internal static class LineStylesExtension
    {
        public static double[] ToStrokeDashArray(this LineStyles lineStyle)
        {
            switch (lineStyle)
            {
                case LineStyles.Dots:
                    return new double[] { 1.5, 2 };
                case LineStyles.DotsRare:
                    return new double[] { 1.5, 4 };
                case LineStyles.DotsVeryRare:
                    return new double[] { 1.5, 8 };
                case LineStyles.Lines:
                    return new double[] { 8, 6 };
                case LineStyles.LinesDots:
                    return new double[] { 5, 2, 1.5, 2 };
                default: return new double[0];
            }
        }
    }
}
