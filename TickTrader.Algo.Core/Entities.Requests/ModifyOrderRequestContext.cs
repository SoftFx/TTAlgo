using Google.Protobuf.WellKnownTypes;
using System;

namespace TickTrader.Algo.Core
{
    public class ModifyOrderRequestContext
    {
        public Domain.ModifyOrderRequest Request { get; } = new Domain.ModifyOrderRequest();

        public string OrderId
        {
            get { return Request.OrderId; }
            set { Request.OrderId = value; }
        }

        public string Symbol
        {
            get { return Request.Symbol; }
            set { Request.Symbol = value; }
        }

        public Domain.OrderInfo.Types.Type Type
        {
            get { return Request.Type; }
            set { Request.Type = value; }
        }

        public Domain.OrderInfo.Types.Side Side
        {
            get { return Request.Side; }
            set { Request.Side = value; }
        }

        public double CurrentAmount
        {
            get { return Request.CurrentAmount; }
            set { Request.CurrentAmount = value; }
        }

        public double? NewAmount
        {
            get { return Request.NewAmount; }
            set { Request.NewAmount = value; }
        }

        public double AmountChange
        {
            get { return Request.AmountChange; }
            set { Request.AmountChange = value; }
        }

        public double? MaxVisibleAmount
        {
            get { return Request.MaxVisibleAmount; }
            set { Request.MaxVisibleAmount = value; }
        }

        public double? Price
        {
            get { return Request.Price; }
            set { Request.Price = value; }
        }

        public double? StopPrice
        {
            get { return Request.StopPrice; }
            set { Request.StopPrice = value; }
        }

        public double? StopLoss
        {
            get { return Request.StopLoss; }
            set { Request.StopLoss = value; }
        }

        public double? TakeProfit
        {
            get { return Request.TakeProfit; }
            set { Request.TakeProfit = value; }
        }

        public double? Slippage
        {
            get { return Request.Slippage; }
            set { Request.Slippage = value; }
        }

        public string Comment
        {
            get { return Request.Comment; }
            set { Request.Comment = value; }
        }

        public string Tag
        {
            get { return Request.Tag; }
            set { Request.Tag = value; }
        }

        public DateTime? Expiration
        {
            get { return Request.Expiration?.ToDateTime(); }
            set { Request.Expiration = value?.ToUniversalTime().ToTimestamp(); }
        }

        public Domain.OrderExecOptions? ExecOptions
        {
            get { return Request.ExecOptions; }
            set { Request.ExecOptions = value; }
        }

        public double CurrentVolume { get; set; }
        public double? NewVolume { get; set; }
        public double? MaxVisibleVolume { get; set; }
    }
}
