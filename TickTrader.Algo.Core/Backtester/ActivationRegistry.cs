using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Api;
using TickTrader.BusinessObjects;

namespace TickTrader.Algo.Core
{
    internal class ActivationRegistry
    {
        private ActivationIndex buyLimitIndex;
        private ActivationIndex buyStopIndex;
        private ActivationIndex buyStopLimitIndex;
        private ActivationIndex sellLimitIndex;
        private ActivationIndex sellStopIndex;
        private ActivationIndex sellStopLimitIndex;

        public ActivationRegistry()
        {
            buyLimitIndex = new ActivationIndex(new DescendingPriceComparer(), (ordPrice, rate) => ordPrice >= rate, (tick) => tick.Ask.PriceToDecimal(), agg => agg.AskLow.ToDecimal());
            buyStopIndex = new ActivationIndex(new AscendingPriceComparer(), (ordPrice, rate) => ordPrice <= rate, (tick) => tick.Ask.PriceToDecimal(), agg => agg.AskHigh.ToDecimal());
            buyStopLimitIndex = new ActivationIndex(new AscendingPriceComparer(), (ordPrice, rate) => ordPrice <= rate, (tick) => tick.Ask.PriceToDecimal(), agg => agg.AskHigh.ToDecimal());
            sellLimitIndex = new ActivationIndex(new AscendingPriceComparer(), (ordPrice, rate) => ordPrice <= rate, (tick) => tick.Bid.PriceToDecimal(), agg => agg.BidHigh.ToDecimal());
            sellStopIndex = new ActivationIndex(new DescendingPriceComparer(), (ordPrice, rate) => ordPrice >= rate, (tick) => tick.Bid.PriceToDecimal(), agg => agg.BidLow.ToDecimal());
            sellStopLimitIndex = new ActivationIndex(new DescendingPriceComparer(), (ordPrice, rate) => ordPrice >= rate, (tick) => tick.Bid.PriceToDecimal(), agg => agg.BidLow.ToDecimal());
        }

        public ActivationRecord AddOrder(OrderAccessor order, RateUpdate currentRate)
        {
            ActivationRecord instantActivation = null;

            if (order.Type == OrderType.Limit || order.Type == OrderType.Stop || order.Type == OrderType.StopLimit)
            {
                ActivationRecord record = new ActivationRecord(order, ActivationTypes.Pending);
                ActivationIndex index = GetPendingIndex(order.Type, order.Side);
                if (index.AddRecord(record, currentRate))
                    instantActivation = record;
            }
            else if (order.Type == OrderType.Position)
            {
                if (order.Entity.TakeProfit != null)
                {
                    ActivationRecord record = new ActivationRecord(order, ActivationTypes.TakeProfit);
                    ActivationIndex index = GetPositionIndex(order.Side, ActivationTypes.TakeProfit);
                    if (index.AddRecord(record, currentRate))
                        instantActivation = record;
                }
                if (order.Entity.StopLoss != null)
                {
                    ActivationRecord record = new ActivationRecord(order, ActivationTypes.StopLoss);
                    ActivationIndex index = GetPositionIndex(order.Side, ActivationTypes.StopLoss);
                    if (index.AddRecord(record, currentRate))
                        instantActivation = record;
                }
            }
            else
                throw new Exception("Invalid order type:" + order.Type);

            return instantActivation;
        }

        private ActivationIndex GetPendingIndex(OrderType ordType, OrderSide ordSide)
        {
            if (ordType == OrderType.Limit)
            {
                if (ordSide == OrderSide.Buy)
                    return buyLimitIndex;
                else
                    return sellLimitIndex;
            }
            else if (ordType == OrderType.Stop)
            {
                if (ordSide == OrderSide.Buy)
                    return buyStopIndex;
                else
                    return sellStopIndex;
            }
            else if (ordType == OrderType.StopLimit)
            {
                if (ordSide == OrderSide.Buy)
                    return buyStopLimitIndex;
                else
                    return sellStopLimitIndex;
            }
            else
                throw new Exception("Unsupported Order Type");
        }

        private ActivationIndex GetPositionIndex(OrderSide side, ActivationTypes type)
        {
            if (type == ActivationTypes.TakeProfit)
            {
                if (side == OrderSide.Sell)
                    return buyLimitIndex;
                else
                    return sellLimitIndex;
            }
            else if (type == ActivationTypes.StopLoss)
            {
                if (side == OrderSide.Sell)
                    return buyStopIndex;
                else
                    return sellStopIndex;
            }
            else
                throw new Exception("Unsupported Activation Type");
        }

        internal bool RemoveOrder(OrderAccessor order)
        {
            bool result = false;

            if (order.Type == OrderType.Limit || order.Type == OrderType.Stop || order.Type == OrderType.StopLimit)
            {
                ActivationIndex index = GetPendingIndex(order.Type, order.Side);
                result |= index.RemoveOrder(order, ActivationTypes.Pending);
            }
            else if (order.Type == OrderType.Position)
            {
                if (order.Entity.TakeProfit != null)
                {
                    ActivationIndex index = GetPositionIndex(order.Side, ActivationTypes.TakeProfit);
                    result |= index.RemoveOrder(order, ActivationTypes.TakeProfit);
                }
                if (order.Entity.StopLoss != null)
                {
                    ActivationIndex index = GetPositionIndex(order.Side, ActivationTypes.StopLoss);
                    result |= index.RemoveOrder(order, ActivationTypes.StopLoss);
                }
            }
            else if (order.Type != OrderType.Market)
                throw new Exception("Invalid order type:" + order.Type);

            return result;
        }

        public void ResetOrderActivation(OrderAccessor order)
        {
            if (order.Type == OrderType.Limit || order.Type == OrderType.Stop || order.Type == OrderType.StopLimit)
            {
                ActivationIndex index = GetPendingIndex(order.Type, order.Side);
                index.ResetOrderActivation(order, ActivationTypes.Pending);
            }
            else if (order.Type == OrderType.Position)
            {
                if (order.TakeProfit != null)
                {
                    ActivationIndex index = GetPositionIndex(order.Side, ActivationTypes.TakeProfit);
                    index.ResetOrderActivation(order, ActivationTypes.TakeProfit);
                }
                if (order.StopLoss != null)
                {
                    ActivationIndex index = GetPositionIndex(order.Side, ActivationTypes.StopLoss);
                    index.ResetOrderActivation(order, ActivationTypes.StopLoss);
                }
            }
        }

        public List<ActivationRecord> CheckPendingOrders(RateUpdate rate)
        {
            List<ActivationRecord> result = new List<ActivationRecord>();
            if (!double.IsNaN(rate.Ask))
            {
                result.AddRange(buyLimitIndex.CheckPendingOrders(rate));
                result.AddRange(buyStopIndex.CheckPendingOrders(rate));
                result.AddRange(buyStopLimitIndex.CheckPendingOrders(rate));
            }
            if (!double.IsNaN(rate.Bid))
            {
                result.AddRange(sellLimitIndex.CheckPendingOrders(rate));
                result.AddRange(sellStopIndex.CheckPendingOrders(rate));
                result.AddRange(sellStopLimitIndex.CheckPendingOrders(rate));
            }
            return result;
        }

        private class AscendingPriceComparer : IComparer<decimal>
        {
            public int Compare(decimal x, decimal y)
            {
                return decimal.Compare(x, y);
            }
        }

        private class DescendingPriceComparer : IComparer<decimal>
        {
            public int Compare(decimal x, decimal y)
            {
                return decimal.Compare(y, x);
            }
        }
    }
}
