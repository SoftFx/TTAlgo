using TickTrader.Algo.Api;
using TickTrader.Algo.CoreV1;

namespace TickTrader.Algo.Backtester
{
    internal class CancelOrderDealerRequest : Api.Ext.CancelOrderRequest
    {
        public CancelOrderDealerRequest(OrderAccessor order, Quote currentRate)
        {
            Order = order;
            CurrentRate = currentRate;
            Confirmed = true;
        }

        public Order Order { get; }
        public Quote CurrentRate { get; }

        public bool Confirmed { get; private set; }

        public void Confirm()
        {
            Confirmed = true;
        }

        public void Reject()
        {
            Confirmed = false;
        }
    }
}
