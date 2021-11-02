using System;
using TickTrader.Algo.Api;
using TickTrader.Algo.Core;
using TickTrader.Algo.Domain;

namespace TickTrader.Algo.CoreV1
{
    internal sealed class AssetsCollection : TradeEntityCollection<AssetAccessor, Asset>, AssetList
    {
        public AssetsCollection(PluginBuilder builder) : base(builder) { }

        public Asset this[string currencyCode] => _entities.TryGetValue(currencyCode, out AssetAccessor asset) ? (Asset)asset : new NullAsset(currencyCode);

        public AssetAccessor Update(Domain.AssetInfo info, out AssetChangeType cType)
        {
            if (!_entities.TryGetValue(info.Currency, out var asset))
            {
                asset = new AssetAccessor(info, _builder.Currencies);
                _entities.TryAdd(info.Currency, asset);
                cType = AssetChangeType.Added;
            }
            else
            if (info.Balance <= 0)
                cType = _entities.TryRemove(info.Currency, out _) ? AssetChangeType.Removed : AssetChangeType.NoChanges;
            else
                cType = asset.Update(info) ? AssetChangeType.Updated : AssetChangeType.NoChanges;

            if (cType != AssetChangeType.NoChanges)
                AssetChanged?.Invoke(asset.Info, cType);

            return asset;
        }

        public event Action<AssetInfo, AssetChangeType> AssetChanged;
        public event Action<AssetModifiedEventArgs> Modified;

        public void FireModified(AssetModifiedEventArgs args) => _builder.InvokePluginMethod((b, p) => Modified?.Invoke(p), args);

        #region Emulation

        public AssetAccessor GetOrAdd(string currency, out AssetChangeType cType)
        {
            if (!_entities.TryGetValue(currency, out AssetAccessor asset))
            {
                asset = new AssetAccessor(new Domain.AssetInfo(0, currency), _builder.Currencies);
                _entities.TryAdd(currency, asset);
                cType = AssetChangeType.Added;
            }
            else
                cType = AssetChangeType.Updated;

            return asset;
        }

        #endregion
    }
}
