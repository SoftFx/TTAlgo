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
            builder.InvokePluginMethod(() => fixture.FireModified(args));
        }

        public AssetAccessor Update(AssetEntity entity, Dictionary<string, Currency> currencies)
        {
            AssetChangeType cType;
            var result = fixture.Update(entity, currencies, out cType);
            AssetChanged?.Invoke(result, cType);
            return result;
        }

        public void Remove(string currencyCode)
        {
            fixture.Remove(currencyCode);
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

            public event Action<AssetModifiedEventArgs> Modified = delegate { };

            public AssetAccessor Update(AssetEntity entity, Dictionary<string, Currency> currencies, out AssetChangeType cType)
            {
                AssetAccessor asset;
                if (!assets.TryGetValue(entity.Currency, out asset))
                {
                    asset = new AssetAccessor(entity, currencies);
                    assets.Add(entity.Currency, asset);
                    cType = AssetChangeType.Added;
                }
                else
                    cType = AssetChangeType.Updated;

                if (entity.Volume <= 0)
                {
                    assets.Remove(entity.Currency);
                    cType = AssetChangeType.Removed;
                }

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

    public enum AssetChangeType { Added, Updated, Removed }
}
