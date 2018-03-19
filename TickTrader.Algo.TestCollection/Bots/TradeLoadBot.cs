using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Api;

namespace TickTrader.Algo.TestCollection.Bots
{
    [TradeBot(DisplayName = "[T] Trade Load Bot", Version = "1.0", Category = "Test Orders",
        Description = "")]
    public class TradeLoadBot : TradeBot
    {
        private int modifyCounter;

        [Parameter]
        public OrderLoadTypes Type { get; set; }

        [Parameter(DefaultValue = 10)]
        public double ParallelOrders { get; set; }

        [Parameter(DefaultValue = 0.1D)]
        public double Volume { get; set; }

        [Parameter(DefaultValue = 1000)]
        public double PriceDelta { get; set; }

        protected override void OnStart()
        {
            for (int i = 0; i < ParallelOrders; i++)
            {
                if (Type == OrderLoadTypes.Modify)
                    ModifyLoop(i, OrderSide.Buy, Volume);
                else if (Type == OrderLoadTypes.OpenCancel)
                    OpenCancelLoop(i, OrderSide.Buy, Volume);
            }

            CreateTimer(TimeSpan.FromSeconds(1), t =>
            {
                Status.Write("{0} operations per second", modifyCounter);
                modifyCounter = 0;
            });
        }

        private async void ModifyLoop(int orderTag, OrderSide orderSide, double orderVolume)
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

                modifyCounter++;
            }
        }

        private async void OpenCancelLoop(int orderTag, OrderSide orderSide, double orderVolume)
        {
            var strTag = "LimitModifyLoop" + orderTag;

            while (true)
            {
                var existing = Account.Orders.FirstOrDefault(o => o.Comment == strTag && o.Type == OrderType.Limit);

                if (existing != null)
                {
                    var newPrice = GetNewPrice(orderSide);
                    await CancelOrderAsync(existing.Id);
                }
                else
                {
                    var newPrice = GetNewPrice(orderSide);
                    await OpenOrderAsync(Symbol.Name, OrderType.Limit, orderSide, orderVolume, newPrice, null, null, strTag);
                }

                modifyCounter++;
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

    public enum OrderLoadTypes { Modify, OpenCancel }
}
