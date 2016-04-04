using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.Algo.Api
{
    internal interface IPluginDataProvider
    {
        OrderList GetOrdersCollection();
        PositionList GetPositionsCollection();
        MarketSeries GetMainMarketSeries();
        Leve2Series GetMainLevel2Series();
    }

    internal interface IPluginActivator
    {
        IPluginDataProvider Activate(AlgoPlugin instance);
    }
}
