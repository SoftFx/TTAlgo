using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.Algo.Api
{
    public abstract class AlgoPlugin
    {
        [ThreadStatic]
        internal static IAlgoActivator activator;

        internal AlgoPlugin()
        {
            if (activator == null)
                throw new InvalidOperationException("Algo context is not initialized!");
            activator.Activate(this);
        }

        protected virtual void Init() { }

        internal void InvokeInit()
        {
            Init();
        }

        //internal virtual void DoInit()
        //{
        //    currentInstance = this;

        //    try
        //    {
        //        Init();

        //        foreach (Indicator i in nestedIndicators)
        //            i.DoInit();
        //    }
        //    finally
        //    {
        //        currentInstance = null;
        //    }
        //}
    }
}
