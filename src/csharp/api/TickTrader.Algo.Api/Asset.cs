using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.Algo.Api
{
    public interface Asset
    {
        string Currency { get; }
        Currency CurrencyInfo { get; }
        double Volume { get; }
        double LockedVolume { get; }
        double FreeVolume { get; }
        bool IsNull { get; }
    }

    public interface AssetList : IEnumerable<Asset>
    {
        int Count { get; }

        Asset this[string currency] { get; }

        event Action<AssetModifiedEventArgs> Modified;
    }

    public interface AssetModifiedEventArgs
    {
        Asset NewAsset { get; }
    }
}
