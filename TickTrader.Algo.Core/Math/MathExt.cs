using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.Algo.Core
{
    public static class MathExt
    {
        public static bool IsPrecisionGreater(this decimal f, int digits)
        {
            return System.Math.Round(f, digits) != f;
        }
    }
}
