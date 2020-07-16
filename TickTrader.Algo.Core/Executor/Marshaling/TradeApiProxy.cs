using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Api;
using TickTrader.Algo.Core.Lib;

namespace TickTrader.Algo.Core
{
    public class TradeApiProxy : CrossDomainObject, ITradeExecutor
    {
        private ITradeExecutor _exec;

        public TradeApiProxy(ITradeExecutor executor)
        {
            _exec = executor;
        }

        #region ITradeExecutor

        public void SendOpenOrder(ICallback<Domain.OrderExecReport.Types.CmdResultCode> callback, OpenOrderCoreRequest request)
        {
            _exec.SendOpenOrder(callback, request);
        }

        public void SendCancelOrder(ICallback<Domain.OrderExecReport.Types.CmdResultCode> callback, CancelOrderRequest request)
        {
            _exec.SendCancelOrder(callback, request);
        }

        public void SendModifyOrder(ICallback<Domain.OrderExecReport.Types.CmdResultCode> callback, ReplaceOrderCoreRequest request)
        {
            _exec.SendModifyOrder(callback, request);
        }

        public void SendCloseOrder(ICallback<Domain.OrderExecReport.Types.CmdResultCode> callback, CloseOrderCoreRequest request)
        {
            _exec.SendCloseOrder(callback, request);
        }

        #endregion
    }
}
