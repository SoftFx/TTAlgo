using System;
using System.Collections.Generic;
using System.Reflection;
using TickTrader.Algo.Api;
using TickTrader.Algo.Api.Ext;

namespace TickTrader.Algo.Core.Metadata
{
    public static class AlgoAssemblyInspector
    {
        private static Dictionary<Type, PluginMetadata> CacheByType = new Dictionary<Type, PluginMetadata>();
        private static Dictionary<string, PluginMetadata> CacheByName = new Dictionary<string, PluginMetadata>();


        public static PluginMetadata GetPlugin(Type algoCustomType)
        {
            if (!CacheByType.TryGetValue(algoCustomType, out var metadata))
            {
                metadata = new PluginMetadata(algoCustomType);
                CacheByType.Add(algoCustomType, metadata);
                CacheByName.Add(algoCustomType.FullName, metadata);
            }

            return metadata;
        }

        public static PluginMetadata GetPlugin(string pluginId)
        {
            CacheByName.TryGetValue(pluginId, out var result);
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
                    reductions.Add(new ReductionMetadata(t, reductionAttr));
            }

            return reductions;
        }
    }
}
