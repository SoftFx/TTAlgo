using System;
using System.Linq;
using TickTrader.Algo.Api;

namespace TickTrader.Algo.TestCollection.Bots
{
    [TradeBot(DisplayName = "Open Close Loop Bot", Version = "1.0", Category = "Test Orders",
              Description = "Performs infinite open-close loop with specified delay between operations. Supports Net and Gross accounts.")]
    public class OpenCloseBot : TradeBot
    {
        private OrderSide _lastSide;

        [Parameter(DefaultValue = 0.1)]
        public double Volume { get; set; }

        [Parameter(DefaultValue = 1000)]
        public int OperationDelay { get; set; }


        protected override void Init()
        {
            if (Account.Type != AccountTypes.Cash)
                CreateTimer(OperationDelay, t =>
                {
                    if (Account.Type == AccountTypes.Net)
                    {
                        var pos = Account.NetPositions.FirstOrDefault(p => p.Volume > 0);

                        if (pos != null)
                            OpenOrder(pos.Symbol, OrderType.Market, Revert(pos.Side), Volume);
                        else
                            OpenNewPos();
                    }
                    else if (Account.Type == AccountTypes.Gross)
                    {
                        var pos = Account.Orders.FirstOrDefault(o => o.Type == OrderType.Position);

                        if (pos == null)
                            OpenNewPos();
                        else
                            CloseOrder(pos.Id);
                    }
                });
            else
                throw new Exception($"Bot support only Net and Gross account types");
        }


        private void OpenNewPos()
        {
            _lastSide = Revert(_lastSide);
            OpenMarketOrder(_lastSide, Volume);
        }

        private OrderSide Revert(OrderSide side)
        {
            if (side == OrderSide.Buy)
                return OrderSide.Sell;
            else
                return OrderSide.Buy;
        }
    }
}
