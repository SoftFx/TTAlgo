using System;
using TickTrader.Algo.Api;

namespace TickTrader.Algo.CoreV1
{
    public class OrderResultEntity : OrderCmdResult
    {
        public OrderResultEntity(OrderCmdResultCodes code, Order entity, DateTime? trTime)
        {
            ResultCode = code;
            if (entity != null)
                ResultingOrder = entity;
            else
                ResultingOrder = Null.Order;
            IsServerResponse = true;
            TransactionTime = trTime ?? DateTime.MinValue;
        }

        public bool IsCompleted { get { return ResultCode == OrderCmdResultCodes.Ok; } }
        public bool IsFaulted { get { return ResultCode != OrderCmdResultCodes.Ok; } }
        public OrderCmdResultCodes ResultCode { get; }
        public Order ResultingOrder { get; }
        public bool IsServerResponse { get; set; }
        public DateTime TransactionTime { get; }
    }
}
