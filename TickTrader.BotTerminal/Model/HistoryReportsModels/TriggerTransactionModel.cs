using TickTrader.Algo.Domain;

namespace TickTrader.BotTerminal
{
    internal sealed class TriggerTransactionModel : BaseTransactionModel
    {
        public TriggerTransactionModel(TriggerReportInfo report, SymbolInfo symbol) : base(symbol)
        {
            TriggeredByOrder = report.OrderIdTriggeredBy;
            OCORelatedOrderId = report.RelatedOrderId;

            Symbol = report.Symbol;
            Type = GetOrderType(report.Type, report.Side);
            TriggerType = GetTriggerType(report.TriggerType);
            TriggerState = GetTriggerState(report.TriggerState);
            ActionType = TradeReportInfo.Types.ReportType.TriggerActivated;

            OpenTime = report.TransactionTime.ToDateTime();
            CloseTime = report.TransactionTime.ToDateTime();
            TriggerTime = report.TriggerTime?.ToDateTime();

            Volume = report.Amount / LotSize;
            OpenPrice = report.Type.IsStop() ? report.StopPrice : report.Price;

            UniqueId = new TradeReportKey(long.Parse(report.ContingentOrderId), null);
        }

        private static TriggerTypes? GetTriggerType(ContingentOrderTrigger.Types.TriggerType triggerType)
        {
            switch (triggerType)
            {
                case ContingentOrderTrigger.Types.TriggerType.OnPendingOrderExpired:
                    return TriggerTypes.OnPendingOrderExpired;
                case ContingentOrderTrigger.Types.TriggerType.OnPendingOrderPartiallyFilled:
                    return TriggerTypes.OnPendingOrderPartiallyFilled;
                case ContingentOrderTrigger.Types.TriggerType.OnTime:
                    return TriggerTypes.OnTime;

                default:
                    return null;
            }
        }

        private static TriggerResult? GetTriggerState(TriggerReportInfo.Types.TriggerResultState state)
        {
            switch (state)
            {
                case TriggerReportInfo.Types.TriggerResultState.Failed:
                    return TriggerResult.Failed;
                case TriggerReportInfo.Types.TriggerResultState.Successful:
                    return TriggerResult.Success;

                default:
                    return null;
            }
        }
    }
}
