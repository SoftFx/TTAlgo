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

        public void SendOpenOrder(Domain.OpenOrderRequest request)
        {
            _exec.SendOpenOrder(request);
        }

        public void SendCancelOrder(Domain.CancelOrderRequest request)
        {
            _exec.SendCancelOrder(request);
        }

        public void SendModifyOrder(Domain.ModifyOrderRequest request)
        {
            _exec.SendModifyOrder(request);
        }

        public void SendCloseOrder(Domain.CloseOrderRequest request)
        {
            _exec.SendCloseOrder(request);
        }

        #endregion
    }
}
