using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SoftFX.Extended.Reports;
using SoftFX.Extended;

namespace TickTrader.BotTerminal
{
    internal class TradeTransactionModel
    {
        public enum TradeSide { Deposit = -1, Buy = 1, Sell = 2 }

        public TradeTransactionModel(TradeTransactionReport item)
        {
            this.TradeRecordSide = (TradeSide)item.TradeRecordSide;


            if (this.TradeRecordSide != TradeSide.Deposit)
            {
                this.Quantity = item.Quantity;
                this.PosOpenPrice = item.PosOpenPrice;
                this.StopLoss = item.StopLoss;
                this.TakeProfit = item.TakeProfit;
                this.PositionLastQuantity = item.PositionLastQuantity;
                this.PositionClosePrice = item.PositionClosePrice;
                this.PositionLeavesQuantity = item.PositionLeavesQuantity;
                this.Commission = item.Commission;
                this.Swap = item.Swap;
            }

            this.AccountBalance = item.AccountBalance;
            this.ActionId = item.ActionId;
            this.AgentCommission = item.AgentCommission;
            this.ClientId = item.ClientId;
            this.CloseConversionRate = item.CloseConversionRate;
            this.CommCurrency = item.CommCurrency;
            this.Comment = item.Comment;
            this.Id = item.Id;
            this.LeavesQuantity = item.LeavesQuantity;
            this.OpenConversionRate = item.OpenConversionRate;
            this.OrderCreated = ConvertToNullOrLocal(item.OrderCreated);
            this.OrderFillPrice = item.OrderFillPrice;
            this.OrderLastFillAmount = item.OrderLastFillAmount;
            this.OrderModified = ConvertToNullOrLocal(item.OrderModified);
            this.PositionClosed = ConvertToNullOrLocal(item.PositionClosed);
            this.PositionCloseRequestedPrice = item.PositionCloseRequestedPrice;
            this.PositionId = item.PositionId;
            this.PositionModified = ConvertToNullOrLocal(item.PositionModified);
            this.PositionOpened = ConvertToNullOrLocal(item.PositionOpened);
            this.PositionQuantity = item.PositionQuantity;
            this.PosOpenReqPrice = item.PosOpenReqPrice;
            this.PosRemainingPrice = item.PosRemainingPrice;
            this.PosRemainingSide = item.PosRemainingSide;
            this.Price = item.Price;
            this.StopPrice = item.StopPrice;
            this.Symbol = item.Symbol;
            this.TradeRecordType = item.TradeRecordType;
            this.TradeTransactionReason = item.TradeTransactionReason;
            this.TradeTransactionReportType = item.TradeTransactionReportType;
            this.TransactionAmount = item.TransactionAmount;
            this.TransactionCurrency = item.TransactionCurrency;
            this.TransactionTime = item.TransactionTime.ToLocalTime();

        }
       

        public double AccountBalance { get; }
        public int ActionId { get; }
        public double? ActualOpenPrice { get { return PosOpenPrice == 0d ? Price : PosOpenPrice; } }
        public double GrossPL { get { return TransactionAmount - (Swap ?? 0d) - (Commission ?? 0d); } }
        public double AgentCommission { get; }
        public string ClientId { get; }
        public double? CloseConversionRate { get; }
        public string CommCurrency { get; }
        public string Comment { get; }
        public double? Commission { get; }
        public string Id { get; }
        public double LeavesQuantity { get; }
        public double? OpenConversionRate { get; }
        public DateTime? OrderCreated { get; }
        public double? OrderFillPrice { get; }
        public double? OrderLastFillAmount { get; }
        public DateTime? OrderModified { get; }
        public DateTime? PositionClosed { get; }
        public double? PositionClosePrice { get; }
        public double PositionCloseRequestedPrice { get; }
        public string PositionId { get; }
        public double? PositionLastQuantity { get; }
        public double? PositionLeavesQuantity { get; }
        public DateTime? PositionModified { get; }
        public DateTime? PositionOpened { get; }
        public double PositionQuantity { get; }
        public double? PosOpenPrice { get; }
        public double PosOpenReqPrice { get; }
        public double? PosRemainingPrice { get; }
        public TradeRecordSide PosRemainingSide { get; }
        public double Price { get; }
        public double? Quantity { get; }
        public double? StopLoss { get; }
        public double StopPrice { get; }
        public double? Swap { get; }
        public string Symbol { get; }
        public double? TakeProfit { get; }
        public TradeSide TradeRecordSide { get; }
        public TradeRecordType TradeRecordType { get; }
        public TradeTransactionReason TradeTransactionReason { get; }
        public TradeTransactionReportType TradeTransactionReportType { get; }
        public double TransactionAmount { get; }
        public string TransactionCurrency { get; }
        public DateTime TransactionTime { get; }

        private DateTime? ConvertToNullOrLocal(DateTime dateTime)
        {
            if (dateTime.Year == 1970 && dateTime.Month == 1 && dateTime.Day == 1)
                return null;

            return dateTime.ToLocalTime();
        }
    }
}
