using TickTrader.Algo.Api;
using TickTrader.Algo.CoreV1;

namespace TickTrader.Algo.Backtester
{
    internal class OpenOrderDealerRequest: Api.Ext.OpenOrderRequest
    {
        public OpenOrderDealerRequest(OrderAccessor order, Quote currentRate)
        {
            Order = order;
            CurrentRate = currentRate;
            Confirmed = true;
        }

        public Order Order { get; }
        public Quote CurrentRate { get; }

        public bool Confirmed { get; private set; }
        public decimal? DealerAmount { get; private set; }
        public double? DealerPrice { get; private set; }

        public void Confirm()
        {
            Confirmed = true;
        }

        public void Confirm(decimal amount, double price)
        {
            DealerAmount = amount;
            DealerPrice = price;
            Confirmed = true;
        }

        public void Reject()
        {
            Confirmed = false;
        }
    }
}
