using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using TickTrader.Algo.Api;
using TickTrader.Algo.Api.Ext;

namespace TickTrader.Algo.Core.Metadata
{
    public static class AlgoAssemblyInspector
    {
        private static ConcurrentDictionary<string, PluginMetadata> PluginCacheByName = new ConcurrentDictionary<string, PluginMetadata>();
        private static ConcurrentDictionary<string, ReductionMetadata> ReductionCacheByName = new ConcurrentDictionary<string, ReductionMetadata>();


        public static PluginMetadata GetPlugin(Type algoCustomType)
        {
            if (!PluginCacheByName.TryGetValue(algoCustomType.FullName, out var metadata))
            {
                metadata = new PluginMetadata(algoCustomType);
                PluginCacheByName.TryAdd(algoCustomType.FullName, metadata);
            }

            return metadata;
        }

        public static ReductionMetadata GetReduction(Type algoCustomType, ReductionAttribute reductionAttr)
        {
            if (!ReductionCacheByName.TryGetValue(algoCustomType.FullName, out var metadata))
            {
                metadata = new ReductionMetadata(algoCustomType, reductionAttr);
                ReductionCacheByName.TryAdd(algoCustomType.FullName, metadata);
            }

            return metadata;
        }

        public static PluginMetadata GetPlugin(string pluginId)
        {
            PluginCacheByName.TryGetValue(pluginId, out var result);
            return result;
        }

        public static ReductionMetadata GetReduction(string reductionId)
        {
            ReductionCacheByName.TryGetValue(reductionId, out var result);
            return result;
        }

        public static IEnumerable<PluginMetadata> FindPlugins(Assembly targetAssembly)
        {
            var plugins = new List<PluginMetadata>();

            foreach (var t in targetAssembly.GetTypes())
            {
                var indicatorAttr = t.GetCustomAttribute<IndicatorAttribute>(false);
                var botAttr = t.GetCustomAttribute<TradeBotAttribute>(false);

                if (indicatorAttr != null && botAttr != null)
                    continue;

                if (indicatorAttr != null)
                {
                    var metadata = GetPlugin(t);
                    if (metadata.Descriptor.Type == AlgoTypes.Indicator)
                        plugins.Add(metadata);
                }
                else if (botAttr != null)
                {
                    var metadata = GetPlugin(t);
                    if (metadata.Descriptor.Type == AlgoTypes.Robot)
                        plugins.Add(metadata);
                }
            }

            return plugins;
        }

        public static IEnumerable<ReductionMetadata> FindReductions(Assembly targetAssembly)
        {
            var reductions = new List<ReductionMetadata>();

            foreach (var t in targetAssembly.GetTypes())
            {
                var reductionAttr = t.GetCustomAttribute<ReductionAttribute>();
                if (reductionAttr != null)
                {
                    var metadata = GetReduction(t, reductionAttr);
                    reductions.Add(new ReductionMetadata(t, reductionAttr));
                }
            }

            return reductions;
        }
    }
}
