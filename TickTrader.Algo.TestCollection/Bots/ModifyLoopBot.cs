using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Api;

namespace TickTrader.Algo.TestCollection.Bots
{
    [TradeBot(DisplayName = "[T] Modify Loop Bot", Version = "1.0", Category = "Test Orders",
        Description = "")]
    public class ModifyLoopBot : TradeBot
    {
        [Parameter(DefaultValue = 10)]
        public double ParallelOrders { get; set; }

        [Parameter(DefaultValue = OrderModifyTypes.Limit)]
        public OrderModifyTypes BotOrderType { get; set; }

        [Parameter(DefaultValue = 0.1D)]
        public double Volume { get; set; }

        [Parameter(DefaultValue = null, IsRequired = false)]
        public double? VolumeChangeMultiplier {
            get { return VolumeChangeMultiplier; }
            set {
                if (value < 0)
                {
                    PrintError("Invalid negative volume.");
                    Exit();
                }
                else
                    VolumeChangeMultiplier = value;
            }
        }

        [Parameter(DefaultValue = 1000)]
        public double PriceDelta { get; set; }

        protected override void OnStart()
        {
            for (int i = 0; i < ParallelOrders; i++)
                KeepOrder(i, OrderSide.Buy, Volume);
        }

        private async void KeepOrder(int orderTag, OrderSide orderSide, double orderVolume)
        {
            var strTag = "LimitModifyLoop" + orderTag; 

            while (true)
            {
                var existing = Account.Orders.FirstOrDefault(o => o.Comment == strTag && o.Type == OrderType.Limit);

                if (existing != null)
                {
                    var newPrice = GetNewPrice(orderSide);
                    await ModifyOrderAsync(existing.Id, newPrice, null, null, strTag);
                }
                else
                {
                    var newPrice = GetNewPrice(orderSide);
                    await OpenOrderAsync(Symbol.Name, OrderType.Limit, orderSide, orderVolume, newPrice, null, null, strTag);
                }
            }
        }

        private double GetNewPrice(OrderSide side)
        {
            var ordPrice = GetCurrentPrice(side);
            if (side == OrderSide.Buy)
                ordPrice -= Symbol.Point * PriceDelta;
            else
                ordPrice += Symbol.Point * PriceDelta;
            return ordPrice;
        }

        protected double GetCurrentPrice(OrderSide side)
        {
            return GetCurrentPrice(side == OrderSide.Buy ? BarPriceType.Ask : BarPriceType.Bid);
        }

        private double GetCurrentPrice(BarPriceType type)
        {
            return type == BarPriceType.Ask ? Symbol.Ask : Symbol.Bid;
        }
    }

    public enum OrderModifyTypes { Limit, Stop, StopLimit }
}
