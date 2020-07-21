using System;

namespace TickTrader.Algo.Domain
{
    public interface ITradeRequest
    {
        string OperationId { get; set; }
    }

    [Flags]
    public enum OrderExecOptions
    {
        None = 0,
        ImmediateOrCancel = 1,
    }


    public partial class OpenOrderRequest : ITradeRequest
    {
        public OrderExecOptions ExecOptions
        {
            get { return (OrderExecOptions)ExecOptionsBitmask; }
            set { ExecOptionsBitmask = (int)value; }
        }
    }


    public partial class ModifyOrderRequest : ITradeRequest
    {
        public OrderExecOptions? ExecOptions
        {
            get { return (OrderExecOptions?)ExecOptionsBitmask; }
            set { ExecOptionsBitmask = (int?)value; }
        }
    }

    public partial class CloseOrderRequest : ITradeRequest
    {
    }

    public partial class CancelOrderRequest : ITradeRequest
    {
    }
}
