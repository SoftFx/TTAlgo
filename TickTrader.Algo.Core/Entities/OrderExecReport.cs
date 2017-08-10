using System;
using System.Collections.Generic;
using TickTrader.Algo.Api;

namespace TickTrader.Algo.Core
{
    [Serializable]
    public class OrderExecReport
    {
        public string OrderId { get; set; }
        public OrderEntity OrderCopy { get; set; }
        public double? NewBalance { get; set; }
        public OrderExecAction ExecAction { get; set; }
        public OrderEntityAction Action { get; set; }
        public List<AssetEntity> Assets { get; set; }
        public OrderCmdResultCodes ResultCode { get; set; }
        public string OperationId { get; set; }
        public bool IsCompleted => ResultCode == OrderCmdResultCodes.Ok;
    }

    public enum OrderExecAction { Opened, Modified, Canceled, Closed, Filled, Expired, Rejected, Activated }
    public enum OrderEntityAction { None, Added, Removed, Updated }
}
