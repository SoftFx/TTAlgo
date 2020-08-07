using System;

namespace TickTrader.Algo.Domain
{
    public interface ITradeRequest
    {
        string OperationId { get; set; }

        string OrderId { get; }

        string LogDetails { get; }
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

        public string OrderId => string.Empty;

        public string LogDetails => string.Empty;
    }


    public partial class ModifyOrderRequest : ITradeRequest
    {
        public OrderExecOptions? ExecOptions
        {
            get { return (OrderExecOptions?)ExecOptionsBitmask; }
            set { ExecOptionsBitmask = (int?)value; }
        }

        public string LogDetails => string.Empty;
    }

    public partial class CloseOrderRequest : ITradeRequest
    {
        public string LogDetails => ByOrderId != null ? $"{(Amount.HasValue && Amount != 0 ? Amount.ToString() : "")}" : $"{ByOrderId}";
    }

    public partial class CancelOrderRequest : ITradeRequest
    {
        public string LogDetails => string.Empty;
    }
}
