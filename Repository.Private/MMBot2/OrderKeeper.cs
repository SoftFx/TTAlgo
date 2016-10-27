using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Api;

namespace MMBot2
{
    internal class OrderKeeper
    {
        private TradeBot tradeApi;
        private Symbol symbol;
        private OrderSide side;
        private string orderTag;
        private bool isBusy;
        private bool isUpdated;
        private double targetVolume;
        private double targetPrice;

        public OrderKeeper(TradeBot bot, Symbol symbol, OrderSide side, string orderTag)
        {
            this.tradeApi = bot;
            this.symbol = symbol;
            this.side = side;
            this.orderTag = orderTag;
        }

        public void SetTarget(double volume, double price)
        {
            if (targetPrice != price || targetVolume != volume)
            {
                targetPrice = price;
                targetVolume = volume;
                isUpdated = true;
                AdjustOrderLoop();
            }
        }

        private async void AdjustOrderLoop()
        {
            if (isBusy)
                return;

            isBusy = true;

            try
            {
                while (isUpdated)
                {
                    isUpdated = false;
                    await AdjustOrder(targetVolume, targetPrice);
                }
            }
            finally
            {
                isBusy = false;
            }
        }

        private async Task AdjustOrder(double volume, double price)
        {
            IEnumerable<Order> orderList = tradeApi.Account.Orders.Where(p => p.Type == OrderType.Limit
                && p.Symbol == symbol.Name && p.Side == side && p.Comment == orderTag);

            if (orderList.Count() > 1)
            {
                //error case
                foreach (Order o in orderList)
                    await tradeApi.CancelOrderAsync(o.Id);
                return;
            }

            Order order = orderList.FirstOrDefault();

            if (order == null)
            {
                await tradeApi.OpenOrderAsync(symbol.Name, OrderType.Limit, side, volume / symbol.ContractSize, price, null, null, orderTag);
                return;
            }
            else if (volume != order.RemainingVolume)
            {
                await tradeApi.CancelOrderAsync(order.Id);
                await tradeApi.OpenOrderAsync(symbol.Name, OrderType.Limit, side, volume / symbol.ContractSize, price, null, null, orderTag);
            }
            else if (price != order.Price)
                await tradeApi.ModifyOrderAsync(order.Id, price);
        }
    }
}
