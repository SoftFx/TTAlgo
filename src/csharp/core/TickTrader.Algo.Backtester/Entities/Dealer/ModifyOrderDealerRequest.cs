using TickTrader.Algo.Api;
using TickTrader.Algo.CoreV1;

namespace TickTrader.Algo.Backtester
{
    internal class ModifyOrderDealerRequest : Api.Ext.ModifyOrderRequest
    {
        public ModifyOrderDealerRequest(OrderAccessor order, Quote currentRate)
        {
            Order = order;
            CurrentRate = currentRate;
            Confirmed = true;
        }

        public double? NewVolume { get; set; }
        public double? NewPrice { get; set; }
        public double? NewStopPrice { get; set; }
        public string NewComment { get; set; }
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
