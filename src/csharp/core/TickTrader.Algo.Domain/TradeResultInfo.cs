using System;

namespace TickTrader.Algo.Domain
{
    public partial class TradeResultInfo
    {
        public DateTime? TransactionTime => (ResultingOrder?.Modified ?? ResultingOrder?.Created)?.ToDateTime();


        public TradeResultInfo(OrderExecReport.Types.CmdResultCode resultCode, OrderInfo resultingOrder)
        {
            ResultCode = resultCode;
            ResultingOrder = resultingOrder;
        }
    }
}
