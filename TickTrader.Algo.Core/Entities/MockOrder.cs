using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Api;

namespace TickTrader.Algo.Core
{
    internal class MockOrder : Order
    {
        public MockOrder(string symbol, OrderType type, OrderSide side)
        {
            Symbol = symbol;
            Type = type;
            Side = side;
        }

        public string Id => "-1";
        public string Symbol { get; }
        public double RequestedVolume { get; set; }
        public double RemainingVolume { get; set; }
        public double MaxVisibleVolume { get; set; }
        public OrderType Type { get; }
        public OrderSide Side { get; }
        public double Price { get; set; }
        public double StopPrice { get; set; }
        public double StopLoss { get; set; }
        public double TakeProfit { get; set; }
        public bool IsNull => false;
        public string Comment { get; set; }
        public string Tag { get; set; }
        public string InstanceId { get; set; }
        public DateTime Expiration { get; set; }
        public DateTime Modified { get; set; }
        public DateTime Created { get; set; }
        public double ExecPrice { get; set; }
        public double ExecVolume { get; set; }
        public double LastFillPrice { get; set; }
        public double LastFillVolume { get; set; }
        public double Margin { get; set; }
        public double Profit { get; set; }
    }
}
