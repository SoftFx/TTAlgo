using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Api;

namespace BotCommon
{
    internal class OrderKeeper :  IDisposable
    {
        private TradeBot tradeApi;
        private Symbol symbol;
        private OrderSide side;
        private string subBotTag;
        private bool isBusy;
        private bool isUpdated;
        private bool isDisposed;
        private double targetVolume;
        private double targetPrice;

        public OrderKeeper(TradeBot bot, Symbol symbol, OrderSide side, string subBotTag)
        {
            this.tradeApi = bot;
            this.symbol = symbol;
            this.side = side;
            this.subBotTag = subBotTag;

            bot.Account.Orders.Filled += Orders_Filled;
            bot.Account.Orders.Expired += Orders_Expired;
            bot.Account.Orders.Canceled += Orders_Canceled;
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

        private void OnOrderChanged(Order order)
        {
            if (order.Symbol == symbol.Name && order.Comment == subBotTag
                && order.Side == side)
            {
                isUpdated = true;
                AdjustOrderLoop();
            }
        }

        private async void AdjustOrderLoop()
        {
            if (isBusy || isDisposed)
                return;

            isBusy = true;

            try
            {
                while (isUpdated && !isDisposed)
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
                && p.Symbol == symbol.Name && p.Side == side && p.Comment == subBotTag);

            if (orderList.Count() > 1)
            {
                //error case
                foreach (Order o in orderList)
                    await tradeApi.CancelOrderAsync(o.Id);
                return;
            }

            Order order = orderList.FirstOrDefault();
            bool isVolumeValid = volume - symbol.MinTradeVolume > -Double.Epsilon;

            if (order == null)
            {
                if (isVolumeValid)
                    await tradeApi.OpenOrderAsync(symbol.Name, OrderType.Limit, side, volume, price, null, null, subBotTag);
                return;
            }
            if (Math.Abs(volume - order.RemainingVolume) > Double.Epsilon)
            {
                await tradeApi.CancelOrderAsync(order.Id);
                await tradeApi.OpenOrderAsync(symbol.Name, OrderType.Limit, side, volume, price, null, null, subBotTag);
            }
            if (Math.Abs(price - order.Price) > Double.Epsilon)
                await tradeApi.ModifyOrderAsync(order.Id, price);
        }

        private void Orders_Canceled(OrderCanceledEventArgs args)
        {
            OnOrderChanged(args.Order);
        }

        private void Orders_Expired(OrderExpiredEventArgs args)
        {
            OnOrderChanged(args.Order);
        }

        private void Orders_Filled(OrderFilledEventArgs args)
        {
            OnOrderChanged(args.OldOrder);
        }

        public void Dispose()
        {
            if (!isDisposed)
            {
                isDisposed = true;
                tradeApi.Account.Orders.Filled -= Orders_Filled;
                tradeApi.Account.Orders.Expired -= Orders_Expired;
                tradeApi.Account.Orders.Canceled -= Orders_Canceled;
            }
        }
    }
}
