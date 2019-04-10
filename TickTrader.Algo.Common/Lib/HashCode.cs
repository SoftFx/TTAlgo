using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.Algo.Common.Lib
{
    public static class HashCode
    {
        public static int Combine(int h1, int h2)
        {
            return (((h1 << 5) + h1) ^ h2);
        }

        public static int Combine(int h1, int h2, int h3)
        {
            return Combine(Combine(h1, h2), h3);
        }

        public static int Combine(int h1, int h2, int h3, int h4)
        {
            return Combine(Combine(h1, h2), Combine(h3, h4));
        }

        public static int Combine(int h1, int h2, int h3, int h4, int h5)
        {
            return Combine(Combine(h1, h2, h3, h4), h5);
        }

        public static int Combine(object h1, object h2)
        {
            return Combine(h1.GetHashCode(), h2.GetHashCode());
        }

        public static int Combine(object h1, object h2, object h3)
        {
            return Combine(h1.GetHashCode(), h2.GetHashCode(), h3.GetHashCode());
        }

        public static int Combine(object h1, object h2, object h3, object h4)
        {
            return Combine(h1.GetHashCode(), h2.GetHashCode(), h3.GetHashCode(), h4.GetHashCode());
        }
    }
}
