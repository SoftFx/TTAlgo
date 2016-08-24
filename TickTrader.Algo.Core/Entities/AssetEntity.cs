using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.Algo.Core
{
    [Serializable]
    public class AssetEntity : Api.Asset
    {
        public AssetEntity()
        {
        }

        public AssetEntity(double balance, string currency)
        {
            this.CurrencyCode = currency;
            this.Volume = balance;
        }

        public string CurrencyCode { get; set; }
        public double Volume { get; set; }
    }
}
