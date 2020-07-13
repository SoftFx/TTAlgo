using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Api;

namespace TickTrader.Algo.Core
{
    public class AssetsCollection : IEnumerable<AssetAccessor>
    {
        private PluginBuilder builder;
        private AssetsFixture fixture = new AssetsFixture();

        internal AssetList AssetListImpl { get { return fixture; } }

        public AssetsCollection(PluginBuilder builder)
        {
            this.builder = builder;
        }

        public void FireModified(AssetModifiedEventArgs args)
        {
            builder.InvokePluginMethod((b, p) => fixture.FireModified(p), args);
        }

        public AssetAccessor Update(Domain.AssetInfo info, Dictionary<string, Currency> currencies)
        {
            return Update(info, currencies, out _);
        }

        public AssetAccessor Update(Domain.AssetInfo info, Dictionary<string, Currency> currencies, out AssetChangeType cType)
        {
            var result = fixture.Update(info, builder.Currencies.GetOrDefault, out cType);
            if (cType != AssetChangeType.NoChanges)
                AssetChanged?.Invoke(result, cType);
            return result;
        }

        public void Remove(string currencyCode)
        {
            fixture.Remove(currencyCode);
        }

        public void Clear()
        {
            fixture.Clear();
        }

        public IEnumerator<AssetAccessor> GetEnumerator()
        {
            return fixture.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return fixture.Values.GetEnumerator();
        }

        public event Action<AssetAccessor, AssetChangeType> AssetChanged;

        #region Emulation

        internal AssetAccessor GetOrCreateAsset(string currency, out AssetChangeType cType)
        {
            return fixture.GetOrAdd(currency, builder.Currencies.GetOrDefault, out cType);
        }

        #endregion

        internal class AssetsFixture : AssetList
        {
            private Dictionary<string, AssetAccessor> assets = new Dictionary<string, AssetAccessor>();

            public int Count { get { return assets.Count; } }
            public IEnumerable<AssetAccessor> Values => assets.Values;

            public Asset this[string currencyCode]
            {
                get
                {
                    if (string.IsNullOrEmpty(currencyCode))
                        return Null.Asset;

                    AssetAccessor asset;
                    if (!assets.TryGetValue(currencyCode, out asset))
                        return new NullAsset(currencyCode);
                    return asset;
                }
            }

            public void FireModified(AssetModifiedEventArgs args)
            {
                Modified(args);
            }

            public void Clear()
            {
                assets.Clear();
            }

            public event Action<AssetModifiedEventArgs> Modified = delegate { };

            public AssetAccessor Update(Domain.AssetInfo info, Func<string, Currency> currencyInfoProvider, out AssetChangeType cType)
            {
                AssetAccessor asset;
                if (!assets.TryGetValue(info.Currency, out asset))
                {
                    asset = new AssetAccessor(info, currencyInfoProvider);
                    assets.Add(info.Currency, asset);
                    cType = AssetChangeType.Added;
                }
                else if (info.Balance <= 0)
                {
                    if (assets.Remove(info.Currency))
                        cType = AssetChangeType.Removed;
                    else
                        cType = AssetChangeType.NoChanges;
                }
                else
                {
                    if (asset.Update((decimal)info.Balance))
                        cType = AssetChangeType.Updated;
                    else
                        cType = AssetChangeType.NoChanges;
                }

                return asset;
            }

            public AssetAccessor GetOrAdd(string currency, Func<string, Currency> currencyInfoProvider, out AssetChangeType cType)
            {
                AssetAccessor asset;
                if (!assets.TryGetValue(currency, out asset))
                {
                    asset = new AssetAccessor(new Domain.AssetInfo(0, currency), currencyInfoProvider);
                    assets.Add(currency, asset);
                    cType = AssetChangeType.Added;
                }
                else
                    cType = AssetChangeType.Updated;
                return asset;
            }

            public void Remove(string currencyCode)
            {
                assets.Remove(currencyCode);
            }

            public IEnumerator<Asset> GetEnumerator()
            {
                return this.assets.Values.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return this.assets.Values.GetEnumerator();
            }
        }
    }

    public enum AssetChangeType { NoChanges, Added, Updated, Removed }
}
