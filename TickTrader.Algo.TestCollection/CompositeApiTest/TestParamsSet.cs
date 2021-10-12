using TickTrader.Algo.Api;

namespace TickTrader.Algo.TestCollection.CompositeApiTest
{
    public enum Behavior { Execution, Pending }

    public enum TestAcion
    {
        Fill, FillByModify, RejectIoC, ExecutionTP, ExecutionSL, Cancel, Expiration, ADReject, ADPartialActivate,
        CloseByBigSmall, CloseBySmallBig, CloseByEven, OpenSlippage, PartialActiveWithSlippage, ModifyCancel,
        OpenOCO,
    }

    public enum TestOrderAction { Open, Modify, Close, Cancel, CloseBy };


    public class TestParamsSet
    {
        public static Symbol Symbol { get; private set; }

        public static OrderList Orders { get; private set; }

        public static AccountTypes AccountType { get; private set; }

        public static double BaseOrderVolume { get; private set; }

        public static double PriceDelta { get; private set; }


        public OrderType Type { get; protected set; }

        public OrderSide Side { get; protected set; }

        public bool Async { get; private set; }

        public OrderExecOptions Options { get; set; }


        public bool IsGrossAcc => AccountType == AccountTypes.Gross;


        public bool IsInstantOrder => (Type == OrderType.Market && !IsGrossAcc) || IsLimitIoC;

        public bool IsLimitIoC => Type == OrderType.Limit && Options.HasFlag(OrderExecOptions.ImmediateOrCancel);

        public bool IsSupportedMaxVisibleVolume => Type != OrderType.Stop && Type != OrderType.Position;

        public bool IsSupportedSlippage => Type == OrderType.Stop || Type == OrderType.Market;

        public bool IsSupportedStopPrice => Type == OrderType.StopLimit || Type == OrderType.Stop;

        public bool IsSupportedOCO => (Type == OrderType.Stop || Type == OrderType.Limit) && !IsGrossAcc;

        public bool IsLimit => Type == OrderType.StopLimit || Type == OrderType.Limit;

        public bool IsStopLimit => Type == OrderType.StopLimit;

        public bool IsPosition => Type == OrderType.Position;


        public TestParamsSet() { }

        public TestParamsSet(OrderType type, OrderSide side, bool async = false)
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

        public OrderTemplate BuildOrder()
        {
            return new OrderTemplate(this)
            {
                Volume = BaseOrderVolume,
            };
        }

        public string Info(TestAcion action) => $"{(Async ? "Async " : "")}{action} {Side} {Type} order ({Options})";

        protected virtual string GetInfo() => $"{(Async ? "Async " : "")}{TestOrderAction.Open} {Side} {Type} order (options: {Options})";

        public override string ToString() => $"{nameof(TestParamsSet)}: Type {Type}, Side {Side}";
    }
}
