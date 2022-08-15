using System;
using TickTrader.Algo.Api;

namespace TickTrader.Algo.TestCollection.CompositeApiTest
{
    internal class OrderBaseSet
    {
        internal static CompositeTradeApiTest Bot { get; set; }


        public static AccountTypes AccountType => Bot.Account.Type;

        public static OrderList Orders => Bot.Account.Orders;

        public static Symbol Symbol => Bot.Symbol;

        public static DateTime UtcNow => Bot.UtcNow;


        public static double BaseOrderVolume => Bot.DefaultOrderVolume;

        public static double PriceDelta => Bot.PriceDelta;


        public OrderType Type { get; protected set; }

        public OrderSide Side { get; protected set; }

        public OrderExecOptions Options { get; set; }


        public bool IsGrossAcc => AccountType == AccountTypes.Gross;


        public bool IsInstantOrder => Type == OrderType.Market || IsLimitIoC;

        public bool IsPendingOrder => !IsInstantOrder;

        public bool IsLimitIoC => Type == OrderType.Limit && Options.HasFlag(OrderExecOptions.ImmediateOrCancel);

        public bool IsSupportedMaxVisibleVolume => IsSupportedIoC;

        public bool IsSupportedSlippage => Type == OrderType.Stop || Type == OrderType.Market;

        public bool IsSupportedStopPrice => Type == OrderType.StopLimit || Type == OrderType.Stop;

        public bool IsSupportedIoC => Type == OrderType.StopLimit || Type == OrderType.Limit;

        public bool IsSupportedRejectedIoC => Type == OrderType.Limit;

        public bool IsSupportedOCO => (Type == OrderType.Stop || Type == OrderType.Limit) && !IsGrossAcc;

        public bool IsSupportedOTO => Type != OrderType.StopLimit;


        public bool IsStopLimit => Type == OrderType.StopLimit;

        public bool IsPosition => Type == OrderType.Position;


        public OrderBaseSet() { }

        public OrderBaseSet(OrderType type, OrderSide side)
        {
            Type = type;
            Side = side;
        }


        public static void InitTemplate(CompositeTradeApiTest bot)
        {
            Bot = bot;
        }


        public OrderStateTemplate BuildOrder(OrderType? type = null, double? newVolume = null)
        {
            var template = new OrderStateTemplate(this, newVolume ?? BaseOrderVolume)
            {
                Type = type ?? Type,
            };

            return template.ForPending();
        }


        public override string ToString() => $"Type={Type} Side={Side}";
    }
}