﻿using TickTrader.Algo.Domain;
using TickTrader.Algo.Domain.CalculatorInterfaces;

namespace TickTrader.Algo.Calculator.TradeSpecificsCalculators
{
    public sealed class ProfitRequest : IProfitCalculateRequest
    {
        public double Price { get; }

        public double Volume { get; }

        public OrderInfo.Types.Side Side { get; }

        public double? ClosePrice { get; }


        public ProfitRequest(double price, double volume, OrderInfo.Types.Side side, double? closePrice = null)
        {
            Price = price;
            Volume = volume;
            Side = side;
            ClosePrice = closePrice;
        }
    }
}
