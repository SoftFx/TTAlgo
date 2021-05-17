using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TickTrader.Algo.Api;
using TickTrader.Algo.Api.Ext;
using TickTrader.Algo.Domain;

namespace TickTrader.Algo.CoreV1.Metadata
{
    public static class PackageMetadataCache
    {
        private static ConcurrentDictionary<string, PackageMetadata> _packageCacheById = new ConcurrentDictionary<string, PackageMetadata>();


        public static PluginMetadata GetPlugin(PluginKey key)
        {
            _packageCacheById.TryGetValue(key.PackageId, out var package);
            return package?.Plugins.FirstOrDefault(p => p.Id == key.DescriptorId);
        }

        public static ReductionMetadata GetReduction(ReductionKey key)
        {
            _packageCacheById.TryGetValue(key.PackageId, out var package);
            return package?.Reductions.FirstOrDefault(r => r.Id == key.DescriptorId);
        }


        public static PackageMetadata ExamineAssembly(string packageId, Assembly targetAssembly)
        {
            if (_packageCacheById.ContainsKey(packageId))
                throw new AlgoException($"Package '{packageId}' has been already cached");

            var plugins = FindPlugins(targetAssembly);
            var reductions = FindReductions(targetAssembly);

            var metadata = new PackageMetadata(packageId, plugins, reductions);
            _packageCacheById.TryAdd(packageId, metadata);
            return metadata;
        }
        

        private static List<PluginMetadata> FindPlugins(Assembly targetAssembly)
        {
            var plugins = new List<PluginMetadata>();

            foreach (var t in targetAssembly.GetTypes())
            {
                var indicatorAttr = t.GetCustomAttribute<IndicatorAttribute>(false);
                var botAttr = t.GetCustomAttribute<TradeBotAttribute>(false);

                if (indicatorAttr != null && botAttr != null)
                    continue;

                var metadata = new PluginMetadata(t);
                plugins.Add(metadata);
            }

            return plugins;
        }

        private static List<ReductionMetadata> FindReductions(Assembly targetAssembly)
        {
            var reductions = new List<ReductionMetadata>();

            foreach (var t in targetAssembly.GetTypes())
            {
                var reductionAttr = t.GetCustomAttribute<ReductionAttribute>();
                if (reductionAttr != null)
                {
                    var metadata = new ReductionMetadata(t, reductionAttr);
                    reductions.Add(metadata);
                }
            }

            return reductions;
        }
    }
}
