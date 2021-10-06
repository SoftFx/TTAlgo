using System;
using TickTrader.Algo.Api;
using TickTrader.Algo.Domain;

namespace TickTrader.Algo.CoreV1
{
    internal sealed class TriggerReportAdapter : TriggerReport
    {
        private readonly double _lotSize;


        public TriggerReportInfo Info { get; }

        public TriggerReportAdapter(TriggerReportInfo info, ISymbolInfo symbol)
        {
            Info = info;
            _lotSize = symbol?.LotSize ?? 1;
        }

        public string ContingentOrderId => Info.ContingentOrderId;

        public DateTime TransactionTime => Info.TransactionTime.ToDateTime();

        public DateTime? TriggerTime => Info.TriggerTime?.ToDateTime();

        public Api.ContingentOrderTrigger.TriggerType TriggerType => Info.TriggerType.ToApi();

        public TriggerResultState TriggerState => Info.TriggerState.ToApi();

        public string OrderIdTriggeredBy => Info.OrderIdTriggeredBy;

        public string Symbol => Info.Symbol;

        public OrderType Type => Info.Type.ToApiEnum();

        public OrderSide Side => Info.Side.ToApiEnum();

        public double? Price => Info.Price;

        public double? StopPrice => Info.StopPrice;

        public double Volume => Info.Amount / _lotSize;

        public string RelatedOrderId => Info.RelatedOrderId;
    }
}
