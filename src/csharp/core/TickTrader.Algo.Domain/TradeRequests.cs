using System;
using System.Collections.Generic;
using System.Linq;

namespace TickTrader.Algo.Domain
{
    public interface ITradeRequest
    {
        string OperationId { get; set; }

        string OrderId { get; }

        string LogDetails { get; }

        List<ITradeRequest> SubRequests { get; }
    }

    [Flags]
    public enum OrderExecOptions
    {
        None = 0,
        ImmediateOrCancel = 1,
        OneCancelsTheOther = 2,
    }


    public partial class OpenOrderRequest : IOrderLogDetailsInfo, ITradeRequest
    {
        public OrderExecOptions ExecOptions
        {
            get { return (OrderExecOptions)ExecOptionsBitmask; }
            set { ExecOptionsBitmask = (int)value; }
        }

        public string OrderId => string.Empty;

        public string LogDetails => string.Empty;

        public List<ITradeRequest> SubRequests => SubOpenRequests.Select(r => (ITradeRequest)r).ToList();

        double? IOrderLogDetailsInfo.Amount => Amount;
    }


    public partial class ModifyOrderRequest : IOrderLogDetailsInfo, ITradeRequest
    {
        public OrderExecOptions? ExecOptions
        {
            get { return (OrderExecOptions?)ExecOptionsBitmask; }
            set { ExecOptionsBitmask = (int?)value; }
        }

        public string LogDetails => string.Empty;

        double? IOrderLogDetailsInfo.Amount => NewAmount ?? CurrentAmount;

        public List<ITradeRequest> SubRequests => null;
    }


    public partial class CloseOrderRequest : ITradeRequest
    {
        public string LogDetails => ByOrderId != null ? $"{(Amount.HasValue && Amount != 0 ? Amount.ToString() : "")}" : $"{ByOrderId}";

        public List<ITradeRequest> SubRequests => null;
    }


    public partial class CancelOrderRequest : ITradeRequest
    {
        public string LogDetails => string.Empty;

        public List<ITradeRequest> SubRequests => null;
    }
}
