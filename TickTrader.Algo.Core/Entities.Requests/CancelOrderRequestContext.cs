namespace TickTrader.Algo.Core
{
    public class CancelOrderRequestContext
    {
        public Domain.CancelOrderRequest Request { get; } = new Domain.CancelOrderRequest();

        public string OrderId
        {
            get { return Request.OrderId; }
            set { Request.OrderId = value; }
        }
    }
}
