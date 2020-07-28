using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Api;

namespace TickTrader.Algo.TestCollection.Auto.Tests
{
    public enum OrderExecutionMode { Execution, Waiting }

    public enum TestAcion
    {
        Fill, FillByModify, RejectIoC, ExecutionTP, ExecutionSL, Cancel, Expiration, ADReject, ADPartialActivate,
        CloseByBigSmall, CloseBySmallBig, CloseByEven, OpenSlippage, PartialActiveWithSlippage
    }

    public enum TestOrderAction { Open, Modify, Close, Cancel, CloseBy };

    public enum TestPropertyAction { Add, Modify, Delete };

    public class TestParamsSet
    {
        public const string Tag = "TAG";

        public static Symbol Symbol { get; set; }

        public static AccountTypes AccountType { get; set; }

        public static OrderList Orders { get; set; }


        public OrderType Type { get; protected set; }

        public OrderSide Side { get; protected set; }

        public bool Async { get; private set; }

        public OrderExecOptions Options { get; set; }


        public bool IsInstantOrder => (Type == OrderType.Market && AccountType != AccountTypes.Gross)
                                   || (Type == OrderType.Limit && Options.HasFlag(OrderExecOptions.ImmediateOrCancel));

        public bool IsLimitIoC => Type == OrderType.Limit && Options.HasFlag(OrderExecOptions.ImmediateOrCancel);

        public bool IsSlippageSupported => Type == OrderType.Stop || Type == OrderType.Market;

        public bool IsLimit => Type == OrderType.StopLimit || Type == OrderType.Limit;

        public TestParamsSet() { }

        public TestParamsSet(OrderType type, OrderSide side, bool async = false)
        {
            Type = type;
            Side = side;
            Async = async;
        }

        public bool SwitchAsyncMode()
        {
            Async = !Async;
            return Async;
        }

        public string Info(TestAcion action) => $"{(Async ? "Async " : "")}{action} {Side} {Type} order (Tag: {Tag}, options: {Options})";

        protected virtual string GetInfo() => $"{(Async ? "Async " : "")}{TestOrderAction.Open} {Side} {Type} order (Tag: {Tag}, options: {Options})";

    }
}
