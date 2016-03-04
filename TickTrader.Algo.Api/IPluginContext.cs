using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.Algo.Api
{
    internal interface IPluginContext
    {
        OrderList GetOrdersCollection();
        PositionList GetPositionsCollection();
    }

    internal interface IPluginActivator
    {
        IPluginContext Activate(AlgoPlugin instance);
    }
}
