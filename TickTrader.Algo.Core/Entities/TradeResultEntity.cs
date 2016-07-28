using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Api;

namespace TickTrader.Algo.Core
{
    [Serializable]
    public class TradeResultEntity : OrderCmdResult
    {
        public TradeResultEntity(OrderCmdResultCodes code, Order entity)
        {
            this.ResultCode = code;
            this.ResultingOrder = entity;
        }

        public bool IsCompleted { get { return ResultCode == OrderCmdResultCodes.Ok; } }
        public bool IsFaulted { get { return ResultCode != OrderCmdResultCodes.Ok; } }
        public OrderCmdResultCodes ResultCode { get; private set; }
        public Order ResultingOrder { get; private set; }
    }
}
