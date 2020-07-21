namespace TickTrader.Algo.Core
{
    public class CloseOrderRequestContext
    {
        public Domain.CloseOrderRequest Request { get; } = new Domain.CloseOrderRequest();

        public string OrderId
        {
            get { return Request.OrderId; }
            set { Request.OrderId = value; }
        }

        public double? Amount
        {
            get { return Request.Amount; }
            set { Request.Amount = value; }
        }

        public double? Slippage
        {
            get { return Request.Slippage; }
            set { Request.Slippage = value; }
        }

        public string ByOrderId
        {
            get { return Request.ByOrderId; }
            set { Request.ByOrderId = value; }
        }

        public double? VolumeLots { get; set; }
    }
}
