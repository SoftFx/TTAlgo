using System;
using System.Collections.Generic;
using TickTrader.Algo.Domain;

namespace TickTrader.Algo.Core
{
    public class ActivationRegistry
    {
        private ActivationIndex buyLimitIndex;
        private ActivationIndex buyStopIndex;
        private ActivationIndex buyStopLimitIndex;
        private ActivationIndex sellLimitIndex;
        private ActivationIndex sellStopIndex;
        private ActivationIndex sellStopLimitIndex;

        public ActivationRegistry()
        {
            buyLimitIndex = new ActivationIndex(new DescendingPriceComparer(), (ordPrice, rate) => ordPrice >= rate, (tick) => tick.Ask, agg => agg.AskLow);
            buyStopIndex = new ActivationIndex(new AscendingPriceComparer(), (ordPrice, rate) => ordPrice <= rate, (tick) => tick.Ask, agg => agg.AskHigh);
            buyStopLimitIndex = new ActivationIndex(new AscendingPriceComparer(), (ordPrice, rate) => ordPrice <= rate, (tick) => tick.Ask, agg => agg.AskHigh);
            sellLimitIndex = new ActivationIndex(new AscendingPriceComparer(), (ordPrice, rate) => ordPrice <= rate, (tick) => tick.Bid, agg => agg.BidHigh);
            sellStopIndex = new ActivationIndex(new DescendingPriceComparer(), (ordPrice, rate) => ordPrice >= rate, (tick) => tick.Bid, agg => agg.BidLow);
            sellStopLimitIndex = new ActivationIndex(new DescendingPriceComparer(), (ordPrice, rate) => ordPrice >= rate, (tick) => tick.Bid, agg => agg.BidLow);
        }

        public int Count { get; private set; }

        public ActivationRecord AddOrder(OrderAccessor order, IRateInfo currentRate)
        {
            bool added = false;
            ActivationRecord instantActivation = null;

            if (order.Type == Domain.OrderInfo.Types.Type.Limit || order.Type == Domain.OrderInfo.Types.Type.Stop || order.Type == Domain.OrderInfo.Types.Type.StopLimit)
            {
                ActivationRecord record = new ActivationRecord(order, ActivationType.Pending);
                ActivationIndex index = GetPendingIndex(order.Type, order.Side);
                if (index.AddRecord(record, currentRate))
                    instantActivation = record;
                added = true;
            }
            else if (order.Type == Domain.OrderInfo.Types.Type.Position)
            {
                if (order.Entity.TakeProfit != null)
                {
                    ActivationRecord record = new ActivationRecord(order, ActivationType.TakeProfit);
                    ActivationIndex index = GetPositionIndex(order.Side, ActivationType.TakeProfit);
                    if (index.AddRecord(record, currentRate))
                        instantActivation = record;
                    added = true;
                }

                if (order.Entity.StopLoss != null)
                {
                    ActivationRecord record = new ActivationRecord(order, ActivationType.StopLoss);
                    ActivationIndex index = GetPositionIndex(order.Side, ActivationType.StopLoss);
                    if (index.AddRecord(record, currentRate))
                        instantActivation = record;
                    added = true;
                }
            }
            else
                throw new Exception("Invalid order type:" + order.Type);

            if (added)
                Count++;

            return instantActivation;
        }

        private ActivationIndex GetPendingIndex(Domain.OrderInfo.Types.Type ordType, Domain.OrderInfo.Types.Side ordSide)
        {
            if (ordType == Domain.OrderInfo.Types.Type.Limit)
            {
                if (ordSide == Domain.OrderInfo.Types.Side.Buy)
                    return buyLimitIndex;
                else
                    return sellLimitIndex;
            }
            else if (ordType == Domain.OrderInfo.Types.Type.Stop)
            {
                if (ordSide == Domain.OrderInfo.Types.Side.Buy)
                    return buyStopIndex;
                else
                    return sellStopIndex;
            }
            else if (ordType == Domain.OrderInfo.Types.Type.StopLimit)
            {
                if (ordSide == Domain.OrderInfo.Types.Side.Buy)
                    return buyStopLimitIndex;
                else
                    return sellStopLimitIndex;
            }
            else
                throw new Exception("Unsupported Order Type");
        }

