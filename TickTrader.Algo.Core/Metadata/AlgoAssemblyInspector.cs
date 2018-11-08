using System;
using System.Collections.Generic;
using System.Reflection;
using TickTrader.Algo.Api;
using TickTrader.Algo.Api.Ext;

namespace TickTrader.Algo.Core.Metadata
{
    public static class AlgoAssemblyInspector
    {
        private static Dictionary<string, PluginMetadata> PluginCacheByName = new Dictionary<string, PluginMetadata>();
        private static Dictionary<string, ReductionMetadata> ReductionCacheByName = new Dictionary<string, ReductionMetadata>();


        public static PluginMetadata GetPlugin(Type algoCustomType)
        {
            if (!PluginCacheByName.TryGetValue(algoCustomType.FullName, out var metadata))
            {
                metadata = new PluginMetadata(algoCustomType);
                PluginCacheByName.Add(algoCustomType.FullName, metadata);
            }

            return metadata;
        }

        public static ReductionMetadata GetReduction(Type algoCustomType, ReductionAttribute reductionAttr)
        {
            if (!ReductionCacheByName.TryGetValue(algoCustomType.FullName, out var metadata))
            {
                metadata = new ReductionMetadata(algoCustomType, reductionAttr);
                ReductionCacheByName.Add(algoCustomType.FullName, metadata);
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
