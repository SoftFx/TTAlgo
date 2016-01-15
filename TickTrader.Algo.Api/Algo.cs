using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.Algo.Api
{
    public abstract class Algo
    {
        [ThreadStatic]
        internal static Algo currentInstance;

        internal List<Indicator> nestedIndicators = new List<Indicator>();

        internal Algo()
        {
        }

        protected virtual void Init() { }

        internal virtual void DoInit()
        {
            currentInstance = this;

            try
            {
                Init();

                foreach (Indicator i in nestedIndicators)
                    i.DoInit();
            }
            finally
            {
                currentInstance = null;
            }
        }
    }
}
