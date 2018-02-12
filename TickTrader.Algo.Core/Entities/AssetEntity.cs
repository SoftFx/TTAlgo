using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Api;

namespace TickTrader.Algo.Core
{
    [Serializable]
    public class AssetEntity 
    {
        public AssetEntity(double balance, string currency)
        {
            this.Currency = currency;
            this.Volume = balance;   
        }

        public string Currency { get; set; }
        public double Volume { get; set; }
        public double TradeVolume { get; set; }

        public bool IsEmpty => Volume == 0;
    }
}
