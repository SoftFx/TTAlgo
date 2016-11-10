using SoftFX.Extended;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.BusinessObjects;

namespace TickTrader.BotTerminal
{
    enum AggregatedOrderType { Unknown, Buy, BuyLimit, BuyStop, Sell, SellLimit, SellStop }

    static class OrderCommontExtensions
    {
        public static AggregatedOrderType Aggregate(this TradeRecordSide side, TradeRecordType type)
        {
            switch (type)
            {
                case TradeRecordType.Market:
                case TradeRecordType.Position:
                    return side == TradeRecordSide.Buy ? AggregatedOrderType.Buy : AggregatedOrderType.Sell;
                case TradeRecordType.Limit:
                    return side == TradeRecordSide.Buy ? AggregatedOrderType.BuyLimit : AggregatedOrderType.SellLimit;
                case TradeRecordType.Stop:
                    return side == TradeRecordSide.Buy ? AggregatedOrderType.BuyStop : AggregatedOrderType.SellStop;
                default: return AggregatedOrderType.Unknown;
            }
        }

        public static AggregatedOrderType Aggregate(this TradeRecordType type, TradeRecordSide side)
        {
            switch (type)
            {
                case TradeRecordType.Market:
                case TradeRecordType.Position:
                    return side == TradeRecordSide.Buy ? AggregatedOrderType.Buy : AggregatedOrderType.Sell;
                case TradeRecordType.Limit:
                    return side == TradeRecordSide.Buy ? AggregatedOrderType.BuyLimit : AggregatedOrderType.SellLimit;
                case TradeRecordType.Stop:
                    return side == TradeRecordSide.Buy ? AggregatedOrderType.BuyStop : AggregatedOrderType.SellStop;
                default: return AggregatedOrderType.Unknown;
            }
        }
    }
}
