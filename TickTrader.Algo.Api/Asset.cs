using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.Algo.Api
{
    public interface Asset
    {
        string CurrencyCode { get; }
        double Volume { get; }
    }

    public interface AssetList : IEnumerable<Asset>
    {
        int Count { get; }

        Asset this[string currencyCode] { get; }

        event Action<AssetModifiedEventArgs> Modified;
    }

    public interface AssetModifiedEventArgs
    {
        Asset NewAsset { get; }
    }
}