        private ActivationIndex GetPositionIndex(Domain.OrderInfo.Types.Side side, ActivationType type)
        {
            if (type == ActivationType.TakeProfit)
            {
                if (side == Domain.OrderInfo.Types.Side.Sell)
                    return buyLimitIndex;
                else
                    return sellLimitIndex;
            }
            else if (type == ActivationType.StopLoss)
            {
                if (side == Domain.OrderInfo.Types.Side.Sell)
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

            if (order.Type == Domain.OrderInfo.Types.Type.Limit || order.Type == Domain.OrderInfo.Types.Type.Stop || order.Type == Domain.OrderInfo.Types.Type.StopLimit)
            {
                ActivationIndex index = GetPendingIndex(order.Type, order.Side);
                result |= index.RemoveOrder(order, ActivationType.Pending);
            }
            else if (order.Type == Domain.OrderInfo.Types.Type.Position)
            {
                if (order.Entity.TakeProfit != null)
                {
                    ActivationIndex index = GetPositionIndex(order.Side, ActivationType.TakeProfit);
                    result |= index.RemoveOrder(order, ActivationType.TakeProfit);
                }
                if (order.Entity.StopLoss != null)
                {
                    ActivationIndex index = GetPositionIndex(order.Side, ActivationType.StopLoss);
                    result |= index.RemoveOrder(order, ActivationType.StopLoss);
                }
            }
            else if (order.Type != Domain.OrderInfo.Types.Type.Market)
                throw new Exception("Invalid order type:" + order.Type);

            if (result)
                Count--;

            return result;
        }

        public void ResetOrderActivation(OrderAccessor order)
        {
            if (order.Type == Domain.OrderInfo.Types.Type.Limit || order.Type == Domain.OrderInfo.Types.Type.Stop || order.Type == Domain.OrderInfo.Types.Type.StopLimit)
            {
                ActivationIndex index = GetPendingIndex(order.Type, order.Side);
                index.ResetOrderActivation(order, ActivationType.Pending);
            }
            else if (order.Type == Domain.OrderInfo.Types.Type.Position)
            {
                if (order.TakeProfit.AsNullable() != null)
                {
                    ActivationIndex index = GetPositionIndex(order.Side, ActivationType.TakeProfit);
                    index.ResetOrderActivation(order, ActivationType.TakeProfit);
                }
                if (order.StopLoss.AsNullable() != null)
                {
                    ActivationIndex index = GetPositionIndex(order.Side, ActivationType.StopLoss);
                    index.ResetOrderActivation(order, ActivationType.StopLoss);
                }
            }
        }

        public void CheckPendingOrders(IRateInfo rate, List<ActivationRecord> result)
        {
            if (Count > 0)
            {
                if (rate.HasAsk)
                {
                    buyLimitIndex.CheckPendingOrders(rate, result);
                    buyStopIndex.CheckPendingOrders(rate, result);
                    buyStopLimitIndex.CheckPendingOrders(rate, result);
                }

                if (rate.HasBid)
                {
                    sellLimitIndex.CheckPendingOrders(rate, result);
                    sellStopIndex.CheckPendingOrders(rate, result);
                    sellStopLimitIndex.CheckPendingOrders(rate, result);
                }
            }
        }

        private class AscendingPriceComparer : IComparer<double>
        {
            public int Compare(double x, double y)
            {
                return x.CompareTo(y);
            }
        }

        private class DescendingPriceComparer : IComparer<double>
        {
            public int Compare(double x, double y)
            {
                return y.CompareTo(x);
            }
        }
    }
}
