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
        internal static IPluginActivator activator;

        private IPluginDataProvider context;
        private MarketSeries mainMarketSeries;
        private Leve2Series mainLevel2Series;
        private OrderList orders;
        private PositionList positions;

        internal AlgoPlugin()
        {
            if (activator == null)
                throw new InvalidOperationException("Algo context is not initialized!");

            this.context = activator.Activate(this);
        }

        protected MarketSeries MainMarketSeries
        {
            get
            {
                if (mainMarketSeries == null)
                    mainMarketSeries = context.GetMainMarketSeries();
                return mainMarketSeries;
            }
        }

        protected Leve2Series MainLevel2Series
        {
            get
            {
                if (mainLevel2Series != null)
                    mainLevel2Series = context.GetMainLevel2Series();
                return mainLevel2Series;
            }
        }

        protected OrderList Orders
        {
            get
            {
                if (orders == null)
                    orders = context.GetOrdersCollection();
                return orders;
            }
        }

        protected PositionList Positions
        {
            get
            {
                if (positions == null)
                    positions =  context.GetPositionsCollection();
                return positions;
            }
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
