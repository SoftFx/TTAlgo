using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Api;

namespace TickTrader.Algo.Core
{
    [Serializable]
    public class TradeReportEntity : TradeReport
    {
        public TradeReportEntity(string id)
        {
            ReportId = id;
        }

        public string ReportId { get; private set; }
        public string OrderId { get; set; }
        public DateTime ReportTime { get; set; }
        public DateTime OpenTime { get; set; }
        public TradeRecordTypes Type { get; set; }
        public TradeExecActions ActionType { get; set; }
        public string Symbol { get; set; }
        public double OpenQuantity { get; set; }
        public double OpenPrice { get; set; }
        public double StopLoss { get; set; }
        public double TakeProfit { get; set; }
        public DateTime CloseTime { get; set; }
        public double CloseQuantity { get; set; }
        public double ClosePrice { get; set; }
        public double RemainingQuantity { get; set; }
        public double Commission { get; set; }
        public string CommissionCurrency { get; set; }
        public double Swap { get; set; }
        public double Balance { get; set; }
        public string Comment { get; set; }
        public double GrossProfitLoss { get; set; }
        public double NetProfitLoss { get; set; }
    }
}
