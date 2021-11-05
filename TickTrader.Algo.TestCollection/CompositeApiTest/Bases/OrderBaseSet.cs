using TickTrader.Algo.Api;

namespace TickTrader.Algo.TestCollection.CompositeApiTest
{
    internal class OrderBaseSet
    {
        public static Symbol Symbol { get; private set; }

        public static OrderList Orders { get; private set; }

        public static AccountTypes AccountType { get; private set; }

        public static double BaseOrderVolume { get; private set; }

        public static double PriceDelta { get; private set; }


        public OrderType Type { get; protected set; }

        public OrderSide Side { get; protected set; }

        public OrderExecOptions Options { get; set; }


        public bool IsGrossAcc => AccountType == AccountTypes.Gross;


        public bool IsInstantOrder => Type == OrderType.Market || IsLimitIoC;

        public bool IsLimitIoC => Type == OrderType.Limit && Options.HasFlag(OrderExecOptions.ImmediateOrCancel);

        public bool IsSupportedMaxVisibleVolume => Type != OrderType.Stop && Type != OrderType.Position;

        public bool IsSupportedSlippage => Type == OrderType.Stop || Type == OrderType.Market;

        public bool IsSupportedStopPrice => Type == OrderType.StopLimit || Type == OrderType.Stop;

        public bool IsSupportedIoC => Type == OrderType.StopLimit || Type == OrderType.Limit;

        public bool IsSupportedRejectedIoC => Type == OrderType.Limit;

        public bool IsSupportedOCO => (Type == OrderType.Stop || Type == OrderType.Limit) && !IsGrossAcc;


        public bool IsStopLimit => Type == OrderType.StopLimit;

        public bool IsPosition => Type == OrderType.Position;


        public OrderBaseSet() { }

        public OrderBaseSet(OrderType type, OrderSide side)
        {
            Type = type;
            Side = side;
        }


        public static void FillBaseParameters(CompositeTradeApiTest bot)
        {
            BaseOrderVolume = bot.DefaultOrderVolume;
            AccountType = bot.Account.Type;
            PriceDelta = bot.PriceDelta;
            Orders = bot.Account.Orders;
            Symbol = bot.Symbol;
        }


        public OrderStateTemplate BuildOrder(OrderType? type = null, double? newVolume = null)
        {
            var template = new OrderStateTemplate(this, newVolume ?? BaseOrderVolume)
            {
                Type = type ?? Type,
            };

            return template;
        }


        public override string ToString() => $"Type={Type} Side={Side}";
    }
}