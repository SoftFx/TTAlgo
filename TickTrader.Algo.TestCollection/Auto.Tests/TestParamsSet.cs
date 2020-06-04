using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Api;

namespace TickTrader.Algo.TestCollection.Auto.Tests
{
    public enum OrderExecutionMode { Execution, Waiting}

    public enum TestAcion { Fill, FillByModify, RejectIoC, TP, SL, Cancel, Expiration, ADReject, ADPartialActivate,
                            CloseByBigSmall, CloseBySmallBig, CloseByEven}

    public enum TestOrderAction { Open, Modify, Close, Cancel, CloseBy };

    public enum TestPropertyAction { Add, Modify, Delete };

    public class TestParamsSet
    {
        public const string Tag = "TAG";


        public static AccountTypes AccountType { get; set; }

        public static OrderList Orders { get; set; }


        public OrderType Type { get; protected set; }

        public OrderSide Side { get; protected set; }

        public bool Async { get; private set; }

        public OrderExecOptions Options { get; set; }

        
        public bool IsInstantOrder => Type == OrderType.Market || (Type != OrderType.StopLimit && Options.HasFlag(OrderExecOptions.ImmediateOrCancel));


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
