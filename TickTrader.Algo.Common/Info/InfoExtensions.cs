using System.Linq;
using TickTrader.Algo.Common.Model;
using TickTrader.Algo.Core;
using TickTrader.Algo.Core.Repository;

namespace TickTrader.Algo.Common.Info
{
    public static class InfoExtensions
    {
        public static PackageKey GetKey(this AlgoPackageRef packageRef)
        {
            return new PackageKey(packageRef.Name, packageRef.Location);
        }

        public static PluginKey GetKey(this AlgoPluginRef pluginRef, AlgoPackageRef packageRef)
        {
            return new PluginKey(packageRef.Name, packageRef.Location, pluginRef.Id);
        }

        public static PluginKey GetKey(this AlgoPluginRef pluginRef, PackageKey packageKey)
        {
            return new PluginKey(packageKey, pluginRef.Id);
        }


        public static PackageInfo ToInfo(this AlgoPackageRef packageRef)
        {
            return new PackageInfo
            {
                Key = new PackageKey(packageRef.Name, packageRef.Location),
                Identity = packageRef.Identity,
                IsValid = packageRef.IsValid,
                Plugins = packageRef.GetPluginRefs().Select(r => r.ToInfo(packageRef)).ToList(),
                IsLocked = packageRef.IsLocked,
                IsObsolete = packageRef.IsObsolete,
            };
        }

        public static PluginInfo ToInfo(this AlgoPluginRef pluginRef, AlgoPackageRef packageRef)
        {
            return new PluginInfo
            {
                Key = pluginRef.GetKey(packageRef),
                Descriptor = pluginRef.Metadata.Descriptor,
            };
        }

        public static PluginInfo ToInfo(this AlgoPluginRef pluginRef, PackageKey packageKey)
        {
            return new PluginInfo
            {
                Key = pluginRef.GetKey(packageKey),
                Descriptor = pluginRef.Metadata.Descriptor,
            };
        }

        public static MappingInfo ToInfo(this Mapping mapping)
        {
            return new MappingInfo
            {
                Key = mapping.Key,
                DisplayName = mapping.DisplayName,
            };
        }

        public static MappingCollectionInfo ToInfo(this MappingCollection mappings)
        {
            return new MappingCollectionInfo
            {
                BarToBarMappings = mappings.BarToBarMappings.Values.Select(ToInfo).ToList(),
                BarToDoubleMappings = mappings.BarToDoubleMappings.Values.Select(ToInfo).ToList(),
                QuoteToBarMappings = mappings.QuoteToBarMappings.Values.Select(ToInfo).ToList(),
                QuoteToDoubleMappings = mappings.QuoteToDoubleMappings.Values.Select(ToInfo).ToList(),
                DefaultFullBarToBarReduction = MappingCollection.DefaultFullBarToBarReduction,
                DefaultBarToDoubleReduction = MappingCollection.DefaultBarToDoubleReduction,
                DefaultFullBarToDoubleReduction = MappingCollection.DefaultFullBarToDoubleReduction,
                DefaultQuoteToBarReduction = MappingCollection.DefaultQuoteToBarReduction,
                DefaultQuoteToDoubleReduction = MappingCollection.DefaultQuoteToDoubleReduction,
            };
        }

        public static SymbolKey ToInfo(this Domain.SymbolInfo symbol)
        {
            return new SymbolKey(symbol.Name, SymbolOrigin.Online);
        }

        public static SymbolKey ToInfo(this SymbolModel symbol)
        {
            return new SymbolKey(symbol.Name, SymbolOrigin.Online);
        }
    }
}
