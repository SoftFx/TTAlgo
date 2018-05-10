using System.Linq;
using TickTrader.Algo.Core.Metadata;

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
                CreatedUtc = packageRef.CreatedUtc,
                Plugins = packageRef.GetPluginRefs().Select(r => r.ToInfo(packageRef)).ToList(),
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
    }
}
