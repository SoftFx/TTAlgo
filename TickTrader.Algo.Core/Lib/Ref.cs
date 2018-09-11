using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.Algo.Core
{
    public static class Ref
    {
        public static void Swap<T>(ref T ref1, ref T ref2)
        {
            var ref1Copy = ref1;
            ref1 = ref2;
            ref2 = ref1Copy;
        }
    }
}
