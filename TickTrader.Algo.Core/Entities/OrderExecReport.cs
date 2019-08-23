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
        public bool IsIoC => OrderCopy != null && OrderCopy.ImmediateOrCancel;
        public PositionEntity NetPosition { get; set; }
    }

    public enum OrderExecAction {None, Opened, Modified, Canceled, Closed, Filled, Expired, Rejected, Activated, DepositWithdraw }
    public enum OrderEntityAction { None, Added, Removed, Updated }
}
