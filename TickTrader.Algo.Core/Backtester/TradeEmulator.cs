using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Api;
using TickTrader.Algo.Core.Lib;

namespace TickTrader.Algo.Core
{
    internal class TradeEmulator : CrossDomainObject, ITradeExecutor
    {
        public TradeEmulator(CalculatorFixture calc, BacktesterCollector collector)
        {
        }

        void ITradeExecutor.SendCancelOrder(CrossDomainCallback<OrderCmdResultCodes> callback, CancelOrderRequest request)
        {
            callback.Invoke(OrderCmdResultCodes.Ok);
        }

        void ITradeExecutor.SendCloseOrder(CrossDomainCallback<OrderCmdResultCodes> callback, CloseOrderRequest request)
        {
            callback.Invoke(OrderCmdResultCodes.Ok);
        }

        void ITradeExecutor.SendModifyOrder(CrossDomainCallback<OrderCmdResultCodes> callback, ReplaceOrderRequest request)
        {
            callback.Invoke(OrderCmdResultCodes.Ok);
        }

        void ITradeExecutor.SendOpenOrder(CrossDomainCallback<OrderCmdResultCodes> callback, OpenOrderRequest request)
        {
            callback.Invoke(OrderCmdResultCodes.Ok);
        }
    }
}
