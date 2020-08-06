﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Api;
using TickTrader.Algo.Domain;

namespace TickTrader.Algo.Core
{
    [Serializable]
    public class TradeResultEntity
    {
        public TradeResultEntity(Domain.OrderExecReport.Types.CmdResultCode code, Domain.OrderInfo entity)
        {
            ResultCode = code;
            ResultingOrder = entity;
            if (entity != null)
                TransactionTime = (entity.Modified ?? entity.Created)?.ToDateTime();
        }

        public Domain.OrderExecReport.Types.CmdResultCode ResultCode { get; private set; }
        public Domain.OrderInfo ResultingOrder { get; private set; }
        public DateTime? TransactionTime { get; private set; }
    }

    public class OrderResultEntity : OrderCmdResult
    {
        public OrderResultEntity(OrderCmdResultCodes code, Order entity, DateTime? trTime)
        {
            this.ResultCode = code;
            if (entity != null)
                this.ResultingOrder = entity;
            else
                this.ResultingOrder = OrderEntity.Null;
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
