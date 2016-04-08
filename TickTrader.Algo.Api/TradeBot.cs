using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.Algo.Api
{
    public class TradeBot : AlgoPlugin
    {
        protected virtual void OnStart() { }
        protected virtual void OnStop() { }

        internal void InvokeStart()
        {
            OnStart();
        }

        internal void InvokeStop()
        {
            OnStop();
        }
    }
}
