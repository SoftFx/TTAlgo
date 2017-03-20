using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Api;

namespace TickTrader.Algo.Core
{
    public class AssetsCollection
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

        public void Update(AssetEntity entity)
        {
            builder.InvokePluginMethod(() => fixture.Update(entity));
        }

        public void Remove(string currencyCode)
        {
            builder.InvokePluginMethod(() => fixture.Remove(currencyCode));
        }

        internal class AssetsFixture : AssetList
        {
            private Dictionary<string, Asset> assets = new Dictionary<string, Asset>();

            public int Count { get { return assets.Count; } }

            public Asset this[string currencyCode]
            {
                get
                {
                    if (string.IsNullOrEmpty(currencyCode))
                        return Null.Asset;

                    Asset asset;
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

            public void Update(AssetEntity entity)
            {
                assets[entity.Currency] = entity;
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
}
