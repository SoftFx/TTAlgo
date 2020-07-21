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

        public void SendOpenOrder(ICallback<Domain.OrderExecReport.Types.CmdResultCode> callback, Domain.OpenOrderRequest request)
        {
            _exec.SendOpenOrder(callback, request);
        }

        public void SendCancelOrder(ICallback<Domain.OrderExecReport.Types.CmdResultCode> callback, Domain.CancelOrderRequest request)
        {
            _exec.SendCancelOrder(callback, request);
        }

        public void SendModifyOrder(ICallback<Domain.OrderExecReport.Types.CmdResultCode> callback, Domain.ModifyOrderRequest request)
        {
            _exec.SendModifyOrder(callback, request);
        }

        public void SendCloseOrder(ICallback<Domain.OrderExecReport.Types.CmdResultCode> callback, Domain.CloseOrderRequest request)
        {
            _exec.SendCloseOrder(callback, request);
        }

        #endregion
    }
}
